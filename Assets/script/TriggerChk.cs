using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChk : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]bool TurnIndi;
    [SerializeField]bool TrafficSignal;
    [SerializeField]bool StayInLane;


    bool isInTrigger;
    float timer;


    CarData plyrcar;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            plyrcar = other.transform.GetComponentInParent<CarData>();    //other.gameObject.GetComponent<CarData>();
            Debug.Log("plyrcar==" + plyrcar.gameObject.name);
            if (TurnIndi)
            {
                if (plyrcar.currentState == PlayerState.LeftIndicator || plyrcar.currentState == PlayerState.RightIndicator)
                {
                    GameMngr.instance.AppreciateCoinAdd("You Followed Indicator Rule");
                }
                else
                {
                    GameMngr.instance.DiscourageCoinDeduct("You Should Follow Indicator Rule");
                }
            }

            if (TrafficSignal)
            {
                isInTrigger = true;  // Player entered the trigger
                timer = 0f;  // Reset the timer
            }

            if (StayInLane)
            {
                GameMngr.instance.DiscourageCoinDeduct("You Should Stay in your Lane");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isInTrigger && other.CompareTag("Player"))
        {
            timer += Time.deltaTime;  // Increment the timer as long as player is in the trigger

            // If the player stays for the full 5 seconds
            if (timer >= 4)
            {
                GameMngr.instance.EnableGreen();  // Call the appreciate method
                isInTrigger = false;  // Prevent further calls to appreciate
            }
        }

        if (StayInLane) 
        {

          //  GameMngr.instance.IsCorrectLane = true;
            GameMngr.instance.CheckStayLane();
        }
    }

    // Called when another collider exits the trigger zone
    private void OnTriggerExit(Collider other)
    {
        if (TrafficSignal) 
        {
            if (other.CompareTag("Player"))
            {
                if (timer < 4)
                {
                     GameMngr.instance.DiscourageCoinDeduct("You Should Follow Signal Rule");  // Call the warning method if the player exits early
                }
                isInTrigger = false;  // Reset trigger state
            }
        }

        if (StayInLane)
        {

           
                //GameMngr.instance.IsCorrectLane = false;
                GameMngr.instance.CheckOutLane();


        }
    }
}

