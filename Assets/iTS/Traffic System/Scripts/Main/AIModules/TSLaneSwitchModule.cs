using System.Collections.Generic;
using UnityEngine;

namespace ITS.AI
{
    public partial class TSTrafficAI
    {
        [Header("Lane Switch Module Settings")]
        [Range(0.1f, 5f)] public float timerForChangeLane = 5f;
        public class TSLaneSwitchModule : TSAIBaseModule
        {
            private bool _changingLane;
            private bool _overtakingFromLeft;
            private Queue<TSReservedPoints> _reservedChangeLanePoints  = new Queue<TSReservedPoints>(100);

            public override void Initialize(TSTrafficAI trafficAI)
            {
                base.Initialize(trafficAI);
                _trafficAI._events.Subscribe(Events.Events.EventNames.UnReserveAll, OnUnReserveAll);
                _trafficAI._events.Subscribe(Events.Events.EventNames.LastPointReleased, ReleaseNextChangeLaneReservedPoint);
            }

            public override void OnFixedUpdate()
            {
                CheckIfNeedsToOvertake();
                CheckIfWeAreOvertakingAndTryToReturnToTheMainLane();
            }

            private void LaneChange(bool left)
            {
                if (_changingLane || _trafficAI.ReservedPoints.Count <= 0 ||
                    !(_trafficAI._steeringPathNavigation.CurrentLane is TSLaneInfo newCurrentLane))
                {
                    return;
                }

                var lasReservedPoint = _trafficAI.ReservedPoints.Peek();
                var newCurrentWaypointOnCar = 0;
                var newLane = 0;

                if (_trafficAI.OverTaking)
                {
                    newCurrentWaypointOnCar = (_overtakingFromLeft
                        ? lasReservedPoint.Point.rightParallelPointIndex
                        : lasReservedPoint.Point.leftParallelPointIndex);
                    newLane = (_overtakingFromLeft
                        ? newCurrentLane.laneLinkRight
                        : newCurrentLane.laneLinkLeft);
                }
                else
                {
                    var point = 0;
                    var lane = 0;
                    var found = false;
                    if (left && lasReservedPoint.Point.leftParallelPointIndex != -1 && newCurrentLane.HasLinkLeft)
                    {
                        found = true;
                        point = lasReservedPoint.Point.leftParallelPointIndex;
                        lane = newCurrentLane.laneLinkLeft;
                    }
                    else if (lasReservedPoint.Point.rightParallelPointIndex != -1 && newCurrentLane.HasLinkRight)
                    {
                        found = true;
                        point = lasReservedPoint.Point.rightParallelPointIndex;
                        lane = newCurrentLane.laneLinkRight;
                    }

                    if (found == false)
                    {
                        return;
                    }

                    newCurrentWaypointOnCar = Mathf.Clamp(point-1,0,int.MaxValue);
                    newLane = lane;
                }

                if (_trafficAI._lanes[newLane].HasVehicleType(_trafficAI.myVehicleType) == false)
                {
                    return;
                }

                var segDistance = _trafficAI.MAXLookAheadDistanceFullStop;
                var counter = 0;
                UnReserveReservedChangeLanePoints();

                if (_trafficAI._lanes[newLane].TryToReserve(_trafficAI, newCurrentWaypointOnCar, segDistance, ref _reservedChangeLanePoints))
                {
                    counter = _reservedChangeLanePoints.Count;
                    
                    lock (_trafficAI._reservedPointsLockObject)
                    {
                        (_trafficAI.ReservedPoints, _reservedChangeLanePoints) = (_reservedChangeLanePoints, _trafficAI.ReservedPoints);
                    }
                }
                else
                {
                    return;
                }
                
                _trafficAI.UnReserveAllReservedConnectors();
                _changingLane = true;
                newCurrentLane = _trafficAI._lanes[newLane];

                if (_trafficAI.OverTaking)
                {
                    _trafficAI.OverTaking = false;
                    _trafficAI.ChangeLaneTime = Time.time;
                }
                _trafficAI.shouldDisable = false;
                _trafficAI.InitializeWaypointsData(newCurrentLane, newCurrentWaypointOnCar, newCurrentWaypointOnCar, newCurrentWaypointOnCar);
            }

