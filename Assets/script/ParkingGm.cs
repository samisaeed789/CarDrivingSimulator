using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UIAnimatorCore;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ParkingGm : MonoBehaviour
{


    [Header("Panels")]
    [SerializeField] GameObject emojiPanel;
    [SerializeField] GameObject failPanel;
    [SerializeField] GameObject completePanel;
    [SerializeField] GameObject Loading;



    [Header("Others")]
    [SerializeField] RCC_CarControllerV3 Car;
    [SerializeField] Camera Cam;
    [SerializeField] RCC_Camera rccCam;
    [SerializeField] RCC_UIController GasBtn;
    [SerializeField] RCC_UIController BrakeBtn;
    [SerializeField] GameObject celeb;
    [SerializeField] ParticleSystem CollectbleCash;
    [SerializeField] ParticleSystem CollectbleCoin;




    [Header("UI")]
    [SerializeField] GameObject Ignition;
    [SerializeField] GameObject LoadBar;
    [SerializeField] Image loadingBar;
    [SerializeField] Text percentageText;
    [SerializeField] Animator sphere;
    [SerializeField] CanvasGroup canvas;




    [Header("Bools")]
    bool IsTimerRunning;
    bool isBrakePressed;
    [SerializeField]bool Test;


    [Header("Levels")]
    [SerializeField]GameObject[] Levels;
    [SerializeField]GameObject[] Cars;


    [SerializeField] int levelnumber;
    [SerializeField] int Carnumber;


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
    IEnumerator Start()
    {
       
        yield return new WaitForSeconds(0.2f);
      
        int currentlvl;
        if (Test)
        {
            currentlvl = levelnumber;
            Cars[Carnumber].SetActive(true);
        }
        else
        {
            currentlvl = ValStorage.selLevelParking;
            Car = Cars[Carnumber].gameObject.GetComponent<RCC_CarControllerV3>();
            Cars[Carnumber].SetActive(true);

        }
        Levels[currentlvl - 1].gameObject.SetActive(true);

    }

    public void OnLevelStatsLoadedHandler(LD_Park lvlData) 
    {
        lvldata = lvlData;
        ObstacleColl Cardata = Car.gameObject.GetComponent<ObstacleColl>();
        if (Cardata.Taillights != null)
            taillights = Cardata.Taillights;
        rbCar = Car.gameObject.GetComponent<Rigidbody>();
        Car.StartEngine();
        rccCam.cameraMode = RCC_Camera.CameraMode.TOP;

        if (lvldata.Confetti.Length != 0) 
        {
            lvlconfti = new ParticleSystem[lvldata.Confetti.Length];
            Array.Copy(lvldata.Confetti, lvlconfti, lvldata.Confetti.Length);
        }


        string name = rbCar.gameObject.name;
        Transform SP;
        if (name == "Jeep")
        {
            SP = lvldata.SpawnPointJeep;
        }
        else
        {
            SP = lvldata.SpawnPoint;

        }
        Debug.Log("asdasdasdas"+SP.gameObject.name);
        StartCoroutine(CheckCarDestination(SP.localPosition));

    }
    
    private IEnumerator CheckCarDestination(Vector3 destination)
    {
        rbCar.velocity = Vector3.zero;
        
        SetPositionBackward(destination);
        rbCar.isKinematic = false;
        yield return new WaitForSeconds(0.5f);
        Loading.SetActive(false);
        GasBtn.pressing = true;

        while (Vector3.Distance(rbCar.transform.position, destination) > 1f)
        {
            yield return null; // Wait until the next frame
        }

        GasBtn.pressing = false;
        BrakeBtn.pressing = true;
        yield return new WaitForSeconds(1.4f);
        StartCoroutine(SetLevel());
    }
    void SetPositionBackward(Vector3 SpawnPoint)
    {

        //Vector3 currentPosition = SpawnPoint.position;// lvldata.SpawnPoint.transform.position;

        //currentPosition.z -= 15f;

        //Car.transform.position = currentPosition;

        //rbCar.position = currentPosition;

        Vector3 currentPosition = SpawnPoint;

        currentPosition.z -= 15f;

      //  Car.transform.position = currentPosition;

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
        BrakeBtn.pressing = false;
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
        StartDance();
        Invoke(nameof(Celeb),0.5f);
    }
    void Celeb() 
    {
     
        RCC_CameraCarSelection celebCam = rccCam.gameObject.GetComponent<RCC_CameraCarSelection>();
        celebCam.enabled = true;
        celeb.SetActive(true);

        StartCoroutine(CompletePanel());
    } 


    public void StartDance()
    {
        Transform dancechar = lvldata.DanceChar?.transform;
        foreach (Transform child in dancechar)
        {
            Animator animator = child.GetComponent<Animator>();

            if (animator != null)
            {
                animator.SetBool("Dance", true);
            }
        }
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

    public void FailLevel() 
    {
        canvas.alpha = 0f;
        StartCoroutine(FailPanel());
    }

    IEnumerator FailPanel() 
    {
        yield return new WaitForSeconds(2f);
        emojiPanel.SetActive(true);
        yield return new WaitForSeconds(4f);
        failPanel.SetActive(true);
    }
    IEnumerator CompletePanel() 
    {
        yield return new WaitForSeconds(7f);
        completePanel.SetActive(true);
    }

    public void Restart()
    {
        Loading.SetActive(true);
        LoadBar.SetActive(true);
        StartCoroutine(LoadAsyncScene("ParkingMode"));
    }

    public void Home()
    {
        Time.timeScale = 1f;
        Loading.SetActive(true);
        LoadBar.SetActive(true);
        StartCoroutine(LoadAsyncScene("MM"));
    }

    IEnumerator LoadAsyncScene(string sceneName)
    {
        if (AdsManager.instance)
            AdsManager.instance.showAdMobRectangleBannerBottomLeft();


        float timer = 0f;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (timer < 5f)
        {
            if (timer < 5f)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / 5f);  // Progress from 0 to 1 based on timer
                loadingBar.fillAmount = progress;
                percentageText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
            else
            {
                // Once the timer reaches 5 seconds, start loading the scene
                // Ensure the progress bar stays at 100% before activation
                loadingBar.fillAmount = 1f;
                percentageText.text = "100%";

                // Allow the scene to activate
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
        sphere.enabled = false;

        if (AdsManager.instance)
            AdsManager.instance.hideAdmobBottomLeftBanner();
        yield return new WaitForSeconds(0.1f);
        asyncLoad.allowSceneActivation = true;
    }
}
