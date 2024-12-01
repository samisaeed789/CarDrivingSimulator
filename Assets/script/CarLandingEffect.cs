using UnityEngine;

public class CarLandingEffect : MonoBehaviour
{
    public ParticleSystem landingEffect; // Assign your particle system in the inspector
    public LayerMask groundLayer;        // Define what layers count as ground
    private bool isAirborne = false;     // Tracks if the car is in the air
    public float DropForce;

   


    Rigidbody rb;
    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }
  
    private void OnEnable()
    {
    
        transform.position += new Vector3(0, 0.5f, 0);
        rb.isKinematic = false;
        rb.useGravity = true;

        // Apply an initial downward force to simulate the drop
        rb.velocity = Vector3.down * DropForce;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with the ground
        if (IsGround(collision))
        {                // Play the landing particle effect
                PlayLandingEffect();
        }
    }

  

    private bool IsGround(Collision collision)
    {
        // Check if the collided object is on the ground layer
        return ((1 << collision.gameObject.layer) & groundLayer) != 0;
    }

    private void PlayLandingEffect()
    {
        if (landingEffect != null)
        {
            landingEffect.Play();
        }
    }
}
