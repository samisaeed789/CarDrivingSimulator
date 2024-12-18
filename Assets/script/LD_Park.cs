using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LD_Park : MonoBehaviour
{
    
    public Transform SpawnPoint;
    public Transform SpawnPointJeep;
    public Transform SpawnPointAudi;
    public Transform SpawnPointBugatti;
    public GameObject[] OnObjets;
    public GameObject DanceChar;
    public ParticleSystem[] Confetti;
    void Start()
    {
        ParkingGm.instance.OnLevelStatsLoadedHandler(this);
    }

}
