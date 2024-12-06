using UnityEngine;
using GoogleMobileAds.Ump.Api;
public class UmpManager : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(FuncStart), 0.25f);
    }
    void FuncStart()
    {
        ConsentRequestParameters request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false,
        };
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }
    private ConsentForm _consentForm;
    void OnConsentInfoUpdated(FormError error)
    {
        if (error != null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
            Debug.LogError(error);
            return;
        }
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            if (formError != null)
            {      
                Debug.LogError(error);
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
                return;
            }
            if (ConsentInformation.CanRequestAds())
            {
                if (AdsManager.instance)
                    AdsManager.instance.InitializeAdmob();            
            }
        });

    }   
}
