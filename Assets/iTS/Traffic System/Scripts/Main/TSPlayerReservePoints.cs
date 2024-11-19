using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ITS.Utils;

[RequireComponent(typeof(TSPlayerFinder))]
public class TSPlayerReservePoints : MonoBehaviour
{


    public float unReservePointTimeInterval = 0.5f;
    public bool useFixedReservationDistance = true;
    public float fixedReservetaionDistance = 10f;
    public float unReserveDistance = 2;
    private TSPlayerFinder playerFinder;
    private float currentSpeedSqr;
    private float c;
    private float maxLockaheadDistance;
    private Rigidbody carBody;
    private float carSpeed;
    private Vector3 localspeed;
    private Transform myTransform;
    private float segDistance;
    private bool Initialized = false;
    WaitForSeconds w;
    Queue<TSPoints> reservedPlayerPoints = new Queue<TSPoints>();
    int currentLane = 0;
    int currentConnector = -1;
    int currentPoint = 0;
    int currentPointReserved = 0;
    int ID;
    TSPoints currentTSPoint;
    TSMainManager manager;
    bool isOnConnector = false;

    /// <summary>
    /// This field would have positive value if the car forward direction is pointing on the same direction as the current point it is traveling on, otherwise it would be negative value
    /// </summary>
    float dirRespectToCurrentPoint = 0;

    void OnEnable()
    {
        playerFinder = GetComponent<TSPlayerFinder>();
        playerFinder.onPointChanged += OnPointChanged;
    }

    // Use this for initialization
    void Start()
    {
        manager = GameObject.FindObjectOfType<TSMainManager>();
        ID = TSUtils.GetUniqueID();
        carBody = GetComponent<Rigidbody>();
        myTransform = transform;
        c = 0.1f * -Physics.gravity.y;
        w = new WaitForSeconds(unReservePointTimeInterval);
        StartCoroutine(UnReservePoints());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        localspeed = myTransform.InverseTransformDirection(carBody.velocity);
        carSpeed = localspeed.magnitude;
        currentSpeedSqr = carSpeed * carSpeed;
        GetMaxLockAheadDistance();
        CompareDirWithCurrentLaneDir();
        ReservePoints();
    }

    /// <summary>
    /// Gets the max lock ahead distance.
    /// </summary>
    void GetMaxLockAheadDistance()
    {
        if (useFixedReservationDistance)
        {
            maxLockaheadDistance = fixedReservetaionDistance;
            return;
        }
        maxLockaheadDistance = (((currentSpeedSqr) / (2.0f * c))) + 15;
        if (maxLockaheadDistance < 0)
            maxLockaheadDistance = 0;
    }

    void CompareDirWithCurrentLaneDir()
    {
        Vector3 currentLaneDir = Vector3.zero;
        if (currentConnector == -1)
        {
            isOnConnector = false;
            int nextPoint = currentPoint + 1;
            if (nextPoint < manager.lanes[currentLane].points.Length)
                currentLaneDir = (manager.lanes[currentLane].points[currentPoint].point - manager.lanes[currentLane].points[nextPoint].point).normalized;
            dirRespectToCurrentPoint = -Vector3.Dot(myTransform.forward, currentLaneDir);
        }
        else
        {
            isOnConnector = true;
            int nextPoint = currentPoint + 1;
            if (nextPoint < manager.lanes[currentLane].points.Length)
                currentLaneDir = (manager.lanes[currentLane].points[currentPoint].point - manager.lanes[currentLane].points[nextPoint].point).normalized;
            dirRespectToCurrentPoint = -Vector3.Dot(myTransform.forward, currentLaneDir);
        }
    }



    void OnPointChanged(int lane, int connector, int point)
    {
        if (lane != currentLane || currentConnector != connector)
        {
            currentPointReserved = point;  //Player changed Lane, start over the reservation of the lane points
            segDistance = 0;
        }
        currentLane = lane;
        currentPoint = point;
        currentConnector = connector;
        currentTSPoint = GetPoint(currentLane, currentPoint, currentConnector);
        Initialized = true;
    }

    void ReservePoints()
    {
        if (Initialized == false) { return; }
        //if (dirRespectToCurrentPoint < 0) return; //We are going backwards! so no points reservation are made!
        bool cont = true;
        while (segDistance <= maxLockaheadDistance && cont)// && sameLane)
        {
            TSPoints point = GetPoint(currentLane, currentPointReserved, currentConnector);
            if (point.TryReservePoint(ID))
            {
                reservedPlayerPoints.Enqueue(point);
                cont = MoveToNextPoint();
                if (!cont) break;
            }
            else if (point.ReservationID != ID)
            {
                break; //Is not a free point, we cant continue
            }
            else
            {
                cont = MoveToNextPoint();
                if (!cont) break;
            }
        }
        currentPointReserved = currentPoint;
        segDistance = 0;
    }

    TSPoints GetPoint(int lane, int point, int connector)
    {
        if (connector == -1)
            return manager.lanes[lane].points[point];
        else return manager.lanes[lane].connectors[connector].points[point];
    }

    bool MoveToNextPoint()
    {
        segDistance += GetPoint(currentLane, currentPointReserved, currentConnector).distanceToNextPoint;
        currentPointReserved++;
        if (currentConnector == -1)
        {
            if (currentPointReserved >= manager.lanes[currentLane].points.Length - 1)
            {
                ReserveAllConnectors(currentLane);
                currentPointReserved = currentPoint;
                return false;
            }
        }
        else
        {
            if (currentPointReserved >= manager.lanes[currentLane].connectors[currentConnector].points.Length - 1)
            {
                currentPointReserved = currentPoint;
                return false;
            }
        }
        return true;
    }

    void ReserveAllConnectors(int lane)
    {
        for (int i = 0; i < manager.lanes[lane].connectors.Length; i++)
        {
            ReserveConnectorsPoints(manager.lanes[lane].connectors[i]);
        }
    }

    void ReserveConnectorsPoints(TSLaneConnector connector)
    {
        for (int i = 0; i < connector.points.Length; i++)
        {
            if (connector.points[i].TryReservePoint(ID))
            {
                reservedPlayerPoints.Enqueue(connector.points[i]);
            }
            else if (connector.points[i].ReservationID != ID)
            {
                break;
            }
        }
    }

    

    IEnumerator UnReservePoints()
    {
        while (true)
        {
            if (reservedPlayerPoints.Count > 0)
            {
                
                TSPoints point = reservedPlayerPoints.Peek();
                Vector3 localPDistance = myTransform.InverseTransformPoint(point.point);
                if (point != currentTSPoint && localPDistance.z < -unReserveDistance || Mathf.Abs(localPDistance.x) > 10)
                {
                    segDistance -= point.distanceToNextPoint;
                    point.TryUnReservePoint(ID);
                    reservedPlayerPoints.Dequeue();
                }
            }
            yield return w;
        }
    }

}
