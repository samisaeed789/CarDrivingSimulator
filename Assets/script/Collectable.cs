using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField]bool Coins;
    [SerializeField]bool Cash;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") ) 
        {
            if (Coins) 
            {
                GameMngr.instance?.CollectablePlay(isCoin: true);

                ParkingGm.instance?.CollectablePlay(isCoin: true);

            }
            if (Cash) 
            {
                GameMngr.instance?.CollectablePlay(isCash: true);
                ParkingGm.instance?.CollectablePlay(isCash: true);
            }
            this.transform.parent.gameObject.SetActive(false);

        }
    }
   
}
