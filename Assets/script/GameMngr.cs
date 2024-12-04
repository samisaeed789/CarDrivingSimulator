using DG.Tweening;
using System;
using System.Collections;
using UIAnimatorCore;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WaypointsFree;
using UnityEngine.Audio;



public enum PlayerState
{
    None,           
    LeftIndicator,  
    RightIndicator,  
    Speeding      
}

public class GameMngr : MonoBehaviour
{

    [System.Serializable]
    public class LevlsCs
    {
        public GameObject Cs;
        public GameObject Levls;
        public float CsTime;
        public String Appreciatetxt;
        public String CSObjtxt;
    }


    public static GameMngr instance;
    MySoundManager soundmgr;



    [Header("Panels")]
    public GameObject Complete;
    public GameObject Fail;
    public GameObject PausePnl;
    public GameObject Loading;
    public GameObject CSBlckPnl;
    public GameObject BlckPnl;

    [Header("GP UI")]
    public Image[] UIgp;
    public GameObject gearup;
    public GameObject LoadBar;
    public GameObject geardown;
    public GameObject LeftIndActv;
    public GameObject RightIndActv;
    public CanvasGroup ControllerBtns;
    public GameObject Ignition;
    public GameObject Appreciate;
    public GameObject CSAppreciate;
    public GameObject Discourage;
    public GameObject MusicOff;
    public RCC_UIController Brake;
    public GameObject IgnitBtn;
    public Image loadingBar;
    public Animator sphere;


    [Header("Levels")]
    [SerializeField]
    public LevlsCs[] lvlcs;


    [Header("Camera")]
    public Camera Cam;
    public Camera CsCam;


    [Header("Player")]
    public RCC_CarControllerV3 Car;
    



    [Header("OtherScripts")]
    [SerializeField] TSTrafficSpawner trafficSpawner;
    [SerializeField] TypingEffect CSObjtxt;


    [Header("Data")]
    public GameObject[] PlayerCars;
    public WaypointsTraveler[] TrafficCars;
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
    public Text percentageText;







    // Variables to store the elapsed time



    float elapsedTime = 0f;
    CarData car;
    RCC_CameraCarSelection CinematicCam;
    GameObject FinalPoint;
    bool IsTimerRunning = false;
    static int CoinsEarnedInLvl;
    GameObject[] brakelght;
    [SerializeField] GameObject[] greenred;
    Color color;
    bool isBrakePressed;
    [HideInInspector] public bool IsCorrectLane;
    [HideInInspector] public bool IsWrongLane;
    [HideInInspector]public bool IsStoppedAtPolice;
    [HideInInspector]public bool IsStayinginLane;
    [HideInInspector] public float LaneTimer;
    [HideInInspector] public bool HasPedestriansCrossed;
    private AsyncOperation async;





    public delegate void CarSetEventHandler(RCC_CarControllerV3 car);
    public event CarSetEventHandler OnCarSet;
    
   



    private void Awake()
    {
        if (instance == null)
            instance = this;

     

    }

    IEnumerator Start()
    {
        RCC_Settings.Instance.dontUseAnyParticleEffects = true;
        trafficSpawner.gameObject.SetActive(false);
        UpdateTimerText();
        CinematicCam = Cam.transform.parent.parent.gameObject.GetComponent<RCC_CameraCarSelection>();
        soundmgr = MySoundManager.instance;
        SetButtonTransparency(ValStorage.GetTransparency());
        Controls.SetMobileController(ValStorage.GetControls());
        yield return new WaitForSeconds(2); // fixed delay
        Loading.SetActive(false);
        
        if (soundmgr)
            soundmgr.SetBGM(true);

        StartCoroutine(PlayCs());
    }
    [SerializeField]int levelnumber;
  
    
    IEnumerator PlayCs() 
    {
        int currentlvl = ValStorage.selLevel; 
        float CSLength = lvlcs[currentlvl - 1].CsTime;
        lvlcs[currentlvl-1].Cs.gameObject.SetActive(true);
        CSAppreciate.transform.GetChild(0).gameObject.GetComponent<Text>().text = lvlcs[currentlvl - 1].Appreciatetxt.ToString();
        CSObjtxt.fullText = lvlcs[currentlvl - 1].CSObjtxt;
        CsCam.gameObject.SetActive(true);
        yield return new WaitForSeconds(CSLength);
        lvlcs[currentlvl-1].Cs.gameObject.SetActive(false);
        Cam.transform.parent.parent.gameObject.SetActive(true);
        CsCam.gameObject.SetActive(false);
        IgnitBtn.SetActive(true);
        lvlcs[currentlvl-1].Levls.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        CSBlckPnl.SetActive(false);
        trafficSpawner.gameObject.SetActive(true);
    }


    LevelData lvlstats;
    #region loading/settinglvl

