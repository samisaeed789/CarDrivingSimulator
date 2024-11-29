using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaypointsFree;

public class LevelData : MonoBehaviour
{
    public Transform SpawnPoint;
    public Transform dancetrans;
    public Transform Pedestians;
    public GameObject Cam;
    public GameObject Filler;
    public GameObject[] greenred;
    public bool IsDisabledTraffic;
    public bool IsStayinLane;
  
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0f);
        GameMngr.instance.OnLevelStatsLoadedHandler(this);
    }

   
}
