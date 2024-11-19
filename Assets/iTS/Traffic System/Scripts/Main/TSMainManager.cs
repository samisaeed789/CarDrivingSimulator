using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ITS.Utils;

/// <summary>
/// TS main   This class is responsible for holding all the info for the roads
/// </summary>
[RequireComponent(typeof(TSTrafficLightCheck))]
public class TSMainManager : MonoBehaviour
{
    /// <summary>
    /// The lanes.  Variable that contains all the lanes informaction.
    /// </summary>
    [SerializeField]
    public TSLaneInfo[] lanes;

    /// <summary>
    /// The menu selection.
    /// </summary>
    public int menuSelection = 0;

    /// <summary>
    /// The lane menu selection.
    /// </summary>
    public int laneMenuSelection = 0;

    /// <summary>
    /// The connections menu selection.
    /// </summary>
    public int connectionsMenuSelection = 0;

    /// <summary>
    /// The settings menu selection.
    /// </summary>
    public int settingsMenuSelection = 0;

    /// <summary>
    /// The width of the visual lines that represents the lanes and connectors on the scene view.
    /// </summary>
    public float visualLinesWidth = 5f;

    /// <summary>
    /// The resolution of the lanes.
    /// </summary>
    public float resolution = 4;

    /// <summary>
    /// The resolution of the connectors.
    /// </summary>
    public float resolutionConnectors = 2.3f;

    /// <summary>
    /// The lane curve speed multiplier.
    /// </summary>
    public float laneCurveSpeedMultiplier = 0.7f;

    /// <summary>
    /// The connectors curve speed multiplier.
    /// </summary>
    public float connectorsCurveSpeedMultiplier = 0.5f;

    /// <summary>
    /// The default type of vehicle.  This would be used as the default vehicle type when new lanes are created.
    /// </summary>
    public TSLaneInfo.VehicleType defaultVehicleType = (TSLaneInfo.VehicleType) (-1);

    /// <summary>
    /// The junctions processed.  This is to know if the junctions have been processed for this TSMainManager instance
    /// </summary>
    public bool junctionsProcessed = false;

    /// <summary>
    /// The vehicle type presets.  This are a list of presets that can be used for quicker access to combinations
    /// of vehicle types.
    /// </summary>
    [SerializeField] public List<VehicleTypePresets> vehicleTypePresets = new List<VehicleTypePresets>();

    /// <summary>
    /// The scale factor.  This is used to scale the tool UI that is draw on the scene view, so you could use the 
    /// tool in a miniature world with ease
    /// </summary>
    public float scaleFactor = 1f;

    public Action<int> OnRemovedLanes;


    [System.Serializable]
    public class VehicleTypePresets
    {
        public TSLaneInfo.VehicleType vehicleType;
        public string name;
    }

    [ContextMenu("Merge lanes")]
    private void MergeLaneTest()
    {
        MergeLanes(46, 52);
    }


    public void MergeLanes(int mainLane, int laneToMerge)
    {
        var mainLaneInfo = lanes[mainLane];
        var mergeLaneInfo = lanes[laneToMerge];
        mainLaneInfo.middlePoints.Add(mergeLaneInfo.conectorA);
        mainLaneInfo.middlePoints.AddRange(mergeLaneInfo.middlePoints);
        mainLaneInfo.conectorB = mergeLaneInfo.conectorB;
        lanes[mainLane] = mainLaneInfo;
    }

    [ContextMenu("AutoConnectLanes")]
    private void AutoConnectNearLanes()
    {
        AutoConnectNearLanes(20f, 120f);
    }
    public void AutoConnectNearLanes(float maxDistance, float maxAngle)
    {
        foreach (var lane in lanes)
        {
            var laneDirection = lane.EndDirection;
            var nearLanes = lanes.FindAllIndexNearestFirstPoint(lane.conectorB, maxDistance);

            foreach (var nearLane in nearLanes)
            {
                var nearLaneDirection = lanes[nearLane].StartDirection;
                var angle = Vector3.Angle(laneDirection, nearLaneDirection);
                
                if (lane.IsConnectedToLane(nearLane) == false && lanes[nearLane] != lane && angle < maxAngle)
                {
                    AddConnector<TSLaneConnector>(lane,lanes[nearLane]);
                }
            }
            
        }
    }

