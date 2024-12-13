using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UIAnimatorCore;
using System;

public class ParkingGm : MonoBehaviour
{


    [SerializeField] RCC_CarControllerV3 Car;
    [SerializeField] Camera Cam;
    [SerializeField] RCC_Camera rccCam;
    [SerializeField] CanvasGroup canvas;
    [SerializeField] RCC_UIController GasBtn;
    [SerializeField] RCC_UIController BrakeBtn;
    [SerializeField] GameObject celeb;
    [SerializeField] ParticleSystem CollectbleCash;
    [SerializeField] ParticleSystem CollectbleCoin;


    [Header("UI")]
    [SerializeField] GameObject Ignition;

    [Header("Bools")]
    bool IsTimerRunning;
    bool isBrakePressed;





    LD_Park lvldata;
    ParticleSystem[] lvlconfti;
    Rigidbody rbCar;
    MySoundManager soundManager;
    GameObject taillights;

    public static ParkingGm instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;

    }
    void Start()
    {
        Car.StartEngine();
        rccCam.cameraMode = RCC_Camera.CameraMode.TOP;

    }
   
    public void OnLevelStatsLoadedHandler(LD_Park lvlData) 
    {
        lvldata = lvlData;

        ObstacleColl Cardata = Car.gameObject.GetComponent<ObstacleColl>();
      
        if (Cardata.Taillights != null)
            taillights = Cardata.Taillights;

        rbCar = Car.gameObject.GetComponent<Rigidbody>();


        if (lvldata.Confetti.Length != 0) 
        {
            lvlconfti = new ParticleSystem[lvldata.Confetti.Length];
            Array.Copy(lvldata.Confetti, lvlconfti, lvldata.Confetti.Length);
        }
        


        StartCoroutine(CheckCarDestination(lvldata.SpawnPoint.position));

    }
    
    private IEnumerator CheckCarDestination(Vector3 destination)
    {
        rbCar.velocity = Vector3.zero;
        SetPositionBackward();
        rbCar.isKinematic = false;

        GasBtn.pressing = true;

        while (Vector3.Distance(rbCar.transform.position, destination) > 0.5f)
        {
            yield return null; // Wait until the next frame
        }

        GasBtn.pressing = false;
        BrakeBtn.pressing = true;
        yield return new WaitForSeconds(0.4f);
        BrakeBtn.pressing = false;
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(SetLevel());
    }
    void SetPositionBackward()
    {

        Vector3 currentPosition = lvldata.transform.position;

        currentPosition.z -= 15f;

        Car.transform.position = currentPosition;

        rbCar.position = currentPosition;

    }
    IEnumerator SetLevel() 
    {
        yield return new WaitForSeconds(1.5f);
        foreach(GameObject g in lvldata.OnObjets) 
        {
            g.SetActive(true);
        }
        yield return new WaitForSeconds(2f);
        rccCam.cameraMode = RCC_Camera.CameraMode.TPS;
        rbCar.isKinematic = false;
        Ignition.SetActive(true);
    }
   



    public void Collided() 
    {
        Cam.DOShakePosition(0.5f, 0.5f, 10, 90f);
    }
    
    public void CarFinalPark() 
    {
        GameObject car = Car.gameObject;
        Rigidbody rb = car.GetComponent<Rigidbody>();
        
        rb.isKinematic = true;
        canvas.alpha = 0f;

        foreach(ParticleSystem particle in lvlconfti) 
        {
            particle.Play();
        }

        Invoke(nameof(Celeb),0.5f);
      
        
    }
    void Celeb() 
    {
       
        RCC_CameraCarSelection celebCam = rccCam.gameObject.GetComponent<RCC_CameraCarSelection>();
        celebCam.enabled = true;
        celeb.SetActive(true);
    }

    public void EngineRun()
    {
        if (soundManager)
            soundManager.PlayEngineSound();
        
        Shakecam();

        Ignition.SetActive(false);
        IsTimerRunning = true;
    }
    void Shakecam() 
    {
        Cam.DOShakePosition(0.5f, 0.5f, 10, 90f).OnKill(() => OnShakeComplete());
    }
    void OnShakeComplete() 
    {
        canvas.alpha = 1f;
        canvas.gameObject.GetComponent<UIAnimator>().PlayAnimation(AnimSetupType.Intro);
    }



    private void Update()
    {
        if (taillights != null && HasBrakeStateChanged())
        {
            UpdateBrakeLightColor(BrakeBtn.pressing);
            isBrakePressed = BrakeBtn.pressing;
        }
    }

    private bool HasBrakeStateChanged()
    {
        return BrakeBtn.pressing != isBrakePressed;
    }

    private void UpdateBrakeLightColor(bool isPressed)
    {
        taillights.SetActive(isPressed);
    }

    public void CollectablePlay(bool isCash = false, bool isCoin = false)
    {
        if (isCash)
        {
            CollectbleCash.Play();
            if (soundManager)
                soundManager.PlayCollectSound();

        }

        if (isCoin)
        {
            CollectbleCoin.Play();
            if (soundManager)
                soundManager.PlayCollectCoin();

        }
    }
}
