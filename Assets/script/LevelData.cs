using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaypointsFree;

public class LevelData : MonoBehaviour
{
    public GameObject FinalP;
    public Transform Dancing;
    public WaypointsGroup[] WPs;
  
    void Start()
    {
        GameMngr.instance.OnLevelStatsLoadedHandler(this);
    }

   
}
