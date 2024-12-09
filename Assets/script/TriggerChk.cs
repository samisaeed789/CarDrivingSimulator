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

    GameMngr gm;


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

    private void Start()
    {
        gm = GameMngr.instance;
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
                    gm.AppreciateCoinAdd("You Followed Indicator Rule");
                }
                else
                {
                    gm.DiscourageCoinDeduct("You Should Follow Indicator Rule");
                }
            }

            if (TrafficSignal)
            {
                isInTrigger = true;  // Player entered the trigger
                timer = 0f;  // Reset the timer
            }

            if (StayInLane)
            {
                gm.LaneTimer = 0f;
                gm.DiscourageCoinDeduct("You Should Stay in your Lane");
            }
            
            if (Pedestrian)
            {
                gm.PlayWalk();
            }

            if (PoliceChk) 
            {
                if (!gm.IsStoppedAtPolice) 
                {
                    gm.DiscourageCoinDeduct("You Should Stop At CheckPoint");
                }
            }  
            
            
            if (LeftIndi) 
            {
                if (plyrcar.currentState == PlayerState.LeftIndicator)
                {
                    gm.AppreciateCoinAdd("You Followed Indicator Rule");
                }
                else
                {
                    gm.DiscourageCoinDeduct("You Should Follow Indicator Rule");
                }
            }

            if (RightIndi)
            {
                if (plyrcar.currentState == PlayerState.RightIndicator)
                {
                    gm.AppreciateCoinAdd("You Followed Indicator Rule");
                }
                else
                {
                    gm.DiscourageCoinDeduct("You Should Follow Indicator Rule");
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
                gm.EnableGreen();  // Call the appreciate method
                isInTrigger = false;  // Prevent further calls to appreciate
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
                gm.EnableRed();

                if (gm.IsGreenEnabled) 
                {
                    gm.AppreciateCoinAdd("You Followed Traffic Signal Rule");
                }
                else if(!gm.IsGreenEnabled)
                {
                    gm.DiscourageCoinDeduct("You Should Follow Signal Rule");
                }
            }
        }

        if (SpeedLimit)
        {
            if (Car.speed <= 40f)
            {
                gm.AppreciateCoinAdd("Speed Limit Rule Followed");
            }
            else
            {
                gm.AppreciateCoinAdd("You Should Follow Speed Limit Rule");

            }
        }

        if (Pedestrian) 
        {
            bool HasCrossed = GameMngr.instance.HasPedestriansCrossed;
            if (HasCrossed) 
            {
                gm.AppreciateCoinAdd("You Followed Pedestrian Rule");

            }
            else 
            {
                gm.DiscourageCoinDeduct("You Should Follow Pedestrian Rule");
            }
        }
    }


    private void OnDisable()
    {
        GameMngr.instance.OnCarSet -= HandleCarSet;

    }
}