    GameObject SetCar() 
    {
        string carIdString = ValStorage.GetCar();

        // Parse CarId to an integer
        if (int.TryParse(carIdString, out int carId))
        {
            // Adjust carId to be 0-based index (if necessary)
            carId -= 1;

            // Ensure the carId is within the valid range of PlayerCars array
            if (carId >= 0 && carId < PlayerCars.Length)
            {
                GameObject CarObj = PlayerCars[carId]; 

                // Assign the components
                Car = CarObj.GetComponent<RCC_CarControllerV3>();
                car = CarObj.GetComponent<CarData>();

                return CarObj;
            }
            else
            {
                return null;

                Debug.LogError("CarId is out of bounds of the PlayerCars array.");
            }


        }
        else
        {
            Debug.LogError("Failed to parse CarId as an integer.");
            return null;

        }
     

    }

    public void OnLevelStatsLoadedHandler(LevelData levelStats)
    {

        lvlstats = levelStats;
        GameObject CarObj= SetCar();
        CarObj.SetActive(true);
        
        OnCarSet?.Invoke(CarObj.GetComponent<RCC_CarControllerV3>());  // Invoke the event when car is set

        Rigidbody rb = CarObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.MovePosition(levelStats.SpawnPoint.position);
            rb.MoveRotation(levelStats.SpawnPoint.rotation);
        }


        dancingchar.SetPositionAndRotation(levelStats.dancetrans.position, levelStats.dancetrans.rotation);


        CinematicCam.target = Car.transform;
        brakelght = new GameObject[car.BrakeLight.Length];
        Array.Copy(car.BrakeLight, brakelght, car.BrakeLight.Length);

        if (levelStats.greenred != null)
        {
            greenred = new GameObject[levelStats.greenred.Length];
            Array.Copy(levelStats.greenred, greenred, levelStats.greenred.Length);
        }
        
        if (lvlstats.IsDisabledTraffic)
            trafficSpawner.DisableAllCars();

        IsStayinginLane = lvlstats.IsStayinLane;
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
        {
            soundmgr.PlayCompleteSound(true);
            soundmgr.playindiSound(false);
        }

        CarSound(false);


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

        UnlckNxtLvl();

        yield return new WaitForSeconds(10f);
        if (soundmgr)
            soundmgr.PlayCompleteSound(false);

        BlckPnl.SetActive(false);
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
        int duration = 3; // Total duration for the animation
        float elapsedTime = 0f; // Time elapsed since the start of the animation
        int currentCoins = 0;

        // Play sound if available
        if (soundmgr)
            soundmgr.PlaycoinSound();

        // Calculate the number of coins per second
        int coinsPerSecond = totalCoins / duration;

        // Loop until the animation reaches the total coins
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime; // Accumulate elapsed time
            currentCoins = Mathf.FloorToInt(coinsPerSecond * elapsedTime); // Increment coins

            // Make sure currentCoins does not exceed totalCoins
            currentCoins = Mathf.Min(currentCoins, totalCoins);

            // Update the UI or text with the current number of coins
            TotalCompltxt.text = currentCoins.ToString();

