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
    public GameObject MusicOff;


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
            if(soundmgr)
                soundmgr.PlayCollectSound();

        }

        if (isCoin) 
        {
            CollectbleCoin.Play();
             if(soundmgr)
                soundmgr.PlayCollectCoin();

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
        SetCoinsinPanel();
    }


    void SetCoinsinPanel()
    {
        if(CoinsEarnedInLvl<0)
            CoinsEarnedInLvl=0;

        
        Timetxt.text = Mathf.FloorToInt(elapsedTime*2).ToString();
        CoinsEarnedlvltxt.text = CoinsEarnedInLvl.ToString();

        StartCoroutine(CounterAnimation(CalculateTotalCoins()));
    }

    private IEnumerator CounterAnimation(int totalCoins)
    {
        yield return new WaitForSeconds(1f);
        int currentCoins = 0;
        if(soundmgr)
            soundmgr.PlaycoinSound();
        while (currentCoins < totalCoins)
        {
            currentCoins += Mathf.CeilToInt(2 * Time.deltaTime);
            currentCoins = Mathf.Min(currentCoins, totalCoins);
            TotalCompltxt.text = currentCoins.ToString();
            yield return null;
        }

         if(soundmgr)
            soundmgr.StopcoinSound();
    }

    private int CalculateTotalCoins()
    {
        int coinsFromTime = Mathf.FloorToInt(elapsedTime*2); 
        if(CoinsEarnedInLvl<0)
            CoinsEarnedInLvl=0;

        return  CoinsEarnedInLvl+ coinsFromTime;
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
   

   public void PlayStopMusic()
   {
        if(MusicOff)
        {
            MusicOff.SetActive(false);
            if(soundmgr)
                soundmgr.SetBGM(true);
        }
        else
        {
            MusicOff.SetActive(true);
            if(soundmgr)
                soundmgr.SetBGM(false);
        }
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
        if(soundmgr)
            soundmgr.SetBGM(true);
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
