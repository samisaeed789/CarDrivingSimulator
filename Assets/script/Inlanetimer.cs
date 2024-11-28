using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inlanetimer : MonoBehaviour
{

    float timer;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && ValStorage.timerforlane >= 7)
        {
            ValStorage.timerforlane += Time.deltaTime;  // Increment the timer as long as player is in the trigger

            // If the player stays for the full 5 seconds
            if (ValStorage.timerforlane >= 7)
            {
                GameMngr.instance.AppreciateCoinAdd("You Followed Lane Rule");
              
            }
        }
    }
}
