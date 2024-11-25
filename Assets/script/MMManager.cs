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


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        PlayerPrefs.SetInt("UnlockedLevels", 5);

        if (PlayerPrefs.GetInt("UnlockedLevels") == 0)
        {
            PlayerPrefs.SetInt("UnlockedLevels", 1);
        }
        CheckUnlocked();
    }

    private void Start()
    {
        //   if (MySoundManager.instance)
        // MySoundManager.instance.SetMainMenuMusic(true, 0.5f);

        if (GiveCoins)
            ValStorage.SetCoins(5000);

        SetCoins();
        Time.timeScale = 1f;

        Application.targetFrameRate = 120;


    }

    public void GoToModeSelection()
    {
       // if (MySoundManager.instance)
         //   MySoundManager.instance.PlayButtonClickSound(1f);

        StartCoroutine(LoadPanel("ModeSelection"));
    }




    public void GoToLevelSelection()
    {
     //   if (MySoundManager.instance)
          //  MySoundManager.instance.PlayButtonClickSound(1f);
        StartCoroutine(LoadPanel("LevelSelection"));
    }

    public void GoToMainMenu()
    {
        //if (MySoundManager.instance)
       //     MySoundManager.instance.PlayButtonClickSound(1f);
        StartCoroutine(LoadPanel("MainMenu"));
    }




    IEnumerator LoadPanel(string sceneName)
    {
        mainMenuPanel.SetActive(false);
        levelSelectionPanel.SetActive(false);
        modeSelectionPanel.SetActive(false);
        loadingScreenPanel.SetActive(true);

        yield return new WaitForSeconds(2f);
        if (sceneName == "MainMenu")
        {

        }
        if (sceneName == "LevelSelection")
        {
            levelSelectionPanel.SetActive(true);
        }

        if (sceneName == "ModeSelection")
        {
            modeSelectionPanel.SetActive(true);
        }

        loadingScreenPanel.SetActive(false);
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
    }

    void Disablehildren()
    {
        foreach (Transform child in CarsCont.transform)
        {
            // Check if the child game object is active/enabled
            if (child.gameObject.activeSelf)
            {
                // Disable the child game object
                child.gameObject.SetActive(false);
            }
        }
    }
    public void SelMode(string Mode) 
    {
        ValStorage.modeSel = Mode;
        BackBtn("LvlSel");
    }


    public void SelLevel(int i)
    {
        ValStorage.selLevel = i;
        BackBtn("Garage");
    }




    public void LoadNxtScene()
    {

        // if (MySoundManager.instance)
        //  MySoundManager.instance.PlayButtonClickSound(1f);
        CarsCont.SetActive(false);
        PlayerPrefs.SetString("SelectedCar", garage.GetCurrCarId());
        StartCoroutine(LoadAsyncScene("Gameplay"));
    }


    IEnumerator LoadAsyncScene(string sceneName)
    {
        PanelActivity(IsLoading: true); // Assuming this method hides/unhides panels


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



    public void PanelActivity(bool MM = false, bool ModeSel = false, bool LvlSel = false, bool ExitPnl = false,bool SettingsPnl=false,bool Garage=false,bool IsLoading=false)
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
        int numUnlockedLevels = PlayerPrefs.GetInt("UnlockedLevels", 0);
        // Loop through all the level buttons in your UI
        for (int i = 1; i <= LvlCards.Length; i++)
        {
            // Get a reference to the button
            Button levelButton = LvlCards[i-1];

            if (levelButton != null)
            {
                // If this level is unlocked, make the button interactable
                if (i <= numUnlockedLevels)
                {
                    levelButton.interactable = true;
                    levelButton.transform.GetChild(0).gameObject.SetActive(false);

                }
                else
                {
                    // If this level is locked, make the button not interactable
                    levelButton.interactable = false;
                    levelButton.transform.GetChild(0).gameObject.SetActive(true);
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
        SettingsPanel.SetActive(State);
    }

    //public void BackBtn()
    //{

    //    if (mainMenuPanel.activeSelf)
    //    {

    //        exitPanel.SetActive(true);
    //    }

    //    if (modeSelectionPanel.activeSelf)
    //    {
    //        modeSelectionPanel.SetActive(false);
    //        mainMenuPanel.SetActive(true);
    //    }

    //    if (levelSelectionPanel.activeSelf)
    //    {
    //        levelSelectionPanel.SetActive(false);
    //        modeSelectionPanel.SetActive(true);
    //    }
    //}








    #region Settings
    public void sttngstab(string S)
    {
        if (S == "SFX")
        {
            SettngsActivity(isSfx: true);
            //  SettngsBtnActivity(isSfx: true);
        }
        if (S == "Graphics")
        {
            SettngsActivity(isGraphic: true);
            //  SettngsBtnActivity(isGraphic: true);


        }
        if (S == "Control")
        {
            SettngsActivity(isControl: true);
            //   SettngsBtnActivity(isControl: true);
            //
        }
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
            ControlsActivity(isArrow:true);
        }

        if (s == "Tilt")
        {
            ControlsActivity(isTilt:true);
        }
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




    #endregion







}
