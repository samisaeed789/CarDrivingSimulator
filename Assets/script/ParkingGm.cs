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
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject envBG;



    [Header("Others")]
    [SerializeField] RCC_CarControllerV3 carController;
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
    [SerializeField] GameObject MusicOff;
    [SerializeField] GameObject Belt;
    [SerializeField] GameObject Beltbtn;
    [SerializeField] GameObject headLight;
    [SerializeField] GameObject headLightActvbtn;
    [SerializeField] RCC_Demo Controls;
    [SerializeField] GameObject gearup;
    [SerializeField] GameObject geardown;
    [SerializeField] GameObject NxtBtnSccs;
    [SerializeField] Image[] UIgp;
    [SerializeField] GameObject UIBlocker;


    [Header("Text")]
    [SerializeField] Text timerText;
    [SerializeField] Text ComptimeText;
    [SerializeField] Text CoinsEarnedlvltxt;
    [SerializeField] Text TotalCompltxt;





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
    float elapsedTime = 0f;
    bool stopAnimation;


    public static ParkingGm instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;

    }
    IEnumerator Start()
    {

        soundManager = MySoundManager.instance;

        UpdateVolume();
        SetButtonTransparency(ValStorage.GetTransparency());
        Controls.SetMobileController(ValStorage.GetControls());

        int currentlvl;
        int selCar;  

        if (Test)
        {
            selCar = Carnumber;
            currentlvl = levelnumber;
        }
        else
        {
            selCar = ValStorage.GetCarNumber();
            currentlvl = ValStorage.selLevelParking;
        }
        GameObject CarSel = Cars[selCar - 1];
        CarSel.SetActive(true);
        carController = CarSel.GetComponent<RCC_CarControllerV3>();





        Levels[currentlvl - 1].gameObject.SetActive(true);
        yield return new WaitForSeconds(0f);
      
    }

    public void OnLevelStatsLoadedHandler(LD_Park lvlData) 
    {
        lvldata = lvlData;
        rbCar = carController.gameObject.GetComponent<Rigidbody>();
        rccCam.cameraMode = RCC_Camera.CameraMode.TOP;
        RCC_CameraCarSelection carselcam = rccCam.gameObject.GetComponent<RCC_CameraCarSelection>();
        carselcam.target = carController.transform;
        carController.StartEngine();
        ObstacleColl Cardata = carController.gameObject.GetComponent<ObstacleColl>();
        if (Cardata.Headlight != null)
        {
            headLight = Cardata.Headlight;
        }


        if (Cardata.Taillights != null)
            taillights = Cardata.Taillights;

        if (lvldata.Confetti.Length != 0) 
        {
            lvlconfti = new ParticleSystem[lvldata.Confetti.Length];
            Array.Copy(lvldata.Confetti, lvlconfti, lvldata.Confetti.Length);
        }


        Transform Spawn = GetCarspawn();
        StartCoroutine(CheckCarDestination(Spawn.localPosition));
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
        Vector3 currentPosition = SpawnPoint;

        currentPosition.z -= 15f;

        rbCar.position = currentPosition;
    }


    Transform GetCarspawn() 
    {
        string name = rbCar.gameObject.name;
        Transform SP;
        if (name == "Jeep")
        {
            SP = lvldata.SpawnPointJeep;
        }
        else if (name== "Mazda") 
        {
            SP = lvldata.SpawnPoint;
        }
        else if (name== "Audi_Etron") 
        {
            SP = lvldata.SpawnPointAudi ;
        }
        else if (name == "Bugatti")
        {
            SP = lvldata.SpawnPointBugatti ;
        }
        else 
        {
            return null;
        }

        return SP;
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
        rbCar.isKinematic = true;
        canvas.alpha = 0f;
        UIBlocker.SetActive(true);

        foreach (ParticleSystem particle in lvlconfti) 
        {
            particle.Play();
        }
        StartDance();
        if (soundManager)
        {
            soundManager.PlayCompleteSound(true);
        }
        Invoke(nameof(Celeb),0.5f);
    }
    void Celeb() 
    {
     
        RCC_CameraCarSelection celebCam = rccCam.gameObject.GetComponent<RCC_CameraCarSelection>();
        celebCam.enabled = true;
        celeb.SetActive(true);
        IsTimerRunning = false;
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

        if (soundManager)
            soundManager.SetBGM(true);
    }
    void Shakecam() 
    {
        Cam.DOShakePosition(0.5f, 0.5f, 10, 90f).OnKill(() => OnShakeComplete());
    }
    void OnShakeComplete() 
    {
        canvas.alpha = 1f;
        UIBlocker.SetActive(false);

        canvas.gameObject.GetComponent<UIAnimator>().PlayAnimation(AnimSetupType.Intro);
        envBG.SetActive(true);
    }



    private void Update()
    {
        if (taillights != null && HasBrakeStateChanged())
        {
            UpdateBrakeLightColor(BrakeBtn.pressing);
            isBrakePressed = BrakeBtn.pressing;
        }

        if (IsTimerRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerText();
        }



    }
    void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60); // Divide elapsed time by 60 to get minutes
        int seconds = Mathf.FloorToInt(elapsedTime % 60); // Get the remainder for seconds

        string timeFormatted = string.Format("{0:D2}:{1:D2}", minutes, seconds);

        // Update the UI Text
        timerText.text = timeFormatted;
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
        if (soundManager)
            soundManager.PlayLevelFailSound();

        CarSound(false);
        canvas.alpha = 0f;
        UIBlocker.SetActive(true);
        StartCoroutine(FailPanel());
    }

    IEnumerator FailPanel() 
    {
        yield return new WaitForSeconds(3f);
        emojiPanel.SetActive(true);
        yield return new WaitForSeconds(4f);
        failPanel.SetActive(true);
    }
    IEnumerator CompletePanel() 
    {
        
        UnlckNxtLvl();

        yield return new WaitForSeconds(7f);
        if (soundManager)
        {
            soundManager.PlayCompleteSound(false);
            CarSound(false);
        }

        completePanel.SetActive(true);

        SetCoinsinPanel();


        if (AdsManager.instance)
            AdsManager.instance.showAdMobRectangleBannerBottomLeft();

    }
    void UnlckNxtLvl()
    {

        int currlvl = ValStorage.selLevelParking;
        int unlockdlvls = ValStorage.GetUnlockedLevelsParking();

        if (currlvl == unlockdlvls)
        {
            ValStorage.SetUnlockedLevelsParking(unlockdlvls + 1);
        }

        if (currlvl == 7)
        {
            NxtBtnSccs.SetActive(false);
        }
    }

    void SetCoinsinPanel()
    {
      
        timerText.text = Mathf.FloorToInt(elapsedTime * 2).ToString();
        CoinsEarnedlvltxt.text = 500.ToString();
        StartCoroutine(CounterAnimation(CalculateTotalCoins()));


        int alreadycoins = ValStorage.GetCoins();
        int totalcoins = alreadycoins + CalculateTotalCoins();
        ValStorage.SetCoins(totalcoins);
    }
    private int CalculateTotalCoins()
    {
        int coinsFromTime = Mathf.FloorToInt(elapsedTime * 2);

        int total = 500 + coinsFromTime;
        return total;
    }
    private IEnumerator CounterAnimation(int totalCoins)
    {

        yield return new WaitForSeconds(1f);
        int duration = 3; // Total duration for the animation
        float elapsedTime = 0f; // Time elapsed since the start of the animation
        int currentCoins = 0;

        // Play sound if available
        if (soundManager)
            soundManager.PlaycoinSound();

        // Calculate the number of coins per second
        int coinsPerSecond = totalCoins / duration;

        // Loop until the animation reaches the total coins
        while (elapsedTime < duration && !stopAnimation)
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
        if (soundManager)
            soundManager.StopcoinSound();

    }
    public void Restart()
    {
        Time.timeScale = 1f;
        StopCoinAnimation();
        Loading.SetActive(true);
        LoadBar.SetActive(true);
        StartCoroutine(LoadAsyncScene("ParkingMode"));
    }

    public void Home()
    {
        Time.timeScale = 1f;
        StopCoinAnimation();
        Loading.SetActive(true);
        LoadBar.SetActive(true);
        StartCoroutine(LoadAsyncScene("MM"));
    }

    public void NextLvlBtn()
    {
        Loading.SetActive(true);
        LoadBar.SetActive(true);

        StopCoinAnimation();
        int currentLevelIndex = ValStorage.selLevel;

        if (currentLevelIndex < Levels.Length)
        {
            ValStorage.selLevelParking += 1;
            StartCoroutine(LoadAsyncScene("ParkingMode"));
        }
    }

    public void StopCoinAnimation()
    {
        stopAnimation = true;
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


        if (soundManager)
            soundManager.PlayButtonClickSound(1f);
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
    public void ChangeControl()
    {

        if (soundManager)
            soundManager.PlayButtonClickSound(1f);

        int currentind = ValStorage.GetControls();

        currentind = (currentind + 1) % 3;
        Controls.SetMobileController(currentind);
        ValStorage.SetControls(currentind);

    }

    public void Pause()
    {
        if (soundManager)
            soundManager.PauseSounds();


        if (soundManager)
            soundManager.PlayButtonClickSound(1f);

        CarSound(false);
        pausePanel.SetActive(true);

        if (AdsManager.instance)
            AdsManager.instance.showAdMobRectangleBannerBottomLeft();

        Time.timeScale = 0f;
    }
    void CarSound(bool IsActive)
    {
        Transform child = carController.transform.Find("All Audio Sources");
        if (child != null)
        {
            child.gameObject.SetActive(IsActive);
        }
        else
        {
            UnityEngine.Debug.LogError("Object not found!");
        }
    }


    public void Resume()
    {
        if (soundManager)
            soundManager.ResumeSounds();

        CheckMusis();

        CarSound(true);
        if (AdsManager.instance)
            AdsManager.instance.hideAdmobBottomLeftBanner();
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    void CheckMusis()
    {
        if (MusicOff.activeSelf)
        {

            if (soundManager)
            {
                soundManager.SetBGM(false);  // Start playing background music
            }
        }
        else
        {

            if (soundManager)
            {
                soundManager.SetBGM(true);  // Stop playing background music
            }
        }

    }

    public void PlayStopMusic()
    {
        // If the MusicOff button is active (meaning music is currently off)
        if (MusicOff.activeSelf)
        {
            MusicOff.SetActive(false);  // Hide the "Music Off" button
            if (soundManager)
            {
                soundManager.SetBGM(true);  // Start playing background music
            }
        }
        else
        {
            MusicOff.SetActive(true);  // Show the "Music Off" button
            if (soundManager)
            {
                soundManager.SetBGM(false);  // Stop playing background music
            }
        }
    }

    public void PlayHorn()
    {
        if (soundManager)
        {
            soundManager.SetBGM(true);  // Start playing background music
        }
    }

    public void OnButtonPressed()
    {

        if (soundManager)
        {
            soundManager.PlayHorn();
        }

    }

    public void OnButtonReleased()
    {

        if (soundManager)
        {
            soundManager.StopHorn();
        }
    }

    public void ToggleSeatBelt()
    {
        if (soundManager)
            soundManager.PlayButtonClickSound(1f);

        Belt.SetActive(true);
        Beltbtn.SetActive(false);
        Invoke(nameof(delayoff), 1.05f);
    }

    public void ToggleHeadlight()
    {

        if (soundManager)
            soundManager.PlayButtonClickSound(1f);
        if (headLight != null)
        {
            // Toggle the active state of the headlight
            headLight.SetActive(!headLight.activeSelf);
        }

        if (headLightActvbtn != null)
        {
            headLightActvbtn.SetActive(!headLight.activeSelf);
        }
    }

    void delayoff()
    {
        Belt.SetActive(false);
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
                float progress = Mathf.Clamp01(timer / 5f);  
                loadingBar.fillAmount = progress;
                percentageText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
            else
            {
                
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

    public void PlayHIT()
    {
        if (soundManager)
            soundManager.PlayHitSound();
    }


    private void UpdateVolume()
    {
        if (soundManager) 
        {
            soundManager.BGM.volume = ValStorage.GetMVolume(); // Music volume
            soundManager.Effectsource.volume = ValStorage.GetSVolume(); // Sound effect volume
        }

    }

    public void SetButtonTransparency(int transval)
    {
        transval = Mathf.Clamp(transval, 1, 5);

        float alpha = Mathf.Lerp(0.2f, 1f, (transval - 1) / 4f);


        foreach (Image UI in UIgp)
        {
            Color buttonColor = UI.color;
            buttonColor.a = alpha;  // Set alpha based on the calculation
            UI.color = buttonColor;
        }
    }

   
}



