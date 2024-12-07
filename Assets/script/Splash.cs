using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour
{

    [SerializeField]GameObject loadingPnl;
    [SerializeField] GameObject gameobjbtn;
    [SerializeField]Image loadingBar;
    [SerializeField]Text prcnttxt;
    [SerializeField]Animator sphere;
    [SerializeField]Animator MeterStart;
    // Start is called before the first frame update

    MySoundManager soundmgr;

    private void Start()
    {
        if (MySoundManager.instance)
            soundmgr = MySoundManager.instance;


        if (soundmgr != null)
            soundmgr.CarUnlock();

    }
    public void EngineStart()
    {

        MeterStart.enabled = true;
        soundmgr.PlayRunningCar();
        gameobjbtn.SetActive(false);

    }

    void PlayNextScene()
    {
        soundmgr.StopRevv5Sound();
        soundmgr.StopRunningCar();
        loadingPnl.SetActive(true);
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
                prcnttxt.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
            else
            {
                // Once the timer reaches 5 seconds, start loading the scene
                // Ensure the progress bar stays at 100% before activation
                loadingBar.fillAmount = 1f;
                prcnttxt.text = "100%";

                Debug.Log("allow");
                // Allow the scene to activate
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
        sphere.enabled = false;
        yield return new WaitForSeconds(0.1f);
        asyncLoad.allowSceneActivation = true;
    }


    void EngineSound()
    {
        if (soundmgr != null)
            soundmgr.PlayEngineSound();
    }

    void BeepSound()
    {
        if (soundmgr != null)
            soundmgr.PlayBeepSound();
    }

    void Revv()
    {
        if (soundmgr != null)
            soundmgr.PlayRevvSound();
    }

    void Revv1()
    {
        if (soundmgr != null)
            soundmgr.PlayRevv1Sound();
    }
    void Revv5()
    {
        if (soundmgr != null)
            soundmgr.PlayRevv5Sound();
    }
}
