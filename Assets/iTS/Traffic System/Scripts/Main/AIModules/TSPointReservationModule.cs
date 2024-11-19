using ITS.Utils;
using UnityEngine;

namespace ITS.AI
{
    public partial class TSTrafficAI
    {
        private float SegDistance { get; set; } = 0f;
        
        public class TSPointReservationModule : TSTrafficAI.TSAIBaseModule
        {
            private TSPlayerSensorModule _playerSensorModule;
            private bool PlayerSensorModulePresent { get; set; }

            public override void PostInitialize()
            {
                base.PostInitialize();
                _playerSensorModule = _trafficAI.GetModule<TSPlayerSensorModule>();
                PlayerSensorModulePresent = _playerSensorModule != null;
            }

            public override void OnFixedUpdate()
            {
                ReservePoints();
            }

            private void ReservePoints()
            {
                _trafficAI.SegDistance = _trafficAI.SegDistance < 0 ? 0 : _trafficAI.SegDistance;
                var tempMaxLookAhead = (_trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved == null
                    ? _trafficAI.MAXLookAheadDistanceFullStop
                    : _trafficAI.MAXLookAheadDistance) + _trafficAI.LengthMargin;
                  
                var distanceChecking =_trafficAI.FullStop && PlayerSensorModulePresent ? Mathf.Clamp(_playerSensorModule.SqrDistance.Sqrt(), 0f, _trafficAI.MAXLookAheadDistanceFullStop): 
                    (_trafficAI.CarDepth > tempMaxLookAhead ? _trafficAI.CarDepth : tempMaxLookAhead) * _trafficAI.reservePointDistanceMultiplier;
                var atLeastOnce = _trafficAI._currentPointDistance <= (_trafficAI.EarlyBrakePoint ? _trafficAI.MAXLookAheadDistance + _trafficAI.minBrakingDistRoadblock : _trafficAI.MAXLookAheadDistance) && _trafficAI.ReservedPointsCount <= 100;

                while (_trafficAI.SegDistance <= distanceChecking || _trafficAI.ReservedPointsCount <= 4 || atLeastOnce)
                {
                    atLeastOnce = false;
                    if (_trafficAI.NextTrackPathCount > 0)
                    {
                        if (_trafficAI._reservationPathNavigation.NoMorePath == false)
                        {
                            TryToRequestNextConnector();
                        }
                    }

                    if (!TryToReservePoint())
                    {
                        break;
                    }
                }
            }
            
            private void TryToRequestNextConnector()
            {
                if (!_trafficAI._reservationPathNavigation.TryGetNextPath(out var nextPath))
                {
                    return;
                }

                if (nextPath.Value.IsConnector == false || nextPath.Value.NextConnector.IsRequested)
                {
                    return;
                }

                nextPath.Value.NextConnector.TryToRequestNextConnector(_trafficAI._reservationPathNavigation.CurrentPointIndex,
                    _trafficAI.minConnectorRequestDistance);
            }
            
            private bool TryToReservePoint()
            {
                _trafficAI._otherCarPresentInJunction = false;
                var trafficLightOverride = _trafficAI.ignoreTrafficLight && _trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved == null && _trafficAI._reservationPathNavigation.CurrentWaypoint.ReservationID != 0;

                if (_trafficAI._reservationPathNavigation.CurrentWaypoint.ReservationID == _trafficAI.MyID)
                {
                    AdvanceReservationPathWaypointIndex();
                    return false;
                }
                
                if (_trafficAI._reservationPathNavigation.CurrentLane.TryToReserve(_trafficAI, _trafficAI._reservationPathNavigation.CurrentPointIndex) == false)
                {
                    /*if (currentLaneIndex is TSLaneConnector)
                    {
                        //ResetRoute();
                    }*/

                    return false;
                }

                if (_trafficAI._reservationPathNavigation.CurrentLane is TSLaneConnector connector)
                {
                    _trafficAI.AddOccupiedLane(connector.NextLane);
                    if (_trafficAI._reservedConnectors.Contains(connector) == false)
                    {
                        _trafficAI._reservedConnectors.Enqueue(connector);
                    }
                }

                var newPoint = CreateNewLaneReservedPoint();
                AddReservedCurrentPoint(newPoint);
                return true;
            }
            
            private TSReservedPoints CreateNewLaneReservedPoint()
            {
                return new TSReservedPoints(_trafficAI._reservationPathNavigation.CurrentLane is TSLaneConnector,
                    _trafficAI._reservationPathNavigation.CurrentPointIndex,
                    _trafficAI._reservationPathNavigation.CurrentWaypoint);
            }

            private void AddReservedCurrentPoint(TSReservedPoints newPoint)
            {
                lock (_trafficAI._reservedPointsLockObject)
                {
                    _trafficAI.ReservedPoints.Enqueue(newPoint);
                }

                _trafficAI._nextCarSpeedSqr = 0;
                _trafficAI.SegDistance += newPoint.Point.distanceToNextPoint;
                AdvanceReservationPathWaypointIndex();
            }

            private void AdvanceReservationPathWaypointIndex()
            {
                if (_trafficAI.OverTaking)
                {
                    _trafficAI._reservationPathNavigation.MoveToPreviousPoint();
                }
                else
                {
                    _trafficAI._reservationPathNavigation.MoveToNextPoint();
                }
            }
        }
    }
}