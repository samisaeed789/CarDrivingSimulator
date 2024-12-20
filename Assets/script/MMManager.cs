using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MMManager : MonoBehaviour
{
    public static MMManager Instance;
    [Header("GameObjects/Panels")]
    public GameObject mainMenuPanel;
    public GameObject modeSelectionPanel;
    public GameObject levelSelectionPanel;
    public GameObject levelSelectionParkingPanel;
    public GameObject loadingScreenPanel;
    public GameObject exitPanel;
    public GameObject SettingsPanel;
    public GameObject GaragePanel;
    public GameObject CarsCont;


    [Header("Settings")]
    public GameObject sfx_pnl;
    public GameObject cntrl_pnl;
    public GameObject grphc_pnl;

    public GameObject sfx_active;
    public GameObject cntrl_active;
    public GameObject grphc_active;

    public GameObject grphc_High;
    public GameObject grphc_Med;
    public GameObject grphc_Low;


    public GameObject cntrl_Steering;
    public GameObject cntrl_Arrow;
    public GameObject cntrl_Tilt;

    [Header("Music-Volume")]
    public Image fillBar;
    private float volume = 0.5f; // Initial volume (full volume)
    private float volumeChangeAmount = 0.1f; // How much the volume changes with each 

    [Header("Sound-Volume")]
    public Image soundfillBar;
    private float soundvolume = 0.5f; // Initial volume (full volume)

    [Header("Sound-Volume")]
    public Image BothfillBar;

    [Header("Transparency")]
    private float transparencyChangeAmount = 0.1f; // A
    [SerializeField] private Text transparencyChange; // A







    [Header("Texts")]
    public Text[] Coins;
    public Text prcnttxt;    
    [SerializeField] Animator sphere;    


    [Header("OtherClasses")]
    public GarageHndlr garage;

    [Header("OtherClasses")]
    public Button[] LvlCards;
    public Button[] LvlCardsParking;
    [SerializeField] RGSK.Reflection reflection;



    private AsyncOperation async;
    public Image loadingBar;


    public static int Levelno;


    //[Header("Temp")]
    //public bool GiveCoins;

    MySoundManager soundmng;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;


        if (PlayerPrefs.GetInt("UnlockedLevels") == 0)
        {
           PlayerPrefs.SetInt("UnlockedLevels", 1);
        }
        if (ValStorage.GetUnlockedLevelsParking() == 0)
        {
            ValStorage.SetUnlockedLevelsParking(1);
        }
        CheckUnlocked();
        CheckUnlockedParking();

        if (ValStorage.GetCoins() < 0) 
        {
            ValStorage.SetCoins(0);
        }

        soundmng = MySoundManager.instance;

        //values for stngs
        SetControlsTTNGS();
        UpdateVolume();
        DispTrnsprncy();

        if (!PlayerPrefs.HasKey("GQuality"))
        {
            Debug.LogError("IsLowEndDevice    " + IsLowEndDevice());


            if(IsLowEndDevice()==true)
                ValStorage.SetGQuality(0);

            if (IsLowEndDevice()==false)
                ValStorage.SetGQuality(1);
        }
        SetSettingsQuality();


    }
    //private void OnEnable()
    //{
    //   AdsManager.instance?.OnWathcVideo.AddListener(Delaywatchvid);
    //}
    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        SetCoins();
        Time.timeScale = 1f;

       


        if (AdsManager.instance )
            AdsManager.instance.showAdmobAdpativeBannerTop();
    }

    void CheckUnlocked()
    {
        int numUnlockedLevels = ValStorage.GetUnlockedLevels();
        // Loop through all the level buttons in your UI
        for (int i = 1; i <= LvlCards.Length; i++)
        {
            // Get a reference to the button
            Button levelButton = LvlCards[i - 1];

            if (levelButton != null)
            {
                // If this level is unlocked, make the button interactable
                if (i <= numUnlockedLevels)
                {
                    levelButton.interactable = true;
                    levelButton.transform.GetChild(1).gameObject.SetActive(false);

                }
                else
                {
                    // If this level is locked, make the button not interactable
                    levelButton.interactable = false;
                    levelButton.transform.GetChild(1).gameObject.SetActive(true);
                }
            }
        }
    }
    
    void CheckUnlockedParking()
    {
        int numUnlockedLevels = ValStorage.GetUnlockedLevelsParking();
        // Loop through all the level buttons in your UI
        for (int i = 1; i <= LvlCardsParking.Length; i++)
        {
            // Get a reference to the button
            Button levelButton = LvlCardsParking[i - 1];

            if (levelButton != null)
            {
                // If this level is unlocked, make the button interactable
                if (i <= numUnlockedLevels)
                {
                    levelButton.interactable = true;
                    levelButton.transform.GetChild(1).gameObject.SetActive(false);

                }
                else
                {
                    // If this level is locked, make the button not interactable
                    levelButton.interactable = false;
                    levelButton.transform.GetChild(1).gameObject.SetActive(true);
                }
            }
        }
    }

    public void BackBtn(string S)
    {
        if (S == "ModeSel")
        {
            PanelActivity(ModeSel: true);
           
        }
        if (S == "LvlSel")
        {
            Disablehildren();
            PanelActivity(LvlSel: true);

        }
        if (S == "LvlSelParking")
        {
            Disablehildren();
            PanelActivity(LvlSelParking: true);

        }
        if (S == "Exit")
        {

            if (AdsManager.instance)
                AdsManager.instance.showAdMobRectangleBannerBottomLeft();

            PanelActivity(ExitPnl: true);
        }
        if (S == "MM")
        {
            PanelActivity(MM: true);

            if(AdsManager.instance)
                AdsManager.instance.hideAdmobBottomLeftBanner();

        }
        if (S == "Garage")
        {
            PanelActivity(Garage: true);
        }

        MySoundManager.instance.PlayButtonClickSound(1);
    }

    void Disablehildren()
    {
        CarsCont.SetActive(false);
    }
    public void SelMode(string Mode)
    {
        ValStorage.modeSel = Mode;

        if (Mode == "DrivingMode") 
        {
            BackBtn("LvlSel");
        }
        else if(Mode == "ParkingMode")
        {
            BackBtn("LvlSelParking");
        }
    }


    public void SelLevel(int i)
    {
        ValStorage.selLevel = i;
        CarsCont.SetActive(true);
        BackBtn("Garage");
    }
    
    public void SelLevelParking(int i)
    {
        ValStorage.selLevelParking = i;
        CarsCont.SetActive(true);
        BackBtn("Garage");
    }


    public void OnVolumeChanged(float value)
    {
        if (MySoundManager.instance)
            MySoundManager.instance.OnVolumeChanged(value);
    }


    public void LoadNxtScene()
    {
        CarsCont.SetActive(false);
        PanelActivity(IsLoading: true);

        if(AdsManager.instance)
            AdsManager.instance.showAdMobRectangleBannerBottomLeft();


        ValStorage.SetCarNumber(garage.GetCurrCarNumber());

        if(ValStorage.modeSel=="DrivingMode")
            StartCoroutine(LoadAsyncScene("Gameplay"));

        else
            StartCoroutine(LoadAsyncScene("ParkingMode"));


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
                prcnttxt.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
            else
            {
                
                loadingBar.fillAmount = 1f;
                prcnttxt.text = "100%";

                Debug.Log("allow");
               
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
        sphere.enabled = false;
        
        if(AdsManager.instance)
           AdsManager.instance.hideAdmobBottomLeftBanner();

        yield return new WaitForSeconds(0.1f);
        asyncLoad.allowSceneActivation = true;
    }
   

    public void PanelActivity(bool MM = false, bool ModeSel = false, bool LvlSel = false, bool LvlSelParking = false, bool ExitPnl = false, bool SettingsPnl = false, bool Garage = false, bool IsLoading = false)
    {

        if (mainMenuPanel)
        {
            mainMenuPanel.SetActive(MM);
        }

        if (modeSelectionPanel)
        {
            modeSelectionPanel.SetActive(ModeSel);
        }

        if (levelSelectionPanel)
        {
            levelSelectionPanel.SetActive(LvlSel);
        }
        
        if (levelSelectionParkingPanel)
        {
            levelSelectionParkingPanel.SetActive(LvlSelParking);
        }

        if (exitPanel)
        {
            exitPanel.SetActive(ExitPnl);
        }

        if (SettingsPanel)
        {
            SettingsPanel.SetActive(SettingsPnl);
        }

        if (GaragePanel)
        {
            GaragePanel.SetActive(Garage);
        }

        if (loadingScreenPanel)
        {
            loadingScreenPanel.SetActive(IsLoading);
        }

    }




    public void SetCoins()
    {
        foreach (Text txt in Coins)
        {
            txt.text = PlayerPrefs.GetInt("Coins").ToString();
        }
    }

  


    public void Exit(bool val)
    {
        if (exitPanel.activeSelf)
        {

            if (val == true)
            {
                Application.Quit();
            }
            else
            {
                exitPanel.SetActive(false);
               
            }
        }

    }

    public void SettngsONOFF(bool State)
    {
        if (MySoundManager.instance)
            MySoundManager.instance.PlayButtonClickSound(1f);

        SettingsPanel.SetActive(State);
    }




    #region Settings
    public void sttngstab(string S)
    {

        if (S == "SFX")
        {
            SettngsActivity(isSfx: true);

        }
        if (S == "Graphics")
        {
            SettngsActivity(isGraphic: true);


        }
        if (S == "Control")
        {
            SettngsActivity(isControl: true);
        }
        if (MySoundManager.instance)
            MySoundManager.instance.PlayButtonClickSound(1f);
    }



    public void SettngsActivity(bool isSfx = false, bool isGraphic = false, bool isControl = false)
    {
        if (sfx_pnl)
        {
            sfx_pnl.SetActive(isSfx);
            sfx_active.SetActive(isSfx);
        }
        if (grphc_pnl)
        {
            grphc_pnl.SetActive(isGraphic);
            grphc_active.SetActive(isGraphic);
        }
        if (cntrl_pnl)
        {
            cntrl_pnl.SetActive(isControl);
            cntrl_active.SetActive(isControl);

        }
    }



    #region Graphics


    public void SetGraphics(string s)
    {
        if (s == "High")
        {
            reflection.SetRefQuality(3);
            GrphicsActivity(isHigh: true);
        }

        if (s == "Med")
        {
            reflection.SetRefQuality(2);
            GrphicsActivity(isMed: true);
        }

        if (s == "Low")
        {
            reflection.SetRefQuality(1);
            GrphicsActivity(isLow: true);
        }
        if (MySoundManager.instance)
            MySoundManager.instance.PlayButtonClickSound(1f);
    }

    public void GrphicsActivity(bool isLow = false, bool isMed = false, bool isHigh = false)
    {
        if (grphc_Low)
        {
            grphc_Low.SetActive(isLow);
        }
        if (grphc_Med)
        {
            grphc_Med.SetActive(isMed);
        }
        if (grphc_High)
        {
            grphc_High.SetActive(isHigh);
        }
    }


    #endregion

    #region Controls


    public void SetControls(string s)
    {
        if (s == "Steer")
        {
            ControlsActivity(isSteer: true); ;
        }

        if (s == "Arrow")
        {
            ControlsActivity(isArrow: true);
        }

        if (s == "Tilt")
        {
            ControlsActivity(isTilt: true);
        }
        if (MySoundManager.instance)
            MySoundManager.instance.PlayButtonClickSound(1f);

      
    }

    public void ControlsActivity(bool isSteer = false, bool isArrow = false, bool isTilt = false)
    {
        if (cntrl_Steering)
        {
            cntrl_Steering.SetActive(isSteer);
        }
        if (cntrl_Arrow)
        {
            cntrl_Arrow.SetActive(isArrow);
        }
        if (cntrl_Tilt)
        {
            cntrl_Tilt.SetActive(isTilt);
        }
    }


    #endregion

    #region volume

    public void AdjustVolume(float changeAmount, bool isMusic)
    {
        // Determine if it's music volume or sound volume
        if (isMusic)
        {
            float vol = Mathf.Clamp(ValStorage.GetMVolume() + changeAmount, 0f, 1f); // Adjust and clamp volume for music
            ValStorage.SetMVolume(vol);
        }
        else
        {
            float soundvol = Mathf.Clamp(ValStorage.GetSVolume() + changeAmount, 0f, 1f); // Adjust and clamp volume for sound effects
            ValStorage.SetSVolume(soundvol);
        }

        UpdateVolume(); // Update both volume and UI
    }

    // This method updates both music and sound volumes
    private void UpdateVolume()
    {
        // Set the volume for music and sound effects
        soundmng.BGM.volume = ValStorage.GetMVolume(); // Music volume
        soundmng.Effectsource.volume = ValStorage.GetSVolume(); // Sound effect volume

        // Update the fill bars for both music and sound
        UpdateFillBar();
    }

    // This method updates the fill bars based on the current volume levels
    private void UpdateFillBar()
    {
        // Update fill bars
        if (fillBar != null)
        {
            fillBar.fillAmount = ValStorage.GetMVolume(); // Set music fill bar
        }

        if (soundfillBar != null)
        {
            soundfillBar.fillAmount = ValStorage.GetSVolume();  // Set sound fill bar
        }

        if (BothfillBar != null)
        {
            BothfillBar.fillAmount = Mathf.Max(ValStorage.GetMVolume(), ValStorage.GetSVolume()); // Set fill bar for both
        }

        // Play a button click sound (optional)
        if (MySoundManager.instance != null)
        {
            MySoundManager.instance.PlayButtonClickSound(1f);
        }
    }

    // Public methods to handle button clicks
    public void DecreaseMusicVolume()
    {
        AdjustVolume(-volumeChangeAmount, true); // Decrease music volume
    }

    public void IncreaseMusicVolume()
    {
        AdjustVolume(volumeChangeAmount, true); // Increase music volume
    }

    public void DecreaseSoundVolume()
    {
        AdjustVolume(-volumeChangeAmount, false); // Decrease sound volume
    }

    public void IncreaseSoundVolume()
    {
        AdjustVolume(volumeChangeAmount, false); // Increase sound volume
    }

    public void DecreaseBothVolume()
    {
        DecreaseMusicVolume();
        DecreaseSoundVolume(); // Decrease both music and sound volume
    }

    public void IncreaseBothVolume()
    {
        IncreaseMusicVolume();
        IncreaseSoundVolume(); // Increase both music and sound volume
    }





    #endregion

    #region TRANSPARENCY

    // Increase the transparency value (if not at maximum)
    public void IncreaseTransparency()
    {
        if (soundmng)
            soundmng.PlayButtonClickSound(1f);


        int trans = ValStorage.GetTransparency();
        if (trans < 5)
        {
            trans++;
            ValStorage.SetTransparency(trans);

        }
        DispTrnsprncy();
    }

    // Decrease the transparency value (if not at minimum)
    public void DecreaseTransparency()
    {

        if (soundmng)
            soundmng.PlayButtonClickSound(1f);

        int trans = ValStorage.GetTransparency();
        if (trans > 1)
        {
            trans--;
            ValStorage.SetTransparency(trans);
        }
        DispTrnsprncy();
    }


    public void DispTrnsprncy()
    {
        transparencyChange.text = ValStorage.GetTransparency().ToString();
    }
    #endregion




    #region Controls

    public void SetControlVal(int val)
    {
        ValStorage.SetControls(val);
    }
    public void SetControlsTTNGS()
    {
        if (ValStorage.GetControls()==0) 
        {
            ControlsActivity(isArrow:true);
        }
        if (ValStorage.GetControls()==1) 
        {
            ControlsActivity(isTilt: true);

        }
        if (ValStorage.GetControls()==2) 
        {
            ControlsActivity(isSteer: true);

        }
    }
    #endregion





    #region quality

    public void SetSettingsQuality() 
    {
        if (ValStorage.GetGQuality() == 0) 
        {
            GrphicsActivity(isLow: true);
        }
        if (ValStorage.GetGQuality() == 1) 
        {
            GrphicsActivity(isMed: true);
        }
        if (ValStorage.GetGQuality() == 2) 
        {
            GrphicsActivity(isHigh: true);
        }
    }

    public void SetQuality(int val) 
    {
        ValStorage.SetGQuality(val);
        QualitySettings.SetQualityLevel((int)val, true);
    }


    

    private bool IsLowEndDevice()
    {
        int totalRam = SystemInfo.systemMemorySize;

        if (totalRam <= 3000 )
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

    public void WatchAd() 
    {
        if (AdsManager.instance)
        {
            if (AdsManager.instance.rewardedInterstitialAD != null)
            {
                AdsManager.instance.ShowAdmobRewardedInterstitial();
            }
        }
    }
    void Delaywatchvid() 
    {
        Invoke(nameof(GrantCoins),0.2f);
    }

    public void GrantCoins() 
    {
        int alreadycoins=ValStorage.GetCoins();
        ValStorage.SetCoins(alreadycoins+300);
        SetCoins();
       
    }
    
    #endregion
}

    






#endregion





