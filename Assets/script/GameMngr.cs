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
    public RCC_UIController Brake;


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
    [SerializeField] Transform dancingchar;
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
    MeshRenderer brakelght;
    [SerializeField] GameObject[] greenred;
    Color color;
    bool isBrakePressed;
    [HideInInspector] public bool IsCorrectLane;
    [HideInInspector] public bool IsWrongLane;


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
        if (soundmgr)
            soundmgr.SetBGM(true);
    }





    #region loading/settinglvl

    public void OnLevelStatsLoadedHandler(LevelData levelStats)
    {
        //  string str = ValStorage.GetCar();
        // int ind = int.Parse(str);
        Car = PlayerCars[0].GetComponent<RCC_CarControllerV3>();  //PlayerCars[ind - 1].GetComponent<RCC_CarControllerV3>();
        car = Car.gameObject.GetComponent<CarData>();
       
        Rigidbody rb = Car.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.MovePosition(levelStats.SpawnPoint.position);
            rb.MoveRotation(levelStats.SpawnPoint.rotation);
        }

        Car.gameObject.SetActive(true);
        dancingchar.SetPositionAndRotation(levelStats.dancetrans.position, levelStats.dancetrans.rotation);

        Indilft = car.Indilft;
        IndiRght = car.Indirght;

        CinematicCam.target = Car.transform;
        brakelght = car.BrakeLight;

        if (levelStats.greenred != null)
        {
            greenred = new GameObject[levelStats.greenred.Length];
            Array.Copy(levelStats.greenred, greenred, levelStats.greenred.Length);
        }

        Debug.Log("car======" + car.gameObject.name);
        Debug.Log("spawnpoint======" + levelStats.SpawnPoint.gameObject.name);

      
    }

    #endregion



    #region PlaysSounds

    public void CollectablePlay(bool isCash = false, bool isCoin = false)
    {
        if (isCash)
        {
            CollectbleCash.Play();
            if (soundmgr)
                soundmgr.PlayCollectSound();

        }

        if (isCoin)
        {
            CollectbleCoin.Play();
            if (soundmgr)
                soundmgr.PlayCollectCoin();

        }
    }

    public void PlayStopMusic()
    {
        Debug.Log("MusicOff active state: " + MusicOff.activeSelf);

        // If the MusicOff button is active (meaning music is currently off)
        if (MusicOff.activeSelf)
        {
            Debug.Log("Turning music on...");
            MusicOff.SetActive(false);  // Hide the "Music Off" button
            if (soundmgr)
            {
                soundmgr.SetBGM(true);  // Start playing background music
                Debug.Log("Music is now ON");
            }
        }
        else
        {
            Debug.Log("Turning music off...");
            MusicOff.SetActive(true);  // Show the "Music Off" button
            if (soundmgr)
            {
                soundmgr.SetBGM(false);  // Stop playing background music
                Debug.Log("Music is now OFF");
            }
        }
    }

    public void PlayHorn()
    {
        if (soundmgr)
        {
            soundmgr.SetBGM(true);  // Start playing background music
            Debug.Log("Music is now ON");
        }
    }

    public void OnButtonPressed()
    {

        if (soundmgr)
        {
            soundmgr.PlayHorn();
        }

    }

    public void OnButtonReleased()
    {

        if (soundmgr)
        {
            soundmgr.StopHorn();
        }
    }
    #endregion

    #region Complete


    public void Celeb()
    {
        if (soundmgr)
            soundmgr.PlayCompleteSound(true);


        // Car.audioType = RCC_CarControllerV3.AudioType.Off;


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
        Transform dancechar = dancingchar.transform.GetChild(0);
        foreach (Transform child in dancechar)
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
        if (soundmgr)
            soundmgr.PlayCompleteSound(false);
        Complete.SetActive(true);
        SetCoinsinPanel();
        car.gameObject.SetActive(false);

    }


    void SetCoinsinPanel()
    {
        if (CoinsEarnedInLvl < 0)
            CoinsEarnedInLvl = 0;


        Timetxt.text = Mathf.FloorToInt(elapsedTime * 2).ToString();
        CoinsEarnedlvltxt.text = CoinsEarnedInLvl.ToString();

        StartCoroutine(CounterAnimation(CalculateTotalCoins()));
    }

    private IEnumerator CounterAnimation(int totalCoins)
    {
        yield return new WaitForSeconds(1f);
        int currentCoins = 0;
        if (soundmgr)
            soundmgr.PlaycoinSound();
        while (currentCoins < totalCoins)
        {
            currentCoins += Mathf.CeilToInt(2 * Time.deltaTime);
            currentCoins = Mathf.Min(currentCoins, totalCoins);
            TotalCompltxt.text = currentCoins.ToString();
            yield return null;
        }

        if (soundmgr)
            soundmgr.StopcoinSound();
    }

    private int CalculateTotalCoins()
    {
        int coinsFromTime = Mathf.FloorToInt(elapsedTime * 2);
        if (CoinsEarnedInLvl < 0)
            CoinsEarnedInLvl = 0;

        return CoinsEarnedInLvl + coinsFromTime;
    }
    #endregion


    #region startlvl

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


    #endregion

    #region UIBtnFunc

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


    public void IndiLft()
    {
        // Check if the right indicator is active, and deactivate it if necessary
        if (RightIndActv.activeSelf)
        {
            RightIndActv.SetActive(false);
            IndiRght.SetActive(false);
        }

        // Toggle the left indicator
        if (LeftIndActv.activeSelf)
        {
            LeftIndActv.SetActive(false);
            Indilft.SetActive(false);
            car.currentState = PlayerState.None;
            if (soundmgr)
                soundmgr.playindiSound(false);
        }
        else
        {
            LeftIndActv.SetActive(true);
            Indilft.SetActive(true);
            car.currentState = PlayerState.LeftIndicator;

            // Optionally play sound for left indicator here
            if (soundmgr)
                soundmgr.playindiSound(true);
        }
    }

    public void IndiRight()
    {
        // Check if the left indicator is active, and deactivate it if necessary
        if (LeftIndActv.activeSelf)
        {
            LeftIndActv.SetActive(false);
            Indilft.SetActive(false);
        }

        // Toggle the right indicator
        if (RightIndActv.activeSelf)
        {
            RightIndActv.SetActive(false);
            IndiRght.SetActive(false);
            car.currentState = PlayerState.None;

            if (soundmgr)
                soundmgr.playindiSound(false);

        }
        else
        {
            RightIndActv.SetActive(true);
            IndiRght.SetActive(true);
            car.currentState = PlayerState.RightIndicator;

            // Optionally play sound for right indicator here
            if (soundmgr)
                soundmgr.playindiSound(true);
        }
    }

    #endregion





    public void EnableGreen()
    {
        if (greenred[0] != null)
            greenred[0].SetActive(false);

        StartCoroutine(delayenablegreen());
    }

    IEnumerator delayenablegreen()
    {
        yield return new WaitForSeconds(0.3f);
        if (greenred[1] != null)
            greenred[1].SetActive(true);

        yield return new WaitForSeconds(0.3f);

        AppreciateCoinAdd("You Followed Traffic Signal Rule");
    }


    public void AppreciateCoinAdd(string s)
    {
        Appreciate.transform.GetChild(0).gameObject.GetComponent<Text>().text = s;// "You Followed Turn Rule";
        StartCoroutine(DelayedAppreciateCoinAdd(1f));
    }


    private IEnumerator DelayedAppreciateCoinAdd(float delay)
    {

        yield return new WaitForSeconds(delay); // Wait for the specified delay

        Appreciate.SetActive(true);
        AddCoins(15);
        CoinsEarnedInLvl = CoinsEarnedInLvl + 15;

        if (soundmgr)
            soundmgr.ExcellentSound();
    }





    public void DiscourageCoinDeduct(string s)
    {
        Discourage.transform.GetChild(0).gameObject.GetComponent<Text>().text = s;

        StartCoroutine(DelayedDiscCoinDed(1f));
    }

    private IEnumerator DelayedDiscCoinDed(float delay)
    {


        yield return new WaitForSeconds(delay); // Wait for the specified delay

        Discourage.SetActive(true);
        AddCoins(-15);

        if (CoinsEarnedInLvl > 0)
            CoinsEarnedInLvl = CoinsEarnedInLvl - 15;

        if (soundmgr)
            soundmgr.DiscourageSound();
    }

    void AddCoins(int val)
    {
        ValStorage.SetCoins(ValStorage.GetCoins() + val);
    }



    void Update()
    {
        if (brakelght && HasBrakeStateChanged())
        {
            UpdateBrakeLightColor(Brake.pressing);
            isBrakePressed = Brake.pressing;
            Debug.Log("running");
        }

        if (IsTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerText();
        }



        //if (IsCorrectLane)
        //{
        //    // Increase the time if in the correct lane
        //    timeInLane += Time.deltaTime;

        //    // If player stays for 7 seconds in the correct lane
        //    if (timeInLane >= 7f)
        //    {
        //        AppreciateCoinAdd("You Followed Turn Rule");
        //        timeInLane = 0f;
        //    }
        //}
        //else
        //{
        //    // If player is in the wrong lane, reset timer and show warning
        //    timeInLane = 0f;
        //    DiscourageCoinDeduct("You Should Followed Turn Rule");
        //}

    }


    float timeInLane;
    public void CheckStayLane() 
    {

        if (Car.GetComponent<RCC_CarControllerV3>().speed >= 10f)  // Check if car is moving
            {
                timeInLane += Time.deltaTime;

                // If player stays in the correct lane for 7 seconds, reward
                if (timeInLane >= 7f)
                {
                    AppreciateCoinAdd("You Followed Turn Rule");
                    timeInLane = 0f; // Reset timer after reward
                }
            }
       
    }
    public void CheckOutLane() 
    {

        if (Car.GetComponent<RCC_CarControllerV3>().speed >= 10f)  // Car is moving
        {
            // Player is out of the correct lane
            timeInLane = 0f;  // Reset the timer since the player left the lane
            DiscourageCoinDeduct("You Should Followed Turn Rule");
        }

    }





    private bool HasBrakeStateChanged()
    {
        return Brake.pressing != isBrakePressed;
    }

    private void UpdateBrakeLightColor(bool isPressed)
    {
        Color color = isPressed ? Color.red : Color.grey; // Use Unity's predefined colors for clarity
        brakelght.material.color = color;
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



    public void BrakeLightts(bool isplay)
    {


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


        foreach (Image UI in UIgp)
        {
            // Image buttonImage = UI.GetComponent<Image>();
            Color buttonColor = UI.color;
            buttonColor.a = alpha;  // Set alpha based on the calculation
            UI.color = buttonColor;
        }

    }
    #endregion

}


 