            private void OverTake(bool left)
            {
                if (_trafficAI.OverTaking || _changingLane || _trafficAI.ReservedPoints.Count <= 0 || !(_trafficAI._steeringPathNavigation.CurrentLane is TSLaneInfo))
                {
                    return;
                }

                var lastReservedPoint = _trafficAI.ReservedPoints.Peek();
                var newCurrentWaypointOnCar = (left ? lastReservedPoint.Point.leftParallelPointIndex : lastReservedPoint.Point.rightParallelPointIndex);
                newCurrentWaypointOnCar = Mathf.Clamp(newCurrentWaypointOnCar - 1, 0, int.MaxValue);
                var newLane = (left ? _trafficAI._steeringPathNavigation.CurrentLane.laneLinkLeft : _trafficAI._steeringPathNavigation.CurrentLane.laneLinkRight);

                if (newCurrentWaypointOnCar == -1 || newCurrentWaypointOnCar < 25 || _trafficAI._lanes[newLane].HasVehicleType(_trafficAI.myVehicleType) == false)
                {
                    return;
                }

                var initialIndex = lastReservedPoint.point;
                
                lock (_trafficAI._occupiedLanesLock)
                {
                    var lastLane = _trafficAI.OccupiedLanes.Dequeue();
                    lastLane.DecreaseTotalOccupation(Mathf.Round((_trafficAI.CarOccupationLenght) / lastLane.totalDistance * 100f));
                }
                
                UnReserveReservedChangeLanePoints();

                var distance = Mathf.Clamp(_trafficAI.MAXLookAheadDistanceFullStop * 5f, _trafficAI.CarDepth * 10f + 3f, float.MaxValue);

                if (_trafficAI._lanes[newLane].TryToReserve(_trafficAI, initialIndex, distance, ref _reservedChangeLanePoints))
                {
                    lock (_trafficAI._reservedPointsLockObject)
                    {
                        (_trafficAI.ReservedPoints, _reservedChangeLanePoints) = (_reservedChangeLanePoints, _trafficAI.ReservedPoints);
                    }
                }
                else
                {
                    return;
                }

                _trafficAI.shouldDisable = false;
                _trafficAI.UnReserveAllReservedConnectors();
                _trafficAI.OverTaking = true;
                _changingLane = true;
                _overtakingFromLeft = !left;
                var newCurrentWaypoint = Mathf.Clamp(newCurrentWaypointOnCar, 0, _trafficAI._steeringPathNavigation.Waypoints.Length - 1);
                var newPreviousWaypoint = newCurrentWaypointOnCar;
                _trafficAI.InitializeWaypointsData(_trafficAI._lanes[newLane], newPreviousWaypoint, newCurrentWaypoint, newCurrentWaypoint);
            }

            private void CheckIfWeAreOvertakingAndTryToReturnToTheMainLane()
            {
                if (_trafficAI.OverTaking == false || Time.time - _trafficAI.ChangeLaneTime <= 1f)
                {
                    return;
                }

                _trafficAI.ChangeLaneTime = Time.time;
                LaneChange(false);
            }

