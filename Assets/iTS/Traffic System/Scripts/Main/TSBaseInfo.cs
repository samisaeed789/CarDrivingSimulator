using System;
using System.Collections.Generic;
using ITS.AI;
using ITS.Utils;
using UnityEngine;

[Serializable]
public abstract class TSBaseInfo
{
    protected TSBaseInfo(){}

    protected TSBaseInfo(Vector3 startPoint, Vector3 endPoint, float resolution, TSLaneInfo.VehicleType defaultVehicleType)
    {
        vehicleType = defaultVehicleType;
        conectorA = startPoint;
        conectorB = endPoint;
        middlePoints = new List<Vector3> {(conectorA + conectorB) / 2f};
    }
    
    protected void CreateWaypoints<T>(float resolution, ref T[] points) where T : TSPoints, new()
    {
        var pts = new Vector3[4 + middlePoints.Count];
        pts[0] = conectorA;
        pts[1] = conectorA;
        var r = 2;
        for (; r < (2 + middlePoints.Count); r++)
            pts[r] = middlePoints[r - 2];

        pts[r] = conectorB;
        pts[r + 1] = conectorB;
        points = new T[0];
        TSUtils.CreatePoints(resolution, pts, ref points, ref totalDistance);
    }
    
    [NonSerialized] protected TSBaseInfo[] _lanes;

    protected internal class LinkData
    {
        public bool IsContrary { get;}
        public TSBaseInfo LinkInfo => _linkInfo;
        [NonSerialized] private TSBaseInfo _linkInfo;

        public LinkData(TSBaseInfo linkInfo, bool isContrary)
        {
            _linkInfo = linkInfo;
            IsContrary = isContrary;
        }
    }

    public bool LeftLinkIsContrary => leftLinkData.IsContrary;
    public bool RightLinkIsContrary => rightLinkData.IsContrary;
    protected internal LinkData leftLinkData;
    protected internal LinkData rightLinkData;
    public bool HasLinkRight => laneLinkRight > -1;
    public bool HasLinkLeft => laneLinkLeft > -1;
    public int laneLinkRight = -1;
    public int laneLinkLeft = -1;
    public float totalDistance;
    public float maxSpeed = 50;
    public Vector3 conectorA = Vector3.zero;
    public Vector3 conectorB = Vector3.zero;
    public List<Vector3> middlePoints = new List<Vector3>();
    public abstract TSPoints[] Points { get; }
    private int? _hashCode;
    public int Id
    {
        get
        {
            if (_hashCode.HasValue == false)
            {
                _hashCode = GetHashCode();
            }

            return _hashCode.Value;
        }
    }
    public TSLaneInfo.VehicleType vehicleType = ((TSLaneInfo.VehicleType) (-1));
    public abstract TSBaseInfo GetNext(TSLaneInfo.VehicleType _vehicleType);
    public abstract TSBaseInfo GetPrevious(TSLaneInfo.VehicleType _vehicleType);
    //public abstract TSBaseInfo GetNext(TSLaneInfo.VehicleType _vehicleType, out bool reversed);
    //public abstract TSBaseInfo GetPrevious(TSLaneInfo.VehicleType _vehicleType, out bool reversed);
    public abstract bool TryToReserve(TSTrafficAI tsTrafficAI, int pointIndex);
    public abstract SwitchResponse TrySwitchToLink(SwitchDirection direction, int pointIndex, int reservationID = 0, int pointsQty = 0, TSTrafficAI carWhoReserves = null, bool travelingReverse = false);

    public virtual void Init(TSLaneInfo[] lanes)
    {
        _lanes = lanes;
        if (HasLinkLeft)
        {
            leftLinkData = new LinkData(lanes[laneLinkLeft],  lanes[laneLinkLeft].laneLinkLeft !=-1 && lanes[lanes[laneLinkLeft].laneLinkLeft] == this);
        }

        if (HasLinkRight)
        {
            rightLinkData = new LinkData(lanes[laneLinkRight],  lanes[laneLinkRight].laneLinkRight!=-1 && lanes[lanes[laneLinkRight].laneLinkRight] == this);
        }
    }
    
    public bool HasVehicleType(TSLaneInfo.VehicleType vehicleTypeToCompare)
    {
        return (((int) vehicleType & (1 << (int)vehicleTypeToCompare)) > 0);
    }
    
    public enum SwitchDirection
    {
        Left,
        Right
    }
    public struct SwitchResponse
    {
        public bool isContrary;
        public TSBaseInfo newBaseInfo;
        public int newPointIndex;
        public TSPoints newPoint;
    }

    
}