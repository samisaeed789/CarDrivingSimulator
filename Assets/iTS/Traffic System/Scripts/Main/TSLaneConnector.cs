using System;
using System.Collections.Generic;
using ITS.AI;
using ITS.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// TS lane connector class.  This class holds all the information regarding the connectors.
/// </summary>
[Serializable]
public class TSLaneConnector : TSBaseInfo
{
    public TSLaneConnector() {}

    public TSLaneConnector(int laneFromIndex, int laneToIndex, TSLaneInfo laneFrom, TSLaneInfo laneTo, float resolution)
    {
        var laneToConectorA = laneTo.StartDirection;
        var laneFromConectorB = laneFrom.EndDirection;
        var angle = Vector3.Angle(laneToConectorA, laneFromConectorB);
        var referenceRight = Vector3.Cross(Vector3.up, laneFromConectorB);
        var sign = Mathf.Sign(Vector3.Dot(laneToConectorA, referenceRight)); // >= 0.0f) ? 1.0f: -1.0f;
        angle *= sign;
        if (angle > 30) direction = Direction.Right;
        else if (angle < -30) direction = Direction.Left;
        else direction = Direction.Straight;
        middlePoints = new List<Vector3>();
        conectorA = laneFrom.conectorB;
        conectorB = laneTo.conectorA;
        //connectorAType = ConnectorType.B;
        //connectorBType = ConnectorType.A;
        nextLane = laneToIndex;
        previousLane = laneFromIndex;
        middlePoints.Add(((conectorA + conectorB) / 2f));
        
        if (Mathf.Abs(angle) > 5)
        {
            var multiplier = Mathf.Min(Mathf.Abs(angle) / 90f * 0.35f, 0.5f);
            var tempDir = Quaternion.LookRotation(laneToConectorA);
            var tempDir1 = Quaternion.LookRotation(laneFromConectorB);
            var tempConnectorA = new GameObject();
            tempConnectorA.transform.position = conectorA;
            tempConnectorA.transform.rotation = tempDir1;
            var tempDistance = tempConnectorA.transform.InverseTransformPoint(conectorB);
            Object.DestroyImmediate(tempConnectorA);
            middlePoints[0] += (tempDir * Vector3.forward * (Mathf.Abs(tempDistance.x) * multiplier)) + (tempDir1 * -Vector3.forward * (Mathf.Abs(tempDistance.z) * multiplier));
        }
        
        var pts = new Vector3[]{conectorA,conectorA, middlePoints[0], conectorB, conectorB};
        points = new TSConnectorPoint[0];
        TSUtils.CreatePoints(resolution, pts, ref points, ref totalDistance);
        laneFrom.connectors = laneFrom.connectors.Add(this);
        laneTo.connectorsReverse = laneTo.connectorsReverse.Add(this);
    }
    public TSLaneConnector(Vector3 startPoint, Vector3 endPoint, float resolution, TSLaneInfo.VehicleType defaultVehicleType) : base(startPoint, endPoint, resolution, defaultVehicleType)
    {
        points = new TSConnectorPoint[0];
    }
    
    public void CreateWaypoints(float resolution)
    {
        CreateWaypoints(resolution, ref points);
    }
    
    [NonSerialized] protected new TSLaneInfo[] _lanes;
    /*public enum ConnectorType
    {
        A = 0,
        B = 1
    }*/
    
    public override TSPoints[] Points => points;
    public override TSBaseInfo GetNext(TSLaneInfo.VehicleType _vehicleType)
    {
        return NextLane;
    }
    
    public override TSBaseInfo GetPrevious(TSLaneInfo.VehicleType _vehicleType)
    {
        return PreviousLane;
    }

    /*public override TSBaseInfo GetNext(TSLaneInfo.VehicleType _vehicleType, out bool reversed)
    {
        reversed = NextLaneIsContrary;
        return GetNext(_vehicleType);
    }

    public override TSBaseInfo GetPrevious(TSLaneInfo.VehicleType _vehicleType, out bool reversed)
    {
        reversed = NextLaneIsContrary;
        return GetPrevious(_vehicleType);
    }*/