            private void CheckIfNeedsToOvertake()
            {
                if (_trafficAI._steeringPathNavigation.CurrentLane is TSLaneInfo == false){return;}
                if (_trafficAI._time - _trafficAI.ChangeLaneTime < _trafficAI.timerForChangeLane){return;}

                var noRoadBlockAhead = _trafficAI._reservationPathNavigation.CurrentWaypoint.roadBlockAhead == false;
                if (!_trafficAI.FullStop && _trafficAI._currentPointDistance <= _trafficAI.minDistanceToOvertake && noRoadBlockAhead){return;}
                
                if (!_trafficAI._overtake || (_trafficAI._reservationPathNavigation.CurrentWaypoint.ReservationID == 0 && !_trafficAI.FullStop && noRoadBlockAhead))
                {
                    return;
                }

                var tsLaneInfo = (TSLaneInfo) _trafficAI._steeringPathNavigation.CurrentLane;
                _trafficAI.ChangeLaneTime = _trafficAI._time;
                if (_trafficAI.CurrentSteerWaypoint.leftParallelPointIndex != -1 && tsLaneInfo.laneLinkLeft >= 0 && tsLaneInfo.laneLinkLeft < _trafficAI._lanes.Length)
                {
                    int nextLane = tsLaneInfo.laneLinkLeft;
                    if (_trafficAI._lanes[nextLane].laneLinkRight != -1 && _trafficAI._lanes[_trafficAI._lanes[nextLane].laneLinkRight].Id == tsLaneInfo.Id)
                    {
                        LaneChange(true);
                    }
                    else if (!_trafficAI.OverTaking && _trafficAI._lanes[_trafficAI._lanes[nextLane].laneLinkLeft].Id == tsLaneInfo.Id)
                    {
                        if (_trafficAI.CarSpeed < _trafficAI._maxSpeed && (_trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved == null || !_trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved.OverTaking))
                        {
                            OverTake(true);
                        }
                    }
                }

                if (_trafficAI.CurrentSteerWaypoint.rightParallelPointIndex != -1 && tsLaneInfo.laneLinkRight >=0 && tsLaneInfo.laneLinkRight < _trafficAI._lanes.Length)
                {
                    int nextLane = tsLaneInfo.laneLinkRight;

                    if (_trafficAI._lanes[nextLane].laneLinkLeft != -1 && _trafficAI._lanes[_trafficAI._lanes[nextLane].laneLinkLeft].Id == tsLaneInfo.Id)
                    {
                        LaneChange(false);
                    }
                    else if (!_trafficAI.OverTaking && _trafficAI._lanes[_trafficAI._lanes[nextLane].laneLinkRight].Id == tsLaneInfo.Id)
                    {
                        if (_trafficAI.CarSpeed < _trafficAI._maxSpeed && (_trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved == null || !_trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved.OverTaking))
                        {
                            OverTake(false);
                        }
                    }
                }
            }
            
            private void ReleaseNextChangeLaneReservedPoint()
            {
                if (_reservedChangeLanePoints.Count == 0)
                {
                    _changingLane = false;
                    return;
                }

                _changingLane = true;
                lock (_trafficAI._reservedPointsLockObject)
                {
                    var tsReservedPoints = _reservedChangeLanePoints.Dequeue();
                    tsReservedPoints.UnReserve(_trafficAI.MyID);
                }
            }

            private void UnReserveReservedChangeLanePoints()
            {
                while (_reservedChangeLanePoints.Count > 0)
                {
                    var peek = _reservedChangeLanePoints.Dequeue();
                    peek.Point.TryUnReservePoint(_trafficAI.MyID);
                }
            }
            
            private void DrawChangeLaneReservedPoints()
            {
                Gizmos.color = Color.yellow;
                foreach (var resP in _reservedChangeLanePoints)
                {
                    if (resP.Point.CarWhoReserved == null && resP.Point.ReservationID == 0) Gizmos.color = Color.cyan;
                    else if (resP.Point.CarWhoReserved != _trafficAI) Gizmos.color = Color.magenta;
                    else
                        Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(resP.Point.point +_trafficAI.PointOffset, _trafficAI.CarWidth);
                }
            }

            private void OnUnReserveAll()
            {
                _changingLane = false;
                _trafficAI.OverTaking = false;
                UnReserveReservedChangeLanePoints();
            }
        }
    }
}