            yield return null; // Wait until the next frame
        }

        // Ensure the final count is exactly totalCoins
        TotalCompltxt.text = totalCoins.ToString();

        // Stop sound if available
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



    void UnlckNxtLvl() 
    {
        int currlvl = ValStorage.selLevel;
        int unlockdlvls = ValStorage.GetUnlockedLevels();

        if (currlvl == unlockdlvls) 
        {
            ValStorage.SetUnlockedLevels(unlockdlvls + 1);
        }
    }

    public void NextLvlBtn() 
    {
        Loading.SetActive(true);
        LoadBar.SetActive(true);
       

        int currentLevelIndex = ValStorage.selLevel;

        if (currentLevelIndex < lvlcs.Length)
        {
            ValStorage.selLevel += 1;
            StartCoroutine(LoadAsyncScene("GamePlay"));
        }
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
    
    public bool IsIndsiactive()
    {
       if(LeftIndActv.activeSelf || RightIndActv.activeSelf) 
        {
            return true;
        }
        else 
        {
          return   false;
        }
    }


    public void IndiLft()
    {
        // Check if the right indicator is active, and deactivate it if necessary
        if (RightIndActv.activeSelf)
        {
            RightIndActv.SetActive(false);
           // IndiRght.SetActive(false);
        }

        // Toggle the left indicator
        if (LeftIndActv.activeSelf)
        {
            LeftIndActv.SetActive(false);
           // Indilft.SetActive(false);
            car.currentState = PlayerState.None;
            if (soundmgr)
                soundmgr.playindiSound(false);
        }
        else
        {
            LeftIndActv.SetActive(true);
          //  Indilft.SetActive(true);
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
           // Indilft.SetActive(false);
        }

        // Toggle the right indicator
        if (RightIndActv.activeSelf)
        {
            RightIndActv.SetActive(false);
          //  IndiRght.SetActive(false);
            car.currentState = PlayerState.None;

            if (soundmgr)
                soundmgr.playindiSound(false);

        }
        else
        {
            RightIndActv.SetActive(true);
           // IndiRght.SetActive(true);
            car.currentState = PlayerState.RightIndicator;

            // Optionally play sound for right indicator here
            if (soundmgr)
                soundmgr.playindiSound(true);
        }
    }

    #endregion



    [HideInInspector]public bool IsGreenEnabled;

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


        IsGreenEnabled = true;
//        yield return new WaitForSeconds(0.3f);

        //AppreciateCoinAdd("You Followed Traffic Signal Rule");
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
        if (brakelght!=null && HasBrakeStateChanged())
        {
            UpdateBrakeLightColor(Brake.pressing);
            isBrakePressed = Brake.pressing;
        }

        if (IsTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerText();
        }


        if (IsStayinginLane && Car.speed>=10f) 
        {
            LaneTimer += Time.fixedDeltaTime;
            if (LaneTimer >= 25f) 
            {
                AppreciateCoinAdd("You Followed Lane Rule");
                LaneTimer = 0f;
            }
        }
    }





    private bool HasBrakeStateChanged()
    {
        return Brake.pressing != isBrakePressed;
    }

    private void UpdateBrakeLightColor(bool isPressed)
    {
        //Color color = isPressed ? Color.red : Color.grey; // Use Unity's predefined colors for clarity

        foreach(GameObject mesh in brakelght) 
        {
            //mesh.material.color = color;
            mesh.gameObject.SetActive(isPressed);
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




    public void PlayWalk() 
    {
        Transform Pedestrians = lvlstats.Pedestians;
        foreach (Transform child in Pedestrians)
        {
            // Get the Animator component from each child
            Animator animator = child.GetComponent<Animator>();
            WaypointsTraveler traveler = child.GetComponent<WaypointsTraveler>();

            // If an Animator is attached to the child, set the "IsWalk" bool to true
            if (animator != null)
            {
                animator.SetBool("IsWalk", true);
            }
            
            if (traveler != null)
            {
                traveler.enabled = true;
            }
        }

        StartCoroutine(DelayStopWalk(Pedestrians));

    }
    IEnumerator DelayStopWalk(Transform Ped) 
    {

        yield return new WaitForSeconds(17f);
        foreach (Transform child in Ped)
        {
            // Get the Animator component from each child
            Animator animator = child.GetComponent<Animator>();
            WaypointsTraveler traveler = child.GetComponent<WaypointsTraveler>();

            // If an Animator is attached to the child, set the "IsWalk" bool to true
            if (animator != null)
            {
                animator.SetBool("IsWalk", false);
                animator.SetBool("IsWait", true);
            }

            if (traveler != null)
            {
                traveler.enabled = false;
            }
        }
        HasPedestriansCrossed = true;
    }

    public void ShowOtherCam() 
    {
        IsStoppedAtPolice = true;
        Cam.enabled = false;
      
        if (lvlstats.Cam) 
        {
            lvlstats.Cam.SetActive(true);
        }
        if (lvlstats.Filler) 
        {
            ControllerBtns.alpha = 0f;
            GameObject filer = lvlstats.Filler;
            filer.SetActive(true);
            StartCoroutine(FillImageOverTime(filer.transform.GetChild(0).gameObject.GetComponent<Image>()));
        }
    }

    private IEnumerator FillImageOverTime(Image fillerimg)
    {
        float elapsedTime = 0f;  // Track the elapsed time

        // While the elapsed time is less than the fill duration
        while (elapsedTime < 4f)
        {
            // Increment elapsed time based on the frame time
            elapsedTime += Time.deltaTime;

            // Set the fillAmount of the image, clamped between 0 and 1
            fillerimg.fillAmount = Mathf.Clamp01(elapsedTime / 4);

            // Wait for the next frame
            yield return null;
        }

        // Ensure the fillAmount is set to 1 at the end (to handle precision issues)
        fillerimg.fillAmount = 1f;
        yield return new WaitForSeconds(1f);
        StartCoroutine(OffCam()); 
    }
    IEnumerator OffCam() 
    {
        lvlstats.Filler.SetActive(false);
        Cam.enabled = true;
        lvlstats.Cam.SetActive(false);
        Car.GetComponent<Rigidbody>().isKinematic = false;
        ControllerBtns.alpha = 1f;
        yield return new WaitForSeconds(1f);
        AppreciateCoinAdd("You Stopped At Police CheckPoint");
    }
    void CarSound(bool IsActive) 
    {

        Transform child = Car.transform.Find("All Audio Sources");


        if (child != null)
        {
            child.gameObject.SetActive(IsActive);

        }
        else
        {
         

            Debug.LogError("Object not found!");
        }
    }
  
    public void Pause()
    {
        if(soundmgr)
            soundmgr.PauseSounds();
       
        CarSound(false);
   

        Time.timeScale = 0f;
        PausePnl.SetActive(true);
    }
   
    public void Resume() 
    {

        if (soundmgr)
            soundmgr.ResumeSounds();

        CarSound(true);
        Time.timeScale = 1f;
        PausePnl.SetActive(false);
    }
    
    public void Restart() 
    {
        Time.timeScale = 1f;
        Loading.SetActive(true);
        LoadBar.SetActive(true);
        StartCoroutine(LoadAsyncScene("GamePlay"));

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
        yield return new WaitForSeconds(0.1f);
        asyncLoad.allowSceneActivation = true;
    }
}



// add headlights
//car sound off on complete 
//indi sound off on complete

