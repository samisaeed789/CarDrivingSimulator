using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Partcleplay : MonoBehaviour
{
    public ParticleSystem Nova;


    public void PlayParticle() 
    {
        Nova.Play();
    }
}