    public TSLaneInfo NextLane => _lanes[nextLane];
    public TSLaneInfo PreviousLane => _lanes[previousLane]; 
    public int nextLane;
    public int previousLane;
    //public ConnectorType connectorAType = ConnectorType.B;
    //public ConnectorType connectorBType = ConnectorType.A;
    public bool forcedStop;
    public bool IsReserved => _reservedByID.Count > 0;
    public bool IsRequested { get; private set; }
    //public bool NextLaneIsContrary => connectorBType == ConnectorType.B;
    private HashSet<int> _reservedByID = new HashSet<int>();
    public TSConnectorPoint[] points;
    public int priority = 1;
    public Direction direction =  Direction.Straight;
    public bool connectorReservedByTrafficLight;
    public float remainingGreenLightTime = -1;

    public enum Direction{
        Left,
        Right,
        Straight
    }
    
    public HashSet<TSLaneConnector> OtherConnectors = new HashSet<TSLaneConnector>();

    public override void Init(TSLaneInfo[] lanes)
    {
        base.Init(lanes);
        _lanes = lanes;
    }

    public void UnReserveLaneIdOnNearConnectorPoints(int myID, float carOccupation)
    {
        if (IsReserved == false) {return; }
        if( _reservedByID.Contains(myID) == false){return;}
        
        if (_reservedByID.Count == 1)
        {
            for (var myIndex = 0; myIndex < points.Length; myIndex++)
            {
                points[myIndex].UnReserveOtherConnectorPointsByLane(PreviousLane.Id);
                points[myIndex].UnReservePointByLane(PreviousLane.Id);
            }

            IsRequested = false;
        }

        _reservedByID.Remove(myID);
        NextLane.DecreaseTotalOccupation(Mathf.Round(carOccupation / NextLane.totalDistance * 100f));
    }
	
