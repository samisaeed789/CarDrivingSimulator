using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaypointsFree;

public class LevelData : MonoBehaviour
{
    public Transform SpawnPoint;
    public Transform dancetrans;
    public GameObject[] greenred;
  
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0f);
        GameMngr.instance.OnLevelStatsLoadedHandler(this);
    }

   
}
