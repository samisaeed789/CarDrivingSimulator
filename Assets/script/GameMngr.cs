using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIAnimatorCore;
using UnityEngine;
using UnityEngine.UI;
using WaypointsFree;



public enum PlayerState
{
    None,           
    LeftIndicator,  
    RightIndicator,  
    Speeding      
}

public class GameMngr : MonoBehaviour
{


    public static GameMngr instance;
    MySoundManager soundmgr;



    [Header("Panels")]
    public GameObject Complete;
    public GameObject Fail;
    public GameObject Pause;
    public GameObject Loading;
    public GameObject BlckPnl;

    [Header("GP UI")]
    public Image[] UIgp;
    public GameObject gearup;
    public GameObject geardown;
    public GameObject LeftIndActv;
    public GameObject RightIndActv;
    public CanvasGroup ControllerBtns;
    public GameObject Ignition;
    public GameObject Appreciate;
    public GameObject Discourage;

    [Header("Camera")]
    public Camera Cam;


    [Header("Player")]
    public RCC_CarControllerV3 Car;



    [Header("OtherScripts")]
    [SerializeField] TSTrafficSpawner trafficSpawner;


    [Header("Data")]
    public GameObject[] PlayerCars;
    public WaypointsTraveler[] TrafficCars;
    GameObject Indilft;
    GameObject IndiRght;
    [HideInInspector] Transform dancingchar;
    [SerializeField] GameObject Conftti;
    [SerializeField] ParticleSystem CollectbleCash;
    [SerializeField] ParticleSystem CollectbleCoin;


    [Header("Canvas")]
    [SerializeField] RCC_Demo Controls;



    [Header("Texts")]
    public Text timerText;
    public Text TotalCompltxt;
    public Text CoinsEarnedlvltxt;
    public Text Timetxt;






    // Variables to store the elapsed time



    float elapsedTime = 0f;
    CarData car;
    RCC_CameraCarSelection CinematicCam;
    GameObject FinalPoint;
    bool IsTimerRunning = false;
    static int CoinsEarnedInLvl;

    IEnumerator Start()
    {
        if (instance == null)
            instance = this;

        UpdateTimerText();
        CinematicCam = Cam.transform.parent.parent.gameObject.GetComponent<RCC_CameraCarSelection>();
        soundmgr = MySoundManager.instance;
        SetButtonTransparency(ValStorage.GetTransparency());
        Controls.SetMobileController(ValStorage.GetControls());
        ControllerBtns.alpha = 0f;
        yield return new WaitForSeconds(2); // fixed delay
        Loading.SetActive(false);
    }





    #region loading/settinglvl

    public void OnLevelStatsLoadedHandler(LevelData levelStats)
    {
      //  string str = ValStorage.GetCar();
       // int ind = int.Parse(str);
        Car = PlayerCars[0].GetComponent<RCC_CarControllerV3>();  //PlayerCars[ind - 1].GetComponent<RCC_CarControllerV3>();
        car = Car.gameObject.GetComponent<CarData>();
        Indilft = car.Indilft;
        IndiRght = car.Indirght;
        dancingchar = levelStats.Dancing;
        CinematicCam.target = Car.transform;
        FinalPoint = levelStats.FinalP;
    }

    #endregion



    #region PlaysSounds

    public void CollectablePlay(bool isCash = false, bool isCoin = false)
    {
        if (isCash) 
        {
            CollectbleCash.Play();

        }

        if (isCoin) 
        {
            CollectbleCoin.Play();

        }
    }


    #endregion

    #region Complete


    public void Celeb()
    {
        trafficSpawner.DisableAllCars();
        StartDance();
        ControllerBtns.alpha = 0f;
        CinematicCam.enabled = true;
        Conftti.SetActive(true);
        BlckPnl.SetActive(true);
        IsTimerRunning = false;
        StartCoroutine(CompletePanel());
    }

    public void StartDance()
    {
        foreach (Transform child in dancingchar)
        {
            Animator animator = child.GetComponent<Animator>();

            if (animator != null)
            {
                animator.SetBool("Dance", true);
            }
        }
    }


    private IEnumerator CompletePanel()
    {

        yield return new WaitForSeconds(8f);
        Complete.SetActive(true);
        CalculateTotalCoins();
    }

    public void CalculateTotalCoins()
    {

        Timetxt.text = Mathf.FloorToInt(elapsedTime).ToString();
        CoinsEarnedlvltxt.text = CoinsEarnedInLvl.ToString();



        float timeMultiplier;
       
        if (elapsedTime <= 300 / 2) // If completed in half the time or less, give a bonus.
        {
            timeMultiplier = 1.5f; // Example multiplier for fast completion.
        }
        else if (elapsedTime <= 300) // If completed within time limit, normal multiplier.
        {
            timeMultiplier = 1.2f; // Example multiplier for normal time.
        }
        else // If exceeded time limit, no bonus multiplier.
        {
            timeMultiplier = 1.0f; // No extra reward for going over time.
        }

        // Calculate total coins including the time multiplier.
        int finalCoins = Mathf.FloorToInt(CoinsEarnedInLvl * timeMultiplier);

        // Display the total coins and time taken on the success panel.
        TotalCompltxt.text = "Total Coins: " + finalCoins;
        StartCoroutine(AnimateCoinCounter(finalCoins));
    }

