using System;
using ITS.AI;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class TSOtherPoints
{
    public int lane = -1;
    public int connector = -1;
    public int pointIndex = -1;
}

[Serializable]
public class TSPoints
{
    public bool IsReserved => ReservationID != 0;
    public bool IsReservedByCar => ReferenceEquals(_carWhoReserved, null) == false;
    public int ReservationID { get; private set; } = 0;
    public TSTrafficAI CarWhoReserved => _carWhoReserved;
    [NonSerialized] private TSTrafficAI _carWhoReserved;
    
    public Vector3 point;
    public float distanceToNextPoint = 0f;
    [FormerlySerializedAs("rightParalelLaneIndex")] public int rightParallelPointIndex = -1;
    public int rightParallelConnectorIndex = -1;
    [FormerlySerializedAs("leftParalelLaneIndex")] public int leftParallelPointIndex = -1;
    public int leftParallelConnectorIndex = -1;
    public float maxSpeedLimit = 1000f;
    public bool roadBlockAhead = false;
    public TSOtherPoints[] nearbyPoints;
    
    public virtual bool TryReservePoint(int id, TSTrafficAI car = null, bool force = false)
    {
        if (IsReserved && !force)
        {
            return ReservationID == id;
        }

        ReservationID = id;
        _carWhoReserved = car;
        return true;
    }
	
    public virtual bool TryUnReservePoint(int id)
    {
        if (ReservationID != id){ return false;}
		
        ReservationID = 0;
        _carWhoReserved = null;
        return true;
    }
}

/// <summary>
/// TS points.  The class holds the information about a point.
/// </summary>
[System.Serializable]
public class TSConnectorPoint : TSPoints
{
    public bool IsReservedByLane => LaneReservationID != -1;
    public int LaneReservationID { get; private set; } = -1;
    public int ConnectorReservationCount { get; set; } = 0;
    public TSConnectorOtherPoints[] otherConnectorsPoints; 
	
    public bool TryReservePointByLane(int id)
    {
        if (IsReservedByLane)
        {
            return false;
        }

        LaneReservationID = id;
        return true;
    }
	
    public void UnReservePointByLane(int id)
    {
        if (LaneReservationID != id) { return; }

        if (ConnectorReservationCount < 2)
        {
            LaneReservationID = -1;
            ConnectorReservationCount = 0;
            return;
        }
		
        ConnectorReservationCount--;
    }

    public void UnReserveOtherConnectorPointsByLane(int id)
    {
        for (var x = 0; x < otherConnectorsPoints.Length; x++)
        {
            var tsConnectorPoint = otherConnectorsPoints[x].Point;
            tsConnectorPoint.UnReservePointByLane(id);
        }
    }

    public void UnReserveOtherConnectorPoints(int myID)
    {
        if (otherConnectorsPoints.Length == 0)
        {
            return;
        }

        for (var index = 0; index < otherConnectorsPoints.Length; index++)
        {
            var otherPoints = otherConnectorsPoints[index];
            otherPoints.Point.TryUnReservePointBase(myID);
        }
    }

    public bool TryReservePoint(int id, TSTrafficAI car, int PreviousLaneId)
    {
        var firstResult = base.TryReservePoint(id, car, false);
        if (firstResult == false) { return false;}
        
        var roolback = false;
        for (var pointIndex = 0; pointIndex < otherConnectorsPoints.Length; pointIndex++)
        {
            var otherConnectorsPoint = otherConnectorsPoints[pointIndex];
            var laneConnector = otherConnectorsPoint.Connector;
            var tsPoints = otherConnectorsPoint.Point;
            if (tsPoints.LaneReservationID == PreviousLaneId)
            {
                if (tsPoints.TryReservePoint(id, car) == false)
                {
                    if (ReferenceEquals(tsPoints.CarWhoReserved , null) == false && tsPoints.LaneReservationID != PreviousLaneId)
                    {
                    }
                }
            }
            else
            {
            }

            if (tsPoints.ReservationID == id || laneConnector.connectorReservedByTrafficLight) {continue;}
            roolback = true;
            break;
        }

        if (roolback)
        {
            UnReserveOtherConnectorPoints(id);
        }

        return !roolback;
        
    }

    private void TryUnReservePointBase(int id)
    {
        base.TryUnReservePoint(id);
    }

    public override bool TryUnReservePoint(int id)
    {
        UnReserveOtherConnectorPoints(id);
        return base.TryUnReservePoint(id);
    }
}