    public bool HasHighestPriority
    {
        get
        {
            foreach (var otherConnector in OtherConnectors)
            {
                if (otherConnector.IsRequested && otherConnector.priority > priority && otherConnector.previousLane != previousLane)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public void TryToRequestNextConnector(int currentWaypoint, float minConnectorRequestDistance)
    {
        /*if (isReserved == false)
		{
			isRequested = false;
			return;
		}*/
        var currentLaneIndex = PreviousLane;
        var isNotTotallyOccupied = NextLane.TotalOccupation < NextLane.maxTotalOccupation;
        var isMinRequestDistanceMeet = currentLaneIndex.totalDistance * (1 - (float) currentWaypoint / currentLaneIndex.Points.Length) < minConnectorRequestDistance;
        if (!isNotTotallyOccupied || !isMinRequestDistanceMeet) {return;}

        IsRequested = true;
    }
	
    public override bool TryToReserve(TSTrafficAI tsTrafficAI, int pointIndex)
    {
        var point = points[pointIndex];
        bool stillMine;
        var id = tsTrafficAI.MyID;
        var carOccupation = tsTrafficAI.CarOccupationLenght;
        var carSpeed = tsTrafficAI.CarSpeed;
        var canCrossConnector = CheckIfCanCrossConnector(tsTrafficAI.ignoreTrafficLight, tsTrafficAI.CarSpeed, tsTrafficAI.PointOffset, tsTrafficAI.FrontPoint);
        if (canCrossConnector)
        {
            stillMine = ReserveNearConnectorPoints(id, carSpeed, carOccupation);
            if (stillMine == false)
            {
                return false;
            }
        }
		
        if (IsReserved && canCrossConnector)
        {
            if (point.TryReservePoint(id, tsTrafficAI, PreviousLane.Id))// && ReserveNearConnectorPoints(id, tsTrafficAI, point))
            {
                return true;
            }

            
            if (point.ReservationID == id)
            {
                point.TryUnReservePoint(id);
            }

            stillMine = false;
            //if (!isFromSameLane)
            //{
                //otherCarPresentInJunction = true;
            //}
            
        }
        else
        {
            if (point.ReservationID == id)
            {
                point.TryUnReservePoint(id);
            }

            stillMine = false;
        }

        return stillMine;
    }

    public override SwitchResponse TrySwitchToLink(SwitchDirection direction, int pointIndex, int reservationID = 0, int pointsQty = 0,
        TSTrafficAI carWhoReserves = null, bool travelingReverse = false)
    {
        var newLane = travelingReverse? NextLane : PreviousLane;
        //var laneConnectors = travelingReverse?newLane.connectorsReverse:newLane.connectors;
        switch (direction)
        {
            case SwitchDirection.Left:
                if (newLane.HasLinkLeft == false)
                {
                    Debug.Log("No connector link found on left");
                    break;
                }

                Debug.Log("connector switching to left");
                var lane = (TSLaneInfo) (newLane.leftLinkData.LinkInfo);
                var laneConnectors = travelingReverse | newLane.LeftLinkIsContrary?lane.connectorsReverse:lane.connectors;
                var newLaneConnector = laneConnectors[points[pointIndex].leftParallelConnectorIndex];
                return new SwitchResponse
                {
                    isContrary =  newLane.LeftLinkIsContrary,
                    newBaseInfo = newLaneConnector,
                    newPointIndex = points[pointIndex].leftParallelPointIndex,
                    newPoint = newLaneConnector.Points[points[pointIndex].leftParallelPointIndex]
                };
            case SwitchDirection.Right:
                if (newLane.HasLinkRight == false)
                {
                    Debug.Log("No connector link found on right");
                    break;
                }
                
                Debug.Log("connector switching to right");
                lane = (TSLaneInfo) (newLane.rightLinkData.LinkInfo);
                laneConnectors = travelingReverse | newLane.RightLinkIsContrary?lane.connectorsReverse:lane.connectors;
                var newBaseInfo = laneConnectors[points[pointIndex].rightParallelConnectorIndex];
                return new SwitchResponse
                {
                    isContrary =  newLane.RightLinkIsContrary,
                    newBaseInfo = newBaseInfo,
                    newPointIndex = points[pointIndex].rightParallelPointIndex,
                    newPoint = newBaseInfo.Points[points[pointIndex].rightParallelPointIndex]
                };
        }
        
        Debug.LogFormat("Failed to switch from connector from lane {0} to {1}", newLane.Id, direction);
        
        return new SwitchResponse
        {
            isContrary = false,
            newBaseInfo = this,
            newPointIndex = pointIndex,
            newPoint = points[pointIndex]
        }; 
    }

    private bool CheckIfCanCrossConnector(bool ignoreTrafficLight, float CarSpeed, Vector3 pointOffset, Transform FrontPoint)
    {
        if (ignoreTrafficLight || remainingGreenLightTime <= -1) return true;
		
        var distanceToRun1 = Mathf.Max(10f, CarSpeed) * remainingGreenLightTime;
        var currentConnectorDistance = (((points[0].point + pointOffset) - FrontPoint.position).magnitude + totalDistance);
        var returningValue = distanceToRun1 > currentConnectorDistance;

        return returningValue;
    }
	
    private bool ReserveNearConnectorPoints(int MyID, float CarSpeed, float carOcupation)
    {
        var roolback = false;

        var cantContinueReserving = CanContinueReserving(MyID, CarSpeed, carOcupation) == false;
        if (IsReserved)
        {
            if (_reservedByID.Contains(MyID)) {return IsReserved;}

            if (cantContinueReserving)
            {
                //Debug.Log($"Reserved, but no by {MyID}");
                return false;
            }
        }
        else
        {
            if (cantContinueReserving) {return false;}
            
            for (var i = 0; i < points.Length; i++)
            {
                for (var pointIndex = 0; pointIndex < points[i].otherConnectorsPoints.Length; pointIndex++)
                {
                    var otherConnectorsPoint = points[i].otherConnectorsPoints[pointIndex];
                    if ( otherConnectorsPoint.Connector.IsRequested && otherConnectorsPoint.Connector.priority > priority && otherConnectorsPoint.Lane.Id != PreviousLane.Id)
                    {
                        roolback = true;
                        break;
                    }

                    var pointIsReservedByLane = otherConnectorsPoint.Point.IsReservedByLane && otherConnectorsPoint.Point.LaneReservationID != PreviousLane.Id;
                    if (!pointIsReservedByLane) {continue;}
                    
                    roolback = true;
                    break;
                }

                if (points[i].IsReservedByLane && points[i].LaneReservationID != PreviousLane.Id)
                {
                    roolback = true;
                    break;
                }
                
                if (roolback)
                {
                    break;
                }
            }

            if (!roolback)
            {
                ReserveConnector();
            }
            else
            {
                return false;
            }
        }

        
        _reservedByID.Add(MyID);
            //Debug.Log($"Adding {MyID} to lane {nextLane} at time {Time.realtimeSinceStartup}");
        NextLane.AddOccupation(Mathf.Round(carOcupation / NextLane.totalDistance * 100f));

        return IsReserved;
    }

    private void ReserveConnector()
    {
        for (var i = 0; i < points.Length; i++)
        {
            for (var pointIndex = 0; pointIndex < points[i].otherConnectorsPoints.Length; pointIndex++)
            {
                var point = points[i].otherConnectorsPoints[pointIndex];
                point.Point.TryReservePointByLane(PreviousLane.Id);
                point.Point.ConnectorReservationCount++;
                OtherConnectors.Add(point.Connector);
            }

            points[i].TryReservePointByLane(PreviousLane.Id);
            points[i].ConnectorReservationCount++;
        }

    }

    private bool CanContinueReserving(int MyID, float CarSpeed, float carOccupation)
    {
        if (IsReserved && _reservedByID.Contains(MyID)) { return true;}

        if (HasHighestPriority == false)
        {
            UnReserveLaneIdOnNearConnectorPoints(MyID, carOccupation);
            return false;
        }

        if (forcedStop && CarSpeed > 0.1f)
        {
            UnReserveLaneIdOnNearConnectorPoints(MyID, carOccupation);
            return false;
        }

        if (NextLane.TotalOccupation > NextLane.maxTotalOccupation)
        {
            UnReserveLaneIdOnNearConnectorPoints(MyID, carOccupation);
            return false;
        }
        
        if (connectorReservedByTrafficLight)
        {
            UnReserveLaneIdOnNearConnectorPoints(MyID, carOccupation);
            return false;
        }

        return true;
    }

    public bool ReserveNearConnectorPoints(int id, TSTrafficAI car, TSConnectorPoint point, out bool isFromSameLane)
    {
        if (!IsReserved)
        {
            isFromSameLane = false;
            return false;
        }
        
        isFromSameLane = true;
        var roolback = false;
        for (var pointIndex = 0; pointIndex < point.otherConnectorsPoints.Length; pointIndex++)
        {
            var otherConnectorsPoint = point.otherConnectorsPoints[pointIndex];
            var laneConnector = otherConnectorsPoint.Connector;
            var tsPoints = otherConnectorsPoint.Point;
            if (tsPoints.LaneReservationID == PreviousLane.Id)
            {
                if (tsPoints.TryReservePoint(id, car) == false)
                {
                    if (ReferenceEquals(tsPoints.CarWhoReserved , null) == false && tsPoints.LaneReservationID != PreviousLane.Id)
                    {
                        isFromSameLane = false;
                    }
                }
            }
            else
            {
                isFromSameLane = false;
            }

            if (tsPoints.ReservationID == id || laneConnector.connectorReservedByTrafficLight) {continue;}
            roolback = true;
            break;
        }

        if (roolback)
        {
            point.UnReserveOtherConnectorPoints(id);
        }

        return !roolback;
    }
}