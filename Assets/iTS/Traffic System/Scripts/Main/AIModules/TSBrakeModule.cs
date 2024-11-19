using ITS.Utils;
using UnityEngine;

namespace ITS.AI
{
    public partial class TSTrafficAI
    {
        private float _currentPointDistance;
        private float _maxCurrentPointSpeed = float.MaxValue;
        public class TSBrakeModule : TSAIBaseModule
        {
            private TSPoints _slowestPoint;
            
            public override void OnDrawGizmosSelected()
            {
                if (ReferenceEquals(_slowestPoint, null)){return;}
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(_slowestPoint.point, Vector3.one);
            }
            
            public override void OnFixedUpdate()
            {
                _trafficAI.AddBrake(GetBrakeForCurves());
                _trafficAI._brake = CheckCarSpeedAndAdjustReturningValueForBraking(_trafficAI._brake);
                _trafficAI._brake = CheckIfNeedsToBrake(_trafficAI._brake);
                ValidateMaxSpeedAndOvertaking();
            }

            private void ValidateMaxSpeedAndOvertaking()
            {
                if (_trafficAI._maxSpeed == 0f)
                {
                    _trafficAI._brake = 1f;
                }

                if (_trafficAI.OverTaking && _trafficAI._reservationPathNavigation.CurrentPointIndex < 25)
                {
                    _trafficAI._brake = 1f;
                }
            }

            private float GetBrakeForCurves()
            {
                _slowestPoint = GetSlowestSpeedPoint();
                UpdateMaxCurrentPointSpeed(_slowestPoint);
                var weAreTooFast = _trafficAI.CarSpeed > _trafficAI._maxCurrentPointSpeed;
                var brakeForCurves = weAreTooFast ? Mathf.Clamp01((_trafficAI.CarSpeed -  _trafficAI._maxCurrentPointSpeed)  * 0.2f) : -1f;
                return brakeForCurves;
            }

            private void UpdateMaxCurrentPointSpeed(TSPoints point)
            {
                var pointVector3 = (point.point + _trafficAI.PointOffset);
                var relativePosition = (pointVector3 - _trafficAI._rearPointPosition);
                var dir = Vector3.Dot(_trafficAI._forward, relativePosition.normalized);
                if (dir > -0.8f) { dir = 1f;}
                var dirSign = Mathf.Sign(dir);
                var sqrDistance = relativePosition.sqrMagnitude * dirSign;
                var maxSpeedLimit = point.maxSpeedLimit.KphToMPS();
                var weAreTooClose = sqrDistance < _trafficAI._sqrMaxLookAheadDistance;
                var carSlowerThanMaxSpeed = _trafficAI.CarSpeed < _trafficAI._maxCurrentPointSpeed;
                var newMaxSpeedLowerThanCurrent = maxSpeedLimit < _trafficAI._maxCurrentPointSpeed;
                var shouldNotSetNewMaxSpeed = (!weAreTooClose || !carSlowerThanMaxSpeed || !newMaxSpeedLowerThanCurrent);
                
                if (shouldNotSetNewMaxSpeed && sqrDistance >= 0f){ return;}
                
                _trafficAI._maxCurrentPointSpeed = maxSpeedLimit;
                MoveToNextPoint();
            }

            private void MoveToNextPoint()
            {
                if (_trafficAI.OverTaking)
                {
                    _trafficAI._brakingPathNavigation.MoveToPreviousPoint();
                }
                else
                {
                    _trafficAI._brakingPathNavigation.MoveToNextPoint();
                }
            }

            private TSPoints GetSlowestSpeedPoint()
            {
                var point = _trafficAI._brakingPathNavigation.CurrentWaypoint;
                var counter = 0;

                while (point.maxSpeedLimit >= _trafficAI._brakingPathNavigation.CurrentLane.maxSpeed && counter < 20)
                {
                    MoveToNextPoint();
                    counter++;
                    point = _trafficAI._brakingPathNavigation.CurrentWaypoint;
                }

                if (counter > 0)
                {
                    _trafficAI._maxCurrentPointSpeed = _trafficAI._maxSpeed;
                }

                return point;
            }

            private float CheckCarSpeedAndAdjustReturningValueForBraking(float returningValue)
            {
                if (_trafficAI.CarSpeed >= (_trafficAI.OverTaking ? _trafficAI._maxSpeed * 1.2f : _trafficAI._maxSpeed))
                {
                    returningValue += (_trafficAI.CarSpeed - _trafficAI._maxSpeed) * 0.2f;
                }

                return returningValue;
            }

