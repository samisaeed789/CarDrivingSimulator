using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolicePark : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameMngr.instance.ControllerBtns.alpha = 0f;
            StartCoroutine(MoveCarSmoothly(GameMngr.instance.Car.gameObject));

        }
    }

    private IEnumerator MoveCarSmoothly(GameObject Car)
    {


        // Set the car to kinematic so physics won't interfere with the movement
        Car.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        Vector3 startPos = Car.transform.position;
        Quaternion startRot = Car.transform.rotation;  // Start rotation
        Vector3 targetPos = this.transform.GetChild(0).transform.position;  // Target position
        Quaternion targetRot = this.transform.GetChild(0).transform.rotation;  // Target rotation (could be the same or different)
        float elapsedTime = 0f;

        // Continue moving and rotating until the duration has passed
        while (elapsedTime < 2f)
        {
            // Lerp the car's position
            Car.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / 2);

            // Lerp the car's rotation
            Car.transform.rotation = Quaternion.Lerp(startRot, targetRot, elapsedTime / 2);

            elapsedTime += Time.deltaTime; // Increment the elapsed time
            yield return null; // Wait until the next frame
        }

        // Ensure the car reaches the exact target position and rotation when done

        Car.transform.position = targetPos;
        Car.transform.rotation = targetRot;





        yield return new WaitForSeconds(0.5f);

        GameMngr.instance.ShowOtherCam();
        this.gameObject.SetActive(false);
    }
}