    private void Awake()
    {
        InitLanesAndConnectors();
    }

    public void InitLanesAndConnectors()
    {
        foreach (var lane in lanes)
        {
            lane.connectorsReverse = new TSLaneConnector[0];
        }

        foreach (var lane in lanes)
        {
            lane.Init(lanes);

            foreach (var connector in lane.connectors)
            {
                connector.Init(lanes);
                lanes[connector.nextLane].connectorsReverse = lanes[connector.nextLane].connectorsReverse.Add(connector);
                foreach (var point in connector.points)
                {
                    foreach (var otherPointse in point.otherConnectorsPoints)
                    {
                        otherPointse.SetPointReference(lanes[otherPointse.lane], lanes[otherPointse.lane].connectors[otherPointse.connector], lanes[otherPointse.lane].connectors[otherPointse.connector].points[otherPointse.pointIndex]);
                        connector.OtherConnectors.Add(otherPointse.Connector);
                    }
                }
            }
        }
    }

    public void AddLane<T>(Vector3[] points, float tolerance) where T : TSLaneInfo
    {
        var laneInfo = (T) new TSLaneInfo(points[0], points[points.Length-1], resolution, defaultVehicleType);
        var list = points.ToList();
        LineUtility.Simplify(list, tolerance, list);
        
        if (list.Count == 2)
        {
            list.Add((list[0] + list[1])/2f);
            list.RemoveAt(0);
            list.RemoveAt(0);
        }
        else
        {
            list.Remove(list[0]);
            list.Remove(list[list.Count - 1]);
        }

        laneInfo.middlePoints = list;
        laneInfo.CreateWaypoints(resolution);
        lanes = lanes.Add(laneInfo);
    }

    public void AddConnector<T>(int laneFromIndex, int laneToIndex) where T : TSLaneConnector
    {
        var laneFrom = lanes[laneFromIndex];
        var laneTo = lanes[laneToIndex];
        var newConnector = (T) new TSLaneConnector( laneFromIndex, laneToIndex, laneFrom, laneTo, resolutionConnectors);
    }
    
    public void AddConnector<T>(TSLaneInfo laneFrom, TSLaneInfo laneTo) where T : TSLaneConnector
    {
        var laneFromIndex = lanes.FindIndex(laneFrom);
        var laneToIndex = lanes.FindIndex(laneTo);
        var newConnector = (T) new TSLaneConnector( laneFromIndex, laneToIndex, laneFrom, laneTo, resolutionConnectors);
    }
    
    public void AddConnector<T>(TSLaneInfo laneFrom, TSLaneInfo laneTo, Vector3[] points) where T : TSLaneConnector
    {
        var laneFromIndex = lanes.FindIndex(laneFrom);
        var laneToIndex = lanes.FindIndex(laneTo);
        var newConnector = (T) new TSLaneConnector( laneFromIndex, laneToIndex, laneFrom, laneTo, resolutionConnectors);
        var list = points.ToList();
        LineUtility.Simplify(list, 0.5f, list);
        newConnector.middlePoints = list;
        newConnector.CreateWaypoints(resolutionConnectors);
    }
    
    public void RemoveConnector(int selectedLane, int selectedConnector)
    {
        lanes[selectedLane].connectors = lanes[selectedLane].connectors.Remove(lanes[selectedLane].connectors[selectedConnector]);
        junctionsProcessed = false;
    }

    public void AddLane<T>(Vector3 startPoint, Vector3 endPoint) where T : TSLaneInfo
    {
        lanes = lanes.Add((T) new TSLaneInfo(startPoint, endPoint, resolution, defaultVehicleType));
    }
    
