using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalPark : MonoBehaviour
{
    // Start is called before the first frame update
    Transform Car;
    public Transform targetpoint;
    public float lerpDuration = 2.0f;
    IEnumerator Start()
    {
        targetpoint= transform.GetChild(0);
        yield return new WaitForSeconds(1f);
        Car = GameMngr.instance.Car.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            StartCoroutine(MoveCarSmoothly());
            GameMngr.instance.Celeb();


        }
    }

    private IEnumerator MoveCarSmoothly()
    {
        // Set the car to kinematic so physics won't interfere with the movement
        Car.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        Vector3 startPos = Car.transform.position;
        Quaternion startRot = Car.transform.rotation;  // Start rotation
        Vector3 targetPos = targetpoint.position;  // Target position
        Quaternion targetRot = targetpoint.rotation;  // Target rotation (could be the same or different)
        float elapsedTime = 0f;

        // Continue moving and rotating until the duration has passed
        while (elapsedTime < lerpDuration)
        {
            // Lerp the car's position
            Car.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / lerpDuration);

            // Lerp the car's rotation
            Car.transform.rotation = Quaternion.Lerp(startRot, targetRot, elapsedTime / lerpDuration);

            elapsedTime += Time.deltaTime; // Increment the elapsed time
            yield return null; // Wait until the next frame
        }

        // Ensure the car reaches the exact target position and rotation when done
        
        Car.transform.position = targetPos;
        Car.transform.rotation = targetRot;
       
        

      

        yield return new WaitForSeconds(0.5f);
        this.transform.GetChild(1).gameObject.SetActive(false);
    }
}
