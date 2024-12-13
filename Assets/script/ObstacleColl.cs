using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleColl : MonoBehaviour
{



    int collisionCount;
    public GameObject Emoji;
    public ParticleSystem Smoke;
    Rigidbody rb;
    RCC_CarControllerV3 Car;
    ParkingGm gm;

    bool hasCollided;

    public GameObject Taillights;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Car = GetComponent<RCC_CarControllerV3>();
        gm = ParkingGm.instance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug log to check if collision is happening

        // Ensure the object collided has the tag "Hurdle"
        if (collision.gameObject.CompareTag("Hurdle"))
        {
            Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                if (!hasCollided)
                {
                    obstacle.HitEffect();
                    StartCoroutine(PlayAngryEmoji());
                    gm.Collided();
                    CheckCollisions();
                    hasCollided = true;
                }
               
            }
            else
            {
                Debug.LogWarning("Obstacle component not found on the collided object.");
            }
        }
    }


    void CheckCollisions() 
    {
       
            collisionCount++;
            if (collisionCount >= 4)
            {
                CarWreckedPlay();
            }
        
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hurdle"))
        {
            hasCollided = false;
        }
    }

    void CarWreckedPlay() 
    {
        rb.isKinematic = true;
        Car.canControl = false;
        Smoke.Play();
    }
    IEnumerator PlayAngryEmoji() 
    {
        Emoji.SetActive(true);
        yield return new WaitForSeconds(1.40f);
        Emoji.SetActive(false);
    }
}


