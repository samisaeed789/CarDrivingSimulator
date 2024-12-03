using System.Collections;
using System.Collections.Generic;
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
    public Text percentageText;



    [Header("OtherClasses")]
    public GarageHndlr garage;

    [Header("OtherClasses")]
    public Button[] LvlCards;


    private AsyncOperation async;
    public Image loadingBar;


    public static int Levelno;


    [Header("Temp")]
    public bool GiveCoins;

    MySoundManager soundmng;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        //PlayerPrefs.SetInt("UnlockedLevels", 5);

        if (PlayerPrefs.GetInt("UnlockedLevels") == 0)
        {
           PlayerPrefs.SetInt("UnlockedLevels", 1);
        }
        CheckUnlocked();

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



    private void Start()
    {


        if (GiveCoins)
            ValStorage.SetCoins(5000);

        SetCoins();
        Time.timeScale = 1f;

      //  Application.targetFrameRate = 120;

      //  IsLowEndDevice();
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
        if (S == "Exit")
        {
            PanelActivity(ExitPnl: true);
        }
        if (S == "MM")
        {
            PanelActivity(MM: true);
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
        //foreach (Transform child in CarsCont.transform)
        //{
        //    // Check if the child game object is active/enabled
        //    if (child.gameObject.activeSelf)
        //    {
        //        // Disable the child game object
        //        child.gameObject.SetActive(false);
        //    }
        //}
    }
    public void SelMode(string Mode)
    {
        ValStorage.modeSel = Mode;
        BackBtn("LvlSel");
    }


    public void SelLevel(int i)
    {
        ValStorage.selLevel = i;
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

        ValStorage.SetCar(garage.GetCurrCarId());
        StartCoroutine(LoadAsyncScene("Gameplay"));
    }


    IEnumerator LoadAsyncScene(string sceneName)
    {


        //// Start loading the scene asynchronously
        async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false; // Prevent the scene from activating immediately



        //// Loop while the scene is loading
        while (!async.isDone)
        {

            percentageText.text = Mathf.FloorToInt(loadingBar.fillAmount * 100) + "%";



            // When the async load reaches 90%, show 100% and allow the scene to activate
            if (async.progress >= 0.9f && loadingBar.fillAmount >= 1f)
            {
                // Ensure the loading bar is filled to 100% after 5 seconds
                loadingBar.fillAmount = 1f;  // Fill the bar to 100%
                percentageText.text = "100%"; // Show 100%

                // Allow scene activation after the progress reaches 90%
                async.allowSceneActivation = true;
            }

            yield return null; // Wait for the next frame
        }


    }



    public void PanelActivity(bool MM = false, bool ModeSel = false, bool LvlSel = false, bool ExitPnl = false, bool SettingsPnl = false, bool Garage = false, bool IsLoading = false)
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

    void CheckUnlocked()
    {
        int numUnlockedLevels =  ValStorage.GetUnlockedLevels();
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
            GrphicsActivity(isHigh: true);
        }

        if (s == "Med")
        {
            GrphicsActivity(isMed: true);
        }

        if (s == "Low")
        {
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
        int totalRam = SystemInfo.systemMemorySize; // in MB

        // Simple checks based on CPU, RAM, and GPU (You can refine these thresholds)
        if (totalRam <= 3000 )
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

    #endregion
}

    






#endregion