    public void RemoveLane(int selectedLane , Action<int> onLaneDeleted)
    {
        if (selectedLane == -1) return;
        OnRemovedLanes?.Invoke(selectedLane);
        for (int q = 0; q < lanes.Length; q++)
        {
            for (int u = 0; u < lanes[q].connectors.Length; u++)
            {
                if (lanes[q].connectors[u].nextLane == selectedLane)
                    lanes[q].connectors = lanes[q].connectors.Remove(lanes[q].connectors[u]);
            }

            for (int u = 0; u < lanes[q].connectors.Length; u++)
            {
                if (lanes[q].connectors[u].nextLane > selectedLane)
                    lanes[q].connectors[u].nextLane--;
            }

            CheckLaneLinking(q, selectedLane);
        }

        lanes = lanes.Remove(lanes[selectedLane]);
        junctionsProcessed = false;
    }
    
    public void SwapLaneDirection(int selectedLane)
    {
        var connectorA = lanes[selectedLane].conectorA;
        var connectorB = lanes[selectedLane].conectorB;
        lanes[selectedLane].conectorA = connectorB;
        lanes[selectedLane].conectorB = connectorA;
        lanes[selectedLane].middlePoints.Reverse();
        Array.Reverse(lanes[selectedLane].points);
    }

    private void CheckLaneLinking(int laneToCheck, int removedLane)
    {
        if (lanes[laneToCheck].laneLinkLeft == removedLane)
        {
            lanes[laneToCheck].laneLinkLeft = -1;
        }

        if (lanes[laneToCheck].laneLinkLeft > removedLane)
        {
            lanes[laneToCheck].laneLinkLeft--;
        }

        if (lanes[laneToCheck].laneLinkRight == removedLane)
        {
            lanes[laneToCheck].laneLinkRight = -1;
        }

        if (lanes[laneToCheck].laneLinkRight > removedLane)
        {
            lanes[laneToCheck].laneLinkRight--;
        }
    }
    

    //****************************************************************************************************************************
    //* DEBUGING CODE FOR VISUALY SEEING THE STATES OF THE LANE POINTS WHILE IN RUNTIME IN THE EDITOR (NOT PERFORMANCE FRIENDLY)
    //****************************************************************************************************************************
    /*
    
    Bounds bounds = new Bounds();
    Camera mainC;

    void OnDrawGizmos()
    {
        if (mainC == null) mainC = GameObject.FindObjectOfType(typeof(Camera)) as Camera;
        if (lanes == null) return;
        for (int i = 0; i < lanes.Length; i++)
        {
            bounds = new Bounds(lanes[i].conectorA, Vector3.one);
            bounds.Encapsulate(lanes[i].conectorB);
            bool draw = false;
            for (int w = 0; w < lanes[i].points.Length; w++)
            {
                if (w % 10 == 0)
                    bounds.Encapsulate(lanes[i].points[w].point);
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Application.isPlaying
                    ? mainC
                    : UnityEditor.SceneView.GetAllSceneCameras()[0]);
                if (GeometryUtility.TestPlanesAABB(planes, bounds))
                    draw = true;
                if (draw)
                {
                    if (lanes[i].points[w].CarWhoReserved != null)
                        Gizmos.color =
                            lanes[i].points[w].CarWhoReserved
                                .myColor; // new Color(1.2f*Mathf.Abs(lanes[i].points[w].reservationID)/20000f,1.5f*Mathf.Abs(lanes[i].points[w].reservationID)/20000f,0.5f *Mathf.Abs(lanes[i].points[w].reservationID)/20000f);
                    else if (lanes[i].points[w].ReservationID == 0)
                        Gizmos.color = Color.blue;
                    else Gizmos.color = Color.red;
                    Gizmos.DrawCube(lanes[i].points[w].point, Vector3.one);
                }
            }

            if (draw)
            {
                for (int c = 0; c < lanes[i].connectors.Length; c++)
                {
                    if (lanes[i].connectors[c].IsReserved)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(lanes[i].connectors[c].conectorA, lanes[i].connectors[c].conectorB);
                    }

                    for (int r = 0; r < lanes[i].connectors[c].points.Length; r++)
                    {
                        if (lanes[i].connectors[c].points[r].LaneReservationID != -1)
                            Gizmos.color =
                                Color.red; // new Color(1.2f*Mathf.Abs(lanes[i].connectors[c].points[r].laneReservationID)/200f,1.5f*Mathf.Abs(lanes[i].connectors[c].points[r].laneReservationID)/200f,0.5f *Mathf.Abs(lanes[i].connectors[c].points[r].laneReservationID)/200f);
                        else
                        {
                            if (lanes[i].connectors[c].points[r].CarWhoReserved != null)
                                Gizmos.color = lanes[i].connectors[c].points[r].CarWhoReserved.myColor;//new Color(1.2f*Mathf.Abs(lanes[i].connectors[c].points[r].reservationID)/20000f,1.5f*Mathf.Abs(lanes[i].connectors[c].points[r].reservationID)/20000f,0.5f *Mathf.Abs(lanes[i].connectors[c].points[r].reservationID)/20000f);
                            else if (lanes[i].connectors[c].points[r].ReservationID != 0)
                            {
                                Gizmos.color = Color.yellow;
                            }
                            else
                                Gizmos.color =
                                    Color.magenta; // new Color((float)c / (float)lanes[i].connectors.Length, (float)i / (float)lanes.Length, (1f - ((float)c / (float)lanes[i].connectors.Length)) * 0.5f);
                        }

                        Gizmos.DrawCube(lanes[i].connectors[c].points[r].point, Vector3.one);
                    }
                }
            }
        }
    }*/
    
    
    public void Clear()
    {
        lanes = new TSLaneInfo[0];
    }
    
