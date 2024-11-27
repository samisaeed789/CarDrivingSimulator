using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChk : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]bool TurnIndi;


    CarData plyrcar;
    private void Start()
    {
        plyrcar = GameMngr.instance.Car.GetComponent<CarData>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TurnIndi) 
        {
            if (other.CompareTag("Player"))
            {
                if(plyrcar.currentState == PlayerState.LeftIndicator) 
                {
                    GameMngr.instance.AppreciateCoinAdd();
                }
                else 
                {
                    GameMngr.instance.DiscourageCoinDeduct();

                }

            }
            
        }
    }

}
