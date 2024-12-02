using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChk : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]bool TurnIndi;
    [SerializeField]bool TrafficSignal;
    [SerializeField]bool StayInLane;
    [SerializeField]bool SpeedLimit;
    [SerializeField]bool Pedestrian;
    [SerializeField]bool PoliceChk;
    [SerializeField]bool LeftIndi;
    [SerializeField]bool RightIndi;


    bool isInTrigger;
    float timer;
    float pedesttimer;
    bool IsPedestCrossed;


    CarData plyrcar;
    [SerializeField]RCC_CarControllerV3 Car;


    private void OnEnable()
    {
        if (GameMngr.instance) 
        {
            GameMngr.instance.OnCarSet += HandleCarSet;
        }
    }
    private void HandleCarSet(RCC_CarControllerV3 car)
    {
        Car = car;

    }

   

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            plyrcar = other.transform.GetComponentInParent<CarData>();    //other.gameObject.GetComponent<CarData>();
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
                GameMngr.instance.LaneTimer = 0f;
                GameMngr.instance.DiscourageCoinDeduct("You Should Stay in your Lane");
            }
            
            if (Pedestrian)
            {
                GameMngr.instance.PlayWalk();
            }

            if (PoliceChk) 
            {
                if (!GameMngr.instance.IsStoppedAtPolice) 
                {
                    GameMngr.instance.DiscourageCoinDeduct("You Should Stop At CheckPoint");
                }
            }  
            
            
            if (LeftIndi) 
            {
                if (plyrcar.currentState == PlayerState.LeftIndicator)
                {
                    GameMngr.instance.AppreciateCoinAdd("You Followed Indicator Rule");
                }
                else
                {
                    GameMngr.instance.DiscourageCoinDeduct("You Should Follow Indicator Rule");
                }
            }

            if (RightIndi)
            {
                if (plyrcar.currentState == PlayerState.RightIndicator)
                {
                    GameMngr.instance.AppreciateCoinAdd("You Followed Indicator Rule");
                }
                else
                {
                    GameMngr.instance.DiscourageCoinDeduct("You Should Follow Indicator Rule");
                }
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
          //  GameMngr.instance.CheckStayLane();
        }

        if (Pedestrian)
        {
            if (!IsPedestCrossed) // Only track if player hasn't already been flagged
            {
                pedesttimer += Time.deltaTime; // Increment the timer
                if (pedesttimer >= 17f) // If the player waits for 17 seconds
                {
                    IsPedestCrossed = true; // Player followed the rule
                }
            }
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

       
   

        if (SpeedLimit)
        {
            if (Car.speed <= 40f)
            {
                GameMngr.instance.AppreciateCoinAdd("Speed Limit Rule Followed");
            }
            else
            {
                GameMngr.instance.AppreciateCoinAdd("You Should Follow Speed Limit Rule");

            }
        }


        if (Pedestrian) // Ensure we're dealing with the player or correct object
        {
            // Debugging
            Debug.LogError("PedestTimer: " + pedesttimer);
            Debug.LogError("IsPedestCrossed: " + IsPedestCrossed);

            // Check the result of the player's behavior
            if (IsPedestCrossed)
            {
                // The player stayed for 17 seconds, appreciate them
                GameMngr.instance.AppreciateCoinAdd("You Followed Pedestrian Rule");
            }
            else
            {
                // The player didn't stay for 17 seconds, discourage them
                GameMngr.instance.DiscourageCoinDeduct("You Should Follow Pedestrian Rule");
            }

            // Reset timer and flags when player exits the trigger area
            pedesttimer = 0f;
            IsPedestCrossed = false; // Reset the crossing state
        }


    }


    private void OnDisable()
    {
        GameMngr.instance.OnCarSet -= HandleCarSet;

    }
}

