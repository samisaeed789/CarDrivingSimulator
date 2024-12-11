using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempGm : MonoBehaviour
{
    [SerializeField] RCC_CarControllerV3 Car;
    [SerializeField] Camera Cam;
    [SerializeField] Animator animator;
    void Start()
    {
        Car.StartEngine();
    }


    public void Collided() 
    {
        Cam.DOShakePosition(0.5f, 0.5f, 10, 90f);
    }
    
    public void Temp() 
    {
        animator.Play("ObsAnim");
    }



}
