using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChk : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]bool TurnIndi;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag =="Player")
        {

        }
    }

}
