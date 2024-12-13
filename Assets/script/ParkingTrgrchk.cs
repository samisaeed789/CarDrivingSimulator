using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingTrgrchk : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            ParkingGm.instance.CarFinalPark();
        }
    }
}
