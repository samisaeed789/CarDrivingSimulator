using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    Animation animationComponent;
    public void HitEffect() 
    {
        Debug.Log("Collision detected with: " + this.gameObject.name);

        Animator animator = GetComponent<Animator>();

        // Check if the Animation component exists
        if (animator != null)
        {
            
            animator.Play("ObsAnim");  // Replace with the name of your animation clip in the Animator Controller
        }
        else
        {
            Debug.LogError("Animator component not found on this GameObject.");
        }
    }
}