    public void ProcessJunctions(bool calculateNerbyPointsForPlayerFinding, float nearbyPointsRadius, Action<string, string, float> progressCallback = null, Action completed = null)
    {
        RemoveBadConnectors(progressCallback);
        var progress = 0f;
        float totalProgress = lanes.Length;
        progressCallback?.Invoke("Processing Roads Data", "Processing Junctions", progress);
        for (var laneIndexP = 0; laneIndexP < lanes.Length; laneIndexP++)
        {
            //Speed Limit of lanes Inclusion on the track data
            for (var point = 0; point < lanes[laneIndexP].points.Length; point++)
            {
                lanes[laneIndexP].points[point].nearbyPoints = new TSConnectorOtherPoints[0];
                if (point + 2 < lanes[laneIndexP].points.Length)
                {
                    var point1 = lanes[laneIndexP].points[point].point;
                    var point2 = lanes[laneIndexP].points[point + 1].point;
                    var point3 = lanes[laneIndexP].points[point + 2].point;
                    var tempDir = Quaternion.LookRotation(point2 - point1);
                    var tempDir1 = Quaternion.LookRotation(point3 - point2);
                    var angle2 = Quaternion.Angle(tempDir1, tempDir);
                    if (angle2 < 5)
                    {
                        lanes[laneIndexP].points[point].maxSpeedLimit = float.MaxValue;
                    }
                    else if (angle2 < 10)
                        lanes[laneIndexP].points[point].maxSpeedLimit = 50f * laneCurveSpeedMultiplier;
                    else if (angle2 < 15)
                        lanes[laneIndexP].points[point].maxSpeedLimit = 40f * laneCurveSpeedMultiplier;
                    else if (angle2 < 20)
                        lanes[laneIndexP].points[point].maxSpeedLimit = 30f * laneCurveSpeedMultiplier;
                    else if (angle2 >= 20)
                        lanes[laneIndexP].points[point].maxSpeedLimit = 25f * laneCurveSpeedMultiplier;
                }

                lanes[laneIndexP].points[point].leftParallelPointIndex = -1;
                lanes[laneIndexP].points[point].rightParallelPointIndex = -1;
                lanes[laneIndexP].points[point].leftParallelConnectorIndex = -1;
                lanes[laneIndexP].points[point].rightParallelConnectorIndex = -1;
                //lanes[laneIndexP].points[point].ConnectorReservationCount = 0;
            }

            for (var connectorIndexP = 0;
                connectorIndexP < lanes[laneIndexP].connectors.Length;
                connectorIndexP++)
            {
                lanes[laneIndexP].connectors[connectorIndexP].previousLane = laneIndexP;

                for (var pointIndexP = 0;
                    pointIndexP < lanes[laneIndexP].connectors[connectorIndexP].points.Length;
                    pointIndexP++)
                {
                    lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].otherConnectorsPoints =
                        new TSConnectorOtherPoints[0]; // List<TSConnetorOtherPoints>();
                    lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].nearbyPoints =
                        new TSConnectorOtherPoints[0];
                    lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP]
                        .ConnectorReservationCount = 0;

                    //Speed Limit of connectors Inclusion on the track data

                    if (pointIndexP + 2 < lanes[laneIndexP].connectors[connectorIndexP].points.Length)
                    {
                        var point1 = lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP]
                            .point;
                        var point2 = lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP + 1]
                            .point;
                        var point3 = lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP + 2]
                            .point;
                        var tempDir = Quaternion.LookRotation(point2 - point1);
                        var tempDir1 = Quaternion.LookRotation(point3 - point2);
                        var angle2 = Quaternion.Angle(tempDir1, tempDir);
                        if (angle2 < 5)
                        {
                            lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].maxSpeedLimit =
                                float.MaxValue;
                        }
                        else if (angle2 < 10)
                            lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].maxSpeedLimit =
                                50f * connectorsCurveSpeedMultiplier;
                        else if (angle2 < 15)
                            lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].maxSpeedLimit =
                                40f * connectorsCurveSpeedMultiplier;
                        else if (angle2 < 20)
                            lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].maxSpeedLimit =
                                30f * connectorsCurveSpeedMultiplier;
                        else if (angle2 >= 20)
                            lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].maxSpeedLimit =
                                25f * connectorsCurveSpeedMultiplier;
                    }


                    //Compare fors
                    for (var laneIndexC = 0; laneIndexC < lanes.Length; laneIndexC++)
                    {
                        for (var connectorIndexC = 0;
                            connectorIndexC < lanes[laneIndexC].connectors.Length;
                            connectorIndexC++)
                        {
                            if (connectorIndexP == connectorIndexC && laneIndexP == laneIndexC) continue;
                            for (var pointIndexC = 0;
                                pointIndexC < lanes[laneIndexC].connectors[connectorIndexC].points.Length;
                                pointIndexC++)
                            {
                                var distance = Vector3.Distance(
                                    lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP].point,
                                    lanes[laneIndexC].connectors[connectorIndexC].points[pointIndexC].point);
                                if (distance < lanes[laneIndexP].laneWidth)
                                {
                                    var otherPoint = new TSConnectorOtherPoints();
                                    otherPoint.lane = laneIndexC;
                                    otherPoint.connector = connectorIndexC;
                                    otherPoint.pointIndex = pointIndexC;
                                    lanes[laneIndexP].connectors[connectorIndexP].points[pointIndexP]
                                        .otherConnectorsPoints = lanes[laneIndexP].connectors[connectorIndexP]
                                        .points[pointIndexP].otherConnectorsPoints.Add(otherPoint);
                                }
                            }
                        }
                    }
                }
            }

            progressCallback?.Invoke("Processing Roads Data", "Processing Junctions", laneIndexP / totalProgress);
        }


        //Processing parallel Lanes
        progress = 0f;
        progressCallback?.Invoke("Processing Roads Data", "Processing parallel lanes", progress);
        InitLanesAndConnectors();
        for (var laneIndex1 = 0; laneIndex1 < lanes.Length; laneIndex1++)
        {
            lanes[laneIndex1].Init(lanes);
            for (var laneIndex2 = 0; laneIndex2 < lanes.Length; laneIndex2++)
            {
                lanes[laneIndex2].Init(lanes);
                var left = false;
                var right = false;
                var otherIsLeft = false;
                var otherIsRigth = false;

                if (lanes[laneIndex1].laneLinkLeft == laneIndex2)
                {
                    left = true;
                }

                if (lanes[laneIndex1].laneLinkRight == laneIndex2)
                {
                    right = true;
                }

                if (lanes[laneIndex2].laneLinkLeft == laneIndex1)
                {
                    otherIsLeft = true;
                }

                if (lanes[laneIndex2].laneLinkRight == laneIndex1)
                {
                    otherIsRigth = true;
                }

                if (left || right)
                {
                    for (var pointIndex1 = 0; pointIndex1 < lanes[laneIndex1].points.Length; pointIndex1++)
                    {
                        var pointIndexParallel = WaypointsUtils.GetNearestWayppoint(lanes[laneIndex2].points,
                            lanes[laneIndex1].points[pointIndex1].point);

                        if (left)
                        {
                            lanes[laneIndex1].points[pointIndex1].leftParallelPointIndex = pointIndexParallel;
                        }
                        else if (right)
                        {
                            lanes[laneIndex1].points[pointIndex1].rightParallelPointIndex = pointIndexParallel;
                        }

                        if (otherIsLeft)
                        {
                            lanes[laneIndex2].points[pointIndexParallel].leftParallelPointIndex = pointIndex1;
                        }
                        else if (otherIsRigth)
                        {
                            lanes[laneIndex2].points[pointIndexParallel].rightParallelPointIndex = pointIndex1;
                        }
                    }

                    var isContrary = (left && lanes[laneIndex1].LeftLinkIsContrary) ||
                                     (right && lanes[laneIndex1].RightLinkIsContrary);
                    for (var index = 0; index < lanes[laneIndex1].connectors.Length; index++)
                    {
                        var connector = lanes[laneIndex1].connectors[index];
                        int laneConnectorIndex1 = connector.nextLane;
                        for (var i = 0;
                            i < (isContrary
                                ? lanes[laneIndex2].connectorsReverse
                                : lanes[laneIndex2].connectors).Length;
                            i++)
                        {
                            var laneConnector =
                                (isContrary
                                    ? lanes[laneIndex2].connectorsReverse
                                    : lanes[laneIndex2].connectors)[i];
                            int laneConnectorIndex2 = isContrary ? laneConnector.previousLane : laneConnector.nextLane;
                            if (lanes[laneConnectorIndex1].laneLinkLeft == laneConnectorIndex2)
                            {
                                left = true;
                            }

                            if (lanes[laneConnectorIndex1].laneLinkRight == laneIndex2)
                            {
                                right = true;
                            }

                            if (lanes[laneConnectorIndex2].laneLinkLeft == laneConnectorIndex1)
                            {
                                otherIsLeft = true;
                            }

                            if (lanes[laneConnectorIndex2].laneLinkRight == laneConnectorIndex1)
                            {
                                otherIsRigth = true;
                            }

                            if (left || right)
                            {
                                for (var pointIndex1 = 0; pointIndex1 < connector.points.Length; pointIndex1++)
                                {
                                    var pointIndexParallel = WaypointsUtils.GetNearestWayppoint(laneConnector.points,
                                        connector.points[pointIndex1].point);

                                    if (left)
                                    {
                                        connector.points[pointIndex1].leftParallelPointIndex = pointIndexParallel;
                                        connector.points[pointIndex1].leftParallelConnectorIndex = i;
                                    }
                                    else if (right)
                                    {
                                        connector.points[pointIndex1].rightParallelPointIndex = pointIndexParallel;
                                        connector.points[pointIndex1].rightParallelConnectorIndex = i;
                                    }

                                    if (otherIsLeft)
                                    {
                                        laneConnector.points[pointIndexParallel].leftParallelPointIndex = pointIndex1;
                                        laneConnector.points[pointIndexParallel].leftParallelConnectorIndex = index;
                                    }
                                    else if (otherIsRigth)
                                    {
                                        laneConnector.points[pointIndexParallel].rightParallelPointIndex = pointIndex1;
                                        laneConnector.points[pointIndexParallel].rightParallelConnectorIndex = index;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            progressCallback?.Invoke("Processing Roads Data", "Removing duplicates",laneIndex1 / totalProgress);
        }


        //Eliminate Duplicates
        progress = 0;
        progressCallback?.Invoke("Processing Roads Data", "Removing duplicates", progress);
        for (var laneIndexP = 0; laneIndexP < lanes.Length; laneIndexP++)
        {
            var lane = lanes[laneIndexP];
            for (var connectorIndexP = 0; connectorIndexP < lane.connectors.Length; connectorIndexP++)
            {
                var connector = lane.connectors[connectorIndexP];
                for (var pointIndexC = 0; pointIndexC < connector.points.Length; pointIndexC++)
                {
                    var point = connector.points[pointIndexC];

                    for (var otherPointsIndex = point.otherConnectorsPoints.Length - 1;
                        otherPointsIndex >= 0;
                        otherPointsIndex--)
                    {
                        var otherPoint = point.otherConnectorsPoints[otherPointsIndex];
                        for (var SecondPointsIndex = 0;
                            SecondPointsIndex < connector.points.Length;
                            SecondPointsIndex++)
                        {
                            var pointC = connector.points[SecondPointsIndex];
                            if (pointIndexC != SecondPointsIndex)
                            {
                                for (var otherPointsIndexC = pointC.otherConnectorsPoints.Length - 1;
                                    otherPointsIndexC >= 0;
                                    otherPointsIndexC--)
                                {
                                    var otherPointC =
                                        pointC.otherConnectorsPoints[otherPointsIndexC];
                                    if (otherPointsIndex < point.otherConnectorsPoints.Length)
                                    {
                                        if (otherPoint.connector == otherPointC.connector &&
                                            otherPoint.lane == otherPointC.lane &&
                                            otherPoint.pointIndex == otherPointC.pointIndex)
                                        {
                                            var pDistance = Vector3.Distance(point.point,
                                                lanes[otherPoint.lane].connectors[otherPoint.connector]
                                                    .points[otherPoint.pointIndex].point);
                                            var pCDistance = Vector3.Distance(pointC.point,
                                                lanes[otherPointC.lane].connectors[otherPointC.connector]
                                                    .points[otherPointC.pointIndex].point);
                                            if (pDistance < pCDistance)
                                                pointC.otherConnectorsPoints =
                                                    pointC.otherConnectorsPoints.Remove(otherPointC);
                                            else
                                                point.otherConnectorsPoints =
                                                    point.otherConnectorsPoints.Remove(otherPointC);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            progressCallback?.Invoke("Processing Roads Data", "Removing duplicates",laneIndexP / totalProgress);
        }


        if (calculateNerbyPointsForPlayerFinding)
        {
            FindNearbyPoints(nearbyPointsRadius, progressCallback);
        }
        
        junctionsProcessed = true;
        completed?.Invoke();
    }

    private void FindNearbyPoints(float nearbyPointsRadius, Action<string,string,float> ProgressCallback)
    {
        float progress = 0;
        float totalProgress = lanes.Length;
        ProgressCallback?.Invoke("Processing Roads Data", "Finding nearby points", progress);
        for (int laneIndexP = 0; laneIndexP < lanes.Length; laneIndexP++)
        {
            TSLaneInfo lane = lanes[laneIndexP];

            for (int lanePointIndex = 0; lanePointIndex < lane.points.Length; lanePointIndex++)
            {
                TSPoints currenP = lane.points[lanePointIndex];

                for (int laneIndexS = 0; laneIndexS < lanes.Length; laneIndexS++)
                {
                    TSLaneInfo laneS = lanes[laneIndexS];
                    for (int lanePointIndexS = 0; lanePointIndexS < laneS.points.Length; lanePointIndexS++)
                    {
                        TSPoints searchP = laneS.points[lanePointIndexS];
                        if (currenP != searchP)
                        {
                            float distance = Vector3.Distance(currenP.point, searchP.point);
                            if (distance < nearbyPointsRadius)
                            {
                                TSOtherPoints otherPoint = new TSOtherPoints();
                                otherPoint.lane = laneIndexS;
                                otherPoint.connector = -1;
                                otherPoint.pointIndex = lanePointIndexS;
                                currenP.nearbyPoints = currenP.nearbyPoints.Add(otherPoint);
                            }
                        }
                    }

                    for (int connectorIndexP = 0; connectorIndexP < laneS.connectors.Length; connectorIndexP++)
                    {
                        TSLaneConnector connector = laneS.connectors[connectorIndexP];
                        for (int pointIndexC = 0; pointIndexC < connector.points.Length; pointIndexC++)
                        {
                            TSPoints point = connector.points[pointIndexC];
                            if (currenP != point)
                            {
                                float distance = Vector3.Distance(currenP.point, point.point);
                                if (distance < nearbyPointsRadius)
                                {
                                    TSOtherPoints otherPoint = new TSOtherPoints();
                                    otherPoint.lane = laneIndexS;
                                    otherPoint.connector = connectorIndexP;
                                    otherPoint.pointIndex = pointIndexC;
                                    currenP.nearbyPoints = currenP.nearbyPoints.Add(otherPoint);
                                }
                            }
                        }
                    }
                }
            }


            for (int connectorIndexP = 0; connectorIndexP < lane.connectors.Length; connectorIndexP++)
            {
                TSLaneConnector connector = lane.connectors[connectorIndexP];
                for (int pointIndexC = 0; pointIndexC < connector.points.Length; pointIndexC++)
                {
                    TSPoints currenP = connector.points[pointIndexC];

                    for (int laneIndexS = 0; laneIndexS < lanes.Length; laneIndexS++)
                    {
                        TSLaneInfo laneS = lanes[laneIndexS];
                        for (int lanePointIndexS = 0; lanePointIndexS < laneS.points.Length; lanePointIndexS++)
                        {
                            TSPoints searchP = laneS.points[lanePointIndexS];
                            if (currenP != searchP)
                            {
                                float distance = Vector3.Distance(currenP.point, searchP.point);
                                if (distance < nearbyPointsRadius)
                                {
                                    TSOtherPoints otherPoint = new TSOtherPoints();
                                    otherPoint.lane = laneIndexS;
                                    otherPoint.connector = -1;
                                    otherPoint.pointIndex = lanePointIndexS;
                                    currenP.nearbyPoints = currenP.nearbyPoints.Add(otherPoint);
                                }
                            }
                        }

                        for (int connectorIndexS = 0; connectorIndexS < laneS.connectors.Length; connectorIndexS++)
                        {
                            TSLaneConnector connectorS = laneS.connectors[connectorIndexS];
                            for (int pointIndexS = 0; pointIndexS < connectorS.points.Length; pointIndexS++)
                            {
                                TSPoints pointS = connectorS.points[pointIndexS];
                                if (currenP != pointS)
                                {
                                    float distance = Vector3.Distance(currenP.point, pointS.point);
                                    if (distance < nearbyPointsRadius)
                                    {
                                        TSOtherPoints otherPoint = new TSOtherPoints();
                                        otherPoint.lane = laneIndexS;
                                        otherPoint.connector = connectorIndexS;
                                        otherPoint.pointIndex = pointIndexS;
                                        currenP.nearbyPoints = currenP.nearbyPoints.Add(otherPoint);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ProgressCallback?.Invoke("Processing Roads Data", "Finding nearby points", laneIndexP / totalProgress);
        }
    }


    private void RemoveBadConnectors(Action<string,string,float> progressCallback)
    {
        float totalProgress = lanes.Length;
        float progress = 0f;
        progressCallback?.Invoke("Processing Roads Data", "Removing invalid connectors", progress);
        for (int laneIndexP = 0; laneIndexP < lanes.Length; laneIndexP++)
        {
            for (int connectorIndexP = 0;
                connectorIndexP < lanes[laneIndexP].connectors.Length;
                connectorIndexP++)
            {
                if (lanes[laneIndexP].connectors[connectorIndexP].points.Length == 0)
                {
                    RemoveConnector(laneIndexP, connectorIndexP);
                    Debug.LogWarning("Removing bad connector at lane->" + laneIndexP + " Connector->" + connectorIndexP);
                    laneIndexP--;
                    break;
                }
            }

            progressCallback?.Invoke("Processing Roads Data", "Removing invalid connectors",laneIndexP / totalProgress);
        }
    }

    public void Save(string path)
    {
        using (StreamWriter stream = new StreamWriter(new FileStream(path, FileMode.Create), System.Text.Encoding.UTF8))
        {
            var jsonstring = JsonUtility.ToJson(this);
            stream.Write(jsonstring);
        }
    }
    
    public void Load(string path)
    {
        using (StreamReader stream = new StreamReader(new FileStream(path, FileMode.Open), System.Text.Encoding.UTF8))
        {
            GameObject newObject = new GameObject();
            var mainManager = newObject.AddComponent<TSMainManager>();
            JsonUtility.FromJsonOverwrite(stream.ReadToEnd(), mainManager);
            lanes = mainManager.lanes;
            DestroyImmediate(newObject);
        }
    }
    
}