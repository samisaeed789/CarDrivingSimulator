using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LD_Park : MonoBehaviour
{
    
    public Transform SpawnPoint;
    public GameObject[] OnObjets;
    public ParticleSystem[] Confetti;
    void Start()
    {
        ParkingGm.instance.OnLevelStatsLoadedHandler(this);
    }

}