            private float CheckIfNeedsToBrake(float returningValue)
            {
                var stillMine = _trafficAI._reservationPathNavigation.CurrentWaypoint.ReservationID == _trafficAI._myID;
                
                if (stillMine && !_trafficAI.FullStop)
                {
                    return returningValue;
                }

                _trafficAI._overtake = stillMine == false || _trafficAI.FullStop || _trafficAI._reservationPathNavigation.CurrentWaypoint.roadBlockAhead;
                _trafficAI._currentPointDistance = _trafficAI.GetCurrentPointDistance();
                var isConnector = _trafficAI._reservationPathNavigation.CurrentWaypoint is TSConnectorPoint;
                var currentWaypointTaken = _trafficAI._reservationPathNavigation.CurrentWaypoint.IsReservedByCar && _trafficAI._reservationPathNavigation.CurrentWaypoint.IsReserved;
                var isReservedByLane = _trafficAI._reservationPathNavigation.CurrentWaypoint is TSConnectorPoint point && point.IsReservedByLane && point.LaneReservationID != _trafficAI._reservationPathNavigation.CurrentLane.GetPrevious(_trafficAI.myVehicleType).Id;
                
                if (!_trafficAI.FullStop && currentWaypointTaken && stillMine == false && isReservedByLane == false)
                {
                    CheckOtherCarDirectionAndAdjustMaxLockAheadDistance();
                    if (_trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved.OverTaking)
                    {
                        return 1f;
                    }

                    _trafficAI._currentPointDistance = UpdateCurrentPointDistance(_trafficAI._currentPointDistance);
                    _trafficAI._overtake = _trafficAI._nextCarSpeedSqr < _trafficAI.CurrentSpeedSqr && _trafficAI._currentPointDistance < _trafficAI.MAXLookAheadDistance + _trafficAI.LengthMargin;
                }
                else
                {
                    _trafficAI._nextCarSpeedSqr = 0;
                }

                if (isConnector)
                {
                    _trafficAI._overtake = false;
                }

                if (_trafficAI.OverTaking)
                {
                    return returningValue;
                }

                UpdateEarlyBrakePoint(currentWaypointTaken);
                var lookAheadDistance = (_trafficAI.EarlyBrakePoint ? _trafficAI.MAXLookAheadDistance + _trafficAI.minBrakingDistRoadblock : _trafficAI.MAXLookAheadDistance);
                
                if (_trafficAI._currentPointDistance <= lookAheadDistance) 
                {
                    var distance = Mathf.Max(_trafficAI._currentPointDistance, 0.0001f);
                    returningValue += Mathf.Max(_trafficAI.LengthMargin, _trafficAI.MAXLookAheadDistance) / distance;
                }

                return _trafficAI._currentPointDistance <= _trafficAI.LengthMargin? 1f: returningValue;
            }

            private void UpdateEarlyBrakePoint(bool currentWaypointTaken)
            {
                var currentConnector = _trafficAI._reservationPathNavigation.CurrentLane as TSLaneConnector;
                _trafficAI.EarlyBrakePoint = _trafficAI._reservationPathNavigation.CurrentWaypoint.ReservationID != 0 &&
                                             currentWaypointTaken == false && (currentConnector == null ||
                                                                               !currentConnector.connectorReservedByTrafficLight);
            }

            private void CheckOtherCarDirectionAndAdjustMaxLockAheadDistance()
            {
                var otherCarDir = Vector3.Dot(_trafficAI._forward, _trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved._forward);
                _trafficAI._nextCarSpeedSqr = _trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved.CurrentSpeedSqr;
                var differentLane = _trafficAI._steeringPathNavigation.CurrentLane != _trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved._steeringPathNavigation.CurrentLane;
                var oppositeDirection = otherCarDir <= 0;
                var otherCarEarlyBrake = _trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved.EarlyBrakePoint;
                var additionalDistance = otherCarEarlyBrake ? _trafficAI.minDistanceToOvertake * 0.5f : _trafficAI.LengthMargin;

                if ((!oppositeDirection || !differentLane) && !otherCarEarlyBrake) { return; }
                
                _trafficAI._nextCarSpeedSqr = 0;
                _trafficAI.MAXLookAheadDistance = _trafficAI.MAXLookAheadDistanceFullStop + additionalDistance;
            }
            
            private float UpdateCurrentPointDistance(float currentPointDistance)
            {
                var distanceVector = _trafficAI._reservationPathNavigation.CurrentWaypoint.CarWhoReserved._rearPointPosition - _trafficAI._frontPointPosition;
                var currentPointDistance1 = distanceVector.magnitude;
                return Mathf.Min(currentPointDistance1, currentPointDistance);
            }
        }
    }
}