    IEnumerator AnimateCoinCounter(int finalCoins)
    {

        yield return new WaitForSeconds(0.6f);
        int currentCoins = 0; // Start from 0 coins
        float timetaken = 0f; // To track the time for the animation

        while (elapsedTime < 1f) // Animate for 1 second (you can adjust this duration)
        {
            if (soundmgr)
                soundmgr.PlaycoinSound(1f);
            // Smoothly increment the current coin count
            currentCoins = Mathf.FloorToInt(Mathf.Lerp(0, finalCoins, elapsedTime));

            // Update the UI text with the current coin count
            TotalCompltxt.text = currentCoins.ToString();


            // Increment elapsed time based on speed
            elapsedTime += Time.deltaTime * 2;

            yield return null; // Wait until the next frame
        }

        // Ensure that the final count is set correctly at the end
        TotalCompltxt.text = finalCoins.ToString();
    }


    #endregion





    public void ChangeControl()
    {
        int currentind = ValStorage.GetControls();
        if (soundmgr)
            soundmgr.PlayButtonClickSound(1f);
        // Increment the index, loop back if needed
        currentind = (currentind + 1) % 3;
        Controls.SetMobileController(currentind);
        ValStorage.SetControls(currentind);

    }
   


    public void Enablegearactv(string s)
    {
        if (s == "drive")
        {
            Gearactive(IsDrive: true);
        }
        if (s == "reverse")
        {
            Gearactive(IsReverse: true);
        }
    }

    public void Gearactive(bool IsDrive = false, bool IsReverse = false)
    {
        if (gearup)
        {
            gearup.SetActive(IsDrive);
        }

        if (geardown)
        {
            geardown.SetActive(IsReverse);
        }
    }
    public void Indiactive(bool IsLeft = false, bool IsRight = false)
    {
        if (LeftIndActv)
        {
            LeftIndActv.SetActive(IsLeft);
        }

        if (RightIndActv)
        {
            RightIndActv.SetActive(IsRight);
        }
    }


    public void EngineRun()
    {
        if (soundmgr)
            soundmgr.PlayEngineSound();

        ShakeCamera();

        Ignition.SetActive(false);
        Car.StartEngine();
        IsTimerRunning = true;
    }
    public void ShakeCamera()
    {
        if (Cam != null)
        {
            Cam.DOShakePosition(0.5f, 0.5f, 10, 90f).OnKill(() => OnShakeComplete());
        }

    }
    void OnShakeComplete()
    {
        OnEnableUI();
    }

    private void OnEnableUI()
    {
        ControllerBtns.alpha = 1;
        ControllerBtns.gameObject.GetComponent<UIAnimator>().PlayAnimation(AnimSetupType.Intro);

    }


    public void IndiLft()
    {
        if (LeftIndActv.activeSelf) 
        {
            LeftIndActv.SetActive(false);
            Indilft.SetActive(false);
            car.currentState = PlayerState.None;
        }
        else
        {
            LeftIndActv.SetActive(true);
            Indilft.SetActive(true);
            car.currentState = PlayerState.LeftIndicator;

        }
    }
    
    public void IndiRight()
    {
        if (RightIndActv.activeSelf) 
        {
            RightIndActv.SetActive(false);
            IndiRght.SetActive(false);
            car.currentState = PlayerState.None;
        }
        else
        {
            RightIndActv.SetActive(true);
            IndiRght.SetActive(true);
            car.currentState = PlayerState.RightIndicator;

        }
    }

    public void AppreciateCoinAdd() 
    {
        Appreciate.SetActive(true);
        AddCoins(15);
        CoinsEarnedInLvl= CoinsEarnedInLvl + 15;

    }
    
    public void DiscourageCoinDeduct() 
    {
        Discourage.SetActive(true);
        AddCoins(-15);

        if(CoinsEarnedInLvl>0)
            CoinsEarnedInLvl = CoinsEarnedInLvl - 15;

    }

    void AddCoins(int val) 
    {
        ValStorage.SetCoins(ValStorage.GetCoins() + val);
    }





    void Update()
    {

        if (IsTimerRunning) 
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerText();

        }
    }

    void UpdateTimerText()
    {
        // Calculate the minutes and seconds
        int minutes = Mathf.FloorToInt(elapsedTime / 60); // Divide elapsed time by 60 to get minutes
        int seconds = Mathf.FloorToInt(elapsedTime % 60); // Get the remainder for seconds

        // Format the time as MM:SS
        string timeFormatted = string.Format("{0:D2}:{1:D2}", minutes, seconds);

        // Update the UI Text
        timerText.text = timeFormatted;
    }







    #region trnsprncy
    public void IncreaseTransparency()
    {
        int trans = ValStorage.GetTransparency();
        if (trans < 5)
        {
            trans++;
            ValStorage.SetTransparency(trans);
            SetButtonTransparency(trans);

        }

    }

    // Decrease the transparency value (if not at minimum)
    public void DecreaseTransparency()
    {
        int trans = ValStorage.GetTransparency();
        if (trans > 1)
        {
            trans--;
            ValStorage.SetTransparency(trans);
            SetButtonTransparency(trans);
        }
    }


    public void SetButtonTransparency(int transval)
    {
        // Clamp the setting value between 1 and 5 to ensure it stays in the valid range
        transval = Mathf.Clamp(transval, 1, 5);

        // Calculate the alpha value: 1 -> 0.1 (slightly visible), 5 -> 1 (fully opaque)
        float alpha = Mathf.Lerp(0.2f, 1f, (transval - 1) / 4f);


        foreach(Image UI in UIgp) 
        {
           // Image buttonImage = UI.GetComponent<Image>();
            Color buttonColor = UI.color;
            buttonColor.a = alpha;  // Set alpha based on the calculation
            UI.color = buttonColor;
        }
      
    }
    #endregion
}
