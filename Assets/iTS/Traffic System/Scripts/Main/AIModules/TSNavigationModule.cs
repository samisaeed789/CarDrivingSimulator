using System;
using System.Collections;
using System.Collections.Generic;
using ITS.AI;
using ITS.Utils;
using UnityEngine;

namespace ITS.AI
{
    public partial class TSTrafficAI
    {
        [Serializable]
        public struct TSNextLaneSelection
        {
            public bool IsNull { get; private set; }
            public TSBaseInfo NextPath;
            public TSLaneInfo NextLane { get; private set; }
            public TSLaneConnector NextConnector { get; private set; }
            public bool IsConnector { get; private set; }

            public TSNextLaneSelection(TSBaseInfo nextPath)
            {
                NextPath = nextPath;
                IsNull = ReferenceEquals(nextPath, null);

                IsConnector = nextPath is TSLaneConnector;
                if (IsConnector)
                {
                    NextConnector = (TSLaneConnector) nextPath;
                    NextLane = null;
                }
                else
                {
                    NextLane = (TSLaneInfo) NextPath;
                    NextConnector = null;
                }
            }
        }

        public struct TSReservedPoints : IEquatable<TSReservedPoints>
        {
            public TSPoints Point { get; private set; }
            public readonly int point;
            public readonly bool isConnector;
            
            public TSReservedPoints(bool _isConnector, int _point, TSPoints _Point)
            {
                isConnector = _isConnector;
                point = _point;
                Point = _Point;
            }

            public void UnReserve(int myID)
            {
                Point.TryUnReservePoint(myID);
            }

            public bool Equals(TSReservedPoints other)
            {
                return Point.Equals(other.Point);
            }

            public override bool Equals(object obj)
            {
                return obj is TSReservedPoints other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = point;
                    hashCode = (hashCode * 397) ^ (isConnector ? 1 : 0);
                    hashCode = (hashCode * 397) ^ Point.GetHashCode();
                    return hashCode;
                }
            }
        }

        public TSPoints CurrentSteerWaypoint => _steeringPathNavigation.CurrentWaypoint;
        public TSPoints PreviousSteerWaypoint => _steeringPathNavigation.Waypoints[_previousSteerWaypointIndex];

        // Public Variables
        //private int _currentSteerWaypointIndex = 0;
        private int _previousSteerWaypointIndex = 0;
        private Vector3 _relativeWaypointPosition = Vector3.zero;
        private Vector3 _relativeWaypointPositionOnCar = Vector3.zero;

        private TSPathNavigation _steeringPathNavigation = new TSPathNavigation();
        //[field: NonSerialized] public TSBaseInfo CurrentLane { get; private set; }
        //[NonSerialized] private readonly List<TSNextLaneSelection> _nextTrackPath = new List<TSNextLaneSelection>(20);
        //private float _currentMaxSpeed = 50f;
        //public TSPoints[] Waypoints => CurrentLane.Points;
        
        [NonSerialized] private TSLaneInfo[] _lanes;
        private int _myID = 0;
        public float RelativeWPosMagnitude { get; set; } = 0;
        //private int _nextTrackIndex = 0;
        [NonSerialized] private readonly Queue<TSLaneConnector> _reservedConnectors = new Queue<TSLaneConnector>(5);
        public Vector3 PointOffset { get; private set; } = Vector3.zero;
        public void SetPointOffset(Vector3 offset)
        {
            PointOffset = offset;
        }

        [field: NonSerialized] public TSLaneInfo LastLaneIndex { get; private set; }

        [field: NonSerialized] private TSReservedPoints? _lastReservedPoint;

        //private Transform _myTransform;
        private bool _isTurning = false;
        [field: NonSerialized] public Queue<TSLaneInfo> OccupiedLanes { get; private set; } = new Queue<TSLaneInfo>(10);
        private readonly object _occupiedLanesLock = new object();
        public float CarOccupationLenght { get; private set; } = 0f;
        public bool HaveLanes => _lanes.Length > 0;
        public int NextTrackPathCount => _steeringPathNavigation.Count;

        public int MyID
        {
            get
            {
                if (_myID == 0)
                {
                    _myID = TSUtils.GetUniqueID();
                }

                return _myID;
            }
        }
        private bool shouldDisable;

        public class TSNavigationModule : TSAIBaseModule
        {
            public override void OnEnable()
            {
                _trafficAI.shouldDisable = false;
            }

            public override void PostInitialize()
            {
                _trafficAI._steeringPathNavigation.NotAbleToMoveToNextPath += CheckIfShouldDisable;
                _trafficAI._steeringPathNavigation.MovedToNextPath += SwitchToNextLane;
            }

            private int _navigationMethodDeepCounter = 0;
            public override void OnFixedUpdate()
            {
                _navigationMethodDeepCounter = 0;
                NavigateToWaypoints();
                CheckReservedPoints();
            }


            private void NavigateToWaypoints()
            {
                _trafficAI.AddNextTrackToPath();
                UpdateRelativePositions();

                if (ShouldIncreaseWaypointIndex() == false || _navigationMethodDeepCounter > 10)
                {
                    return;
                }

                CheckIfShouldTriggerIsTurningEvent();
                UpdateCurrentWaypointIndex();
                _navigationMethodDeepCounter++;
                NavigateToWaypoints();
            }

            private void UpdateCurrentWaypointIndex()
            {
                _trafficAI._previousSteerWaypointIndex = _trafficAI._steeringPathNavigation.CurrentPointIndex;
                if (_trafficAI.OverTaking)
                {
                    _trafficAI._steeringPathNavigation.MoveToPreviousPoint();
                }
                else
                {
                    _trafficAI._steeringPathNavigation.MoveToNextPoint();
                }
            }
            private void SwitchToNextLane()
            {
                _trafficAI._previousSteerWaypointIndex = 0;
                _trafficAI.UpdateCurrentMaxSpeed();
            }

