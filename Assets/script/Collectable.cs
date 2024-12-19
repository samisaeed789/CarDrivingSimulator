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

                if(ValStorage.modeSel=="DrivingMode")
                    GameMngr.instance?.CollectablePlay(isCoin: true);
                else
                    ParkingGm.instance?.CollectablePlay(isCoin: true);

            }
            if (Cash) 
            {
                if (ValStorage.modeSel == "DrivingMode") 
                    GameMngr.instance?.CollectablePlay(isCash: true);

                else
                    ParkingGm.instance?.CollectablePlay(isCash: true);
            }
            this.transform.parent.gameObject.SetActive(false);

        }
    }
   
}
