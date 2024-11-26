using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIAnimatorCore;
using UnityEngine;
using UnityEngine.UI;
using WaypointsFree;


public class GameMngr : MonoBehaviour
{

    MySoundManager soundmgr;

    [Header("Panels")]
    public GameObject Complete;
    public GameObject Fail;
    public GameObject Pause;
    public GameObject Loading;

    [Header("GP UI")]
    public GameObject gearup;
    public GameObject geardown;
    public CanvasGroup ControllerBtns;
    public GameObject Ignition;
    public Image[] UIgp;

    [Header("Camera")]
    public Camera Cam;


    [Header("Player")]
    public RCC_CarControllerV3 Car;


    [Header("Data")]
    public WaypointsTraveler[] TrafficCars;

        






    [Header("Canvas")]
    [SerializeField] RCC_Demo Controls;

    // [Header("Settings")]





    IEnumerator Start()
    {
        soundmgr = MySoundManager.instance;


        SetButtonTransparency(ValStorage.GetTransparency());
        Controls.SetMobileController(ValStorage.GetControls());
        ControllerBtns.alpha = 0f;
        yield return new WaitForSeconds(2); // fixed delay
        Loading.SetActive(false);
    }

   






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





    #region loading/settinglvl

    public void OnLevelStatsLoadedHandler(LevelData levelStats)
    {

       
        //for (int i = 0; i < levelStats.WPs.Length; i++)
        //{

        //    TrafficCars[i].Waypoints = levelStats.WPs[i];
        //    TrafficCars[i].gameObject.SetActive(true);
        //}



    }

    #endregion












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

    
    public void EngineRun() 
    {
        if (soundmgr)
            soundmgr.PlayEngineSound();

        ShakeCamera();

        Ignition.SetActive(false);
        Car.StartEngine();
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