            private void CheckIfShouldDisable()
            {
                if (!_trafficAI.shouldDisable)
                {
                    return;
                }

                _trafficAI.Disable();
            }

            private bool ShouldIncreaseWaypointIndex()
            {
                var width = _trafficAI.CarWidth * 5f;
                var isNear = _trafficAI._relativeWaypointPosition.z < _trafficAI.LookAheadDistance;
                var isInsideWidth = Mathf.Abs(_trafficAI._relativeWaypointPosition.x) < width;
                return isNear && isInsideWidth;
            }

            private void CheckIfShouldTriggerIsTurningEvent()
            {
                if (_trafficAI._steeringPathNavigation.Count <= 1 || _trafficAI._isTurning)
                {
                    return;
                }

                if (_trafficAI._steeringPathNavigation[1].IsConnector == false || _trafficAI._steeringPathNavigation.Waypoints.Length - _trafficAI._steeringPathNavigation.CurrentPointIndex >= 15)
                {
                    return;
                }

                switch (_trafficAI._steeringPathNavigation[1].NextConnector.direction)
                {
                    case TSLaneConnector.Direction.Left:
                        _trafficAI._isTurning = true;
                        _trafficAI._mainThread.Post(state =>  _trafficAI.OnTurnLeft?.Invoke(true), this);
                        break;
                    case TSLaneConnector.Direction.Right:
                        _trafficAI._isTurning = true;
                        _trafficAI._mainThread.Post(state =>  _trafficAI.OnTurnRight?.Invoke(true), this);
                        break;
                }
            }

            private void UpdateRelativePositions()
            {
                _trafficAI._relativeWaypointPosition = _trafficAI.InverseTransformPoint(_trafficAI._position, _trafficAI._rotation,
                    Vector3.one, _trafficAI._steeringPathNavigation.CurrentWaypoint.point + _trafficAI.PointOffset);// _trafficAI._myTransform.InverseTransformPoint(_trafficAI._steeringPathNavigation.CurrentWaypoint.point + _trafficAI.PointOffset);

                if (_trafficAI.ReservedPoints.Count > 0)
                {
                    _trafficAI._relativeWaypointPositionOnCar = _trafficAI.InverseTransformPoint(_trafficAI._rearPointPosition, _trafficAI._rearPointRotation,
                        Vector3.one, _trafficAI.ReservedPoints.Peek().Point.point + _trafficAI.PointOffset);
                        //_trafficAI.RearPoint.InverseTransformPoint(_trafficAI.ReservedPoints.Peek().Point.point + _trafficAI.PointOffset);
                }

                _trafficAI.RelativeWPosMagnitude = _trafficAI._relativeWaypointPosition.magnitude;
            }

            private void CheckReservedPoints()
            {
                var distanceForCheck = Mathf.Sign(_trafficAI._relativeWaypointPositionOnCar.z) * _trafficAI._relativeWaypointPositionOnCar.sqrMagnitude;
                if (distanceForCheck > 0 || _trafficAI.ReservedPoints.Count <= 1)
                {
                    return;
                }

                _trafficAI.DispatchEvent(Events.Events.EventNames.LastPointReleased);
                
                TSReservedPoints cachedReservedPoint;
                
                lock (_trafficAI._reservedPointsLockObject)
                {
                    cachedReservedPoint = _trafficAI.ReservedPoints.Dequeue();
                }

                cachedReservedPoint.UnReserve(_trafficAI._myID);
                _trafficAI.ReduceSegDistance(cachedReservedPoint.Point.distanceToNextPoint);
                CheckIfSwitchedToNewLaneOrConnector(cachedReservedPoint);
            }

            private void CheckIfSwitchedToNewLaneOrConnector(TSReservedPoints cachedReservedPoint)
            {
                var changed = _trafficAI._lastReservedPoint.HasValue &&
                              _trafficAI._lastReservedPoint.Value.isConnector != cachedReservedPoint.isConnector;
                _trafficAI._lastReservedPoint = cachedReservedPoint;

                if (changed == false)
                {
                    return;
                }

                if (cachedReservedPoint.isConnector)
                {
                    if (_trafficAI.OccupiedLanes.Count > 0)
                    {
                        lock (_trafficAI._occupiedLanesLock)
                        {
                            _trafficAI.LastLaneIndex = _trafficAI.OccupiedLanes.Dequeue();
                        }

                        _trafficAI.LastLaneIndex.DecreaseTotalOccupation(Mathf.Round(_trafficAI.CarOccupationLenght / _trafficAI.LastLaneIndex.totalDistance * 100f));
                    }
                }
                else
                {
                    _trafficAI.UnReserveLaneIdOnNextReservedConnector();
                    TriggerNotTurningEvents();
                }

                DecreasePathIndexAndRemoveCurrentPath();
            }

            private void DecreasePathIndexAndRemoveCurrentPath()
            {
                if (_trafficAI._steeringPathNavigation.Count == 0)
                {
                    return;
                }

                _trafficAI._reservationPathNavigation.DecreasePathIndex();
                _trafficAI._brakingPathNavigation.DecreasePathIndex();
                _trafficAI._steeringPathNavigation.DecreasePathIndex();
                _trafficAI._steeringPathNavigation.RemoveAt(0);
            }

            private void TriggerNotTurningEvents()
            {
                _trafficAI._mainThread.Post(state =>  _trafficAI.OnTurnRight?.Invoke(false), this);
                _trafficAI._mainThread.Post(state =>  _trafficAI.OnTurnLeft?.Invoke(false), this);
                _trafficAI._isTurning = false;
            }
        }
    }
}