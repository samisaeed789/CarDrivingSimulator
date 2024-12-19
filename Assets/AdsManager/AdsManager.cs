using UnityEngine;
using GoogleMobileAds.Api;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AdsManager : MonoBehaviour
{
    public static AdsManager instance;
    public UnityEvent OnWathcVideo;
    private BannerView bannerViewTop, bannerViewBottom, bannerViewTopLeft, bannerViewTopRight;
    private BannerView bannerViewBottomLeft, bannerViewBottomRight, bannerViewCenter;
    private BannerView bannerViewRectangleTop, bannerViewRectangleBottom, bannerViewRectangleTopLeft, bannerViewRectangleTopRight;
    private BannerView bannerViewRectangleBottomLeft, bannerViewRectangleBottomRight, bannerViewRectangleCenter;
    private BannerView bannerViewFullAdaptiveCentre, bannerViewFullAdaptiveTop, bannerViewFullAdaptiveBottom;
    private BannerView bannerViewCustomWidthAdaptiveCentre, bannerViewCustomWidthAdaptiveTop;
    private BannerView bannerViewCustomWidthAdaptiveTopLeft, bannerViewCustomWidthAdaptiveTopRight, bannerViewCustomWidthAdaptiveBottom, bannerViewCustomWidthAdaptiveBottomLeft, bannerViewCustomWidthAdaptiveBottomRight;
    private BannerView bannerViewSmartTop, bannerViewSmartBottom;

    private bool bannerViewTopLoaded, bannerViewBottomLoaded, bannerViewTopLeftLoaded, bannerViewTopRightLoaded;
    private bool bannerViewBottomLeftLoaded, bannerViewBottomRightLoaded, bannerViewCenterLoaded;
    private bool bannerViewRectangleTopLoaded, bannerViewRectangleBottomLoaded, bannerViewRectangleTopLeftLoaded, bannerViewRectangleTopRightLoaded;
    private bool bannerViewRectangleBottomLeftLoaded, bannerViewRectangleBottomRightLoaded, bannerViewRectangleCenterLoaded;
    private bool bannerViewFullAdaptiveCentreLoaded, bannerViewFullAdaptiveTopLoaded, bannerViewFullAdaptiveBottomLoaded;
    private bool bannerViewCustomWidthAdaptiveCentreLoaded, bannerViewCustomWidthAdaptiveTopLoaded;
    private bool bannerViewCustomWidthAdaptiveTopLeftLoaded, bannerViewCustomWidthAdaptiveTopRightLoaded, bannerViewCustomWidthAdaptiveBottomLoaded, bannerViewCustomWidthAdaptiveBottomLeftLoaded, bannerViewCustomWidthAdaptiveBottomRightLoaded;
    private bool bannerViewSmartTopLoaded, bannerViewSmartBottomLoaded;

    private bool bannerViewTopShowing, bannerViewBottomShowing, bannerViewTopLeftShowing, bannerViewTopRightShowing;
    private bool bannerViewBottomLeftShowing, bannerViewBottomRightShowing, bannerViewCenterShowing;
    private bool bannerViewRectangleTopShowing, bannerViewRectangleBottomShowing, bannerViewRectangleTopLeftShowing, bannerViewRectangleTopRightShowing;
    private bool bannerViewRectangleBottomLeftShowing, bannerViewRectangleBottomRightShowing, bannerViewRectangleCenterShowing;
    private bool bannerViewFullAdaptiveCentreShowing, bannerViewFullAdaptiveTopShowing, bannerViewFullAdaptiveBottomShowing;
    private bool bannerViewCustomWidthAdaptiveCentreShowing, bannerViewCustomWidthAdaptiveTopShowing;
    private bool bannerViewCustomWidthAdaptiveTopLeftShowing, bannerViewCustomWidthAdaptiveTopRightShowing, bannerViewCustomWidthAdaptiveBottomShowing, bannerViewCustomWidthAdaptiveBottomLeftShowing, bannerViewCustomWidthAdaptiveBottomRightShowing;
    private bool bannerViewSmartTopShowing, bannerViewSmartBottomShowing;

    private InterstitialAd interstitialAd0, interstitialAd1, interstitialAd2, interstitialAd3, interstitialAd4;
    public RewardedInterstitialAd rewardedInterstitialAD;
    private RewardedAd videoAD;
    private AppOpenAd thisAppOpenAd;
    [SerializeField]
    ScreenOrientation screenOrientation;

    [SerializeField]
    bool showAppOpenAd;
    public bool StopShowingAdsOnLowEndDevices;
    [SerializeField]
    string[] admobBannerIds;
    [SerializeField]
    string[] admobInterstitialIds;
    [SerializeField]
    string admobRewardedInterstitialID, admobRewardedVideoID, appOpenAdID;
    [SerializeField]
    bool ShowTestAds;
    int minRamSize;


    bool isAdAlreadyShowing;
    float deviceRam;
    void Awake()
    {
        if (instance != this && instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        minRamSize = 2048;
        deviceRam = SystemInfo.systemMemorySize;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeAdmob()
    {
        if (StopShowingAdsOnLowEndDevices) {
            if (deviceRam < minRamSize) {
                //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
                return;
            }
        }
        checkForAdIds();
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            OnInitializationComp();
        });
    }   
    void OnInitializationComp()
    {
        loadAdmobRewardedInterstitial();
        Invoke("LoadVideoAD", 5f);
        tryToLoadAllInterstitials();
        //Invoke("loadAdmobRewardedInterstitial", 6f);
        if (showAppOpenAd)
            LoadAppOpenAd();
       // UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
    }
    bool wentToBackground;  
    
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            wentToBackground = true;
            return;
        }
            CancelInvoke(nameof(DelayInAppOpen));
            Invoke(nameof(DelayInAppOpen), 0.15f);
    }
    void DelayInAppOpen()
    {
        if (!wentToBackground)
            return;
        if  (showAppOpenAd && !isAdAlreadyShowing)
            ShowAdmobAppOpenAd();
    }

    void checkForAdIds()
    {
        if (admobBannerIds.Length > 0)
        {
            if (admobBannerIds[0] == "")
                admobBannerIds[0] = "ca-app-pub-3940256099942544/6300978111";
        }
        for (int i = 0; i < admobInterstitialIds.Length; i++)
        {
            if (admobInterstitialIds[i] == "" || admobInterstitialIds[i] == null)
                admobInterstitialIds[i] = "ca-app-pub-3940256099942544/1033173712";
        }

        if (admobRewardedInterstitialID == "" || admobRewardedInterstitialID == null)
            admobRewardedInterstitialID = "ca-app-pub-3940256099942544/5354046379";
        if (admobRewardedVideoID == "" || admobRewardedVideoID == null)
            admobRewardedVideoID = "ca-app-pub-3940256099942544/5224354917";
        if (appOpenAdID == "" || appOpenAdID == null)
            appOpenAdID = "ca-app-pub-3940256099942544/3419835294";
    }

    #region Banners

    int bannerInc = 0;

    AdRequest CreateAdRequest()
    {
        var adRequest = new AdRequest();
        return adRequest;
    }

    BannerView ShowAdmobBannerCollapsible(BannerView view, AdSize size, AdPosition position)
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return null;

        if (view != null)
        {
            view.Destroy();
            view = null;
        }

        string banId = "ca-app-pub-3940256099942544/6300978111";
        if (!ShowTestAds)
        {
            if (admobBannerIds.Length > bannerInc)
            {
                if (admobBannerIds[bannerInc] != "")
                    banId = admobBannerIds[bannerInc];
            }

            bannerInc++;
            if (bannerInc >= admobBannerIds.Length)
                bannerInc = 0;
        }
        view = new BannerView(banId, size, position);
        AdRequest adRequest = CreateAdRequest();
        if(position==AdPosition.Bottom)
            adRequest.Extras.Add("collapsible", "bottom");
        else if(position == AdPosition.Top)
            adRequest.Extras.Add("collapsible", "top");
        view.LoadAd(adRequest);
        ListenToAdEvents(view);
        return view;
    }
    BannerView showAdmobBanner(BannerView view, AdSize size, AdPosition position)
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return null;

        if (view != null)
        {
            view.Destroy();
            view = null;
        }

        string banId = "ca-app-pub-3940256099942544/6300978111";
        if (!ShowTestAds)
        {
            if (admobBannerIds.Length > bannerInc)
            {
                if (admobBannerIds[bannerInc] != "")
                    banId = admobBannerIds[bannerInc];
            }

            bannerInc++;
            if (bannerInc >= admobBannerIds.Length)
                bannerInc = 0;
        }
        view = new BannerView(banId, size, position);
        view.LoadAd(CreateAdRequest());
        ListenToAdEvents(view);
        return view;
    }

    void changeBannerBools(BannerView _view, bool state)
    {
        if (_view == bannerViewTop)
        {
            bannerViewTopLoaded = state;
            bannerViewTopShowing = state;
        }
        else if (_view == bannerViewTopLeft)
        {
            bannerViewTopLeftLoaded = state;
            bannerViewTopLeftShowing = state;
        }
        else if (_view == bannerViewTopRight)
        {
            bannerViewTopRightLoaded = state;
            bannerViewTopRightShowing = state;
        }
        else if (_view == bannerViewCenter)
        {
            bannerViewCenterLoaded = state;
            bannerViewCenterShowing = state;
        }
        else if (_view == bannerViewBottom)
        {
            bannerViewBottomLoaded = state;
            bannerViewBottomShowing = state;
        }
        else if (_view == bannerViewBottomLeft)
        {
            bannerViewBottomLeftLoaded = state;
            bannerViewBottomLeftShowing = state;
        }
        else if (_view == bannerViewBottomRight)
        {
            bannerViewBottomRightLoaded = state;
            bannerViewBottomRightShowing = state;
        }
        else if (_view == bannerViewRectangleTop)
        {
            bannerViewRectangleTopLoaded = state;
            bannerViewRectangleTopShowing = state;
        }
        else if (_view == bannerViewRectangleTopLeft)
        {
            bannerViewRectangleTopLeftLoaded = state;
            bannerViewRectangleTopLeftShowing = state;
        }
        else if (_view == bannerViewRectangleTopRight)
        {
            bannerViewRectangleTopRightLoaded = state;
            bannerViewRectangleTopRightShowing = state;
        }
        else if (_view == bannerViewRectangleCenter)
        {
            bannerViewRectangleCenterLoaded = state;
            bannerViewRectangleCenterShowing = state;
        }
        else if (_view == bannerViewRectangleBottom)
        {
            bannerViewRectangleBottomLoaded = state;
            bannerViewRectangleBottomShowing = state;
        }
        else if (_view == bannerViewRectangleBottomLeft)
        {
            bannerViewRectangleBottomLeftLoaded = state;
            bannerViewRectangleBottomLeftShowing = state;
        }
        else if (_view == bannerViewRectangleBottomRight)
        {
            bannerViewRectangleBottomRightLoaded = state;
            bannerViewRectangleBottomRightShowing = state;
        }
        else if (_view == bannerViewFullAdaptiveTop)
        {
            bannerViewFullAdaptiveTopLoaded = state;
            bannerViewFullAdaptiveTopShowing = state;
        }
        else if (_view == bannerViewFullAdaptiveCentre)
        {
            bannerViewFullAdaptiveCentreLoaded = state;
            bannerViewFullAdaptiveCentreShowing = state;
        }
        else if (_view == bannerViewFullAdaptiveBottom)
        {
            bannerViewFullAdaptiveBottomLoaded = state;
            bannerViewFullAdaptiveBottomShowing = state;
        }
        if (_view == bannerViewCustomWidthAdaptiveTop)
        {
            bannerViewCustomWidthAdaptiveTopLoaded = state;
            bannerViewCustomWidthAdaptiveTopShowing = state;
        }
        else if (_view == bannerViewCustomWidthAdaptiveTopLeft)
        {
            bannerViewCustomWidthAdaptiveTopLeftLoaded = state;
            bannerViewCustomWidthAdaptiveTopLeftShowing = state;
        }
        else if (_view == bannerViewCustomWidthAdaptiveTopRight)
        {
            bannerViewCustomWidthAdaptiveTopRightLoaded = state;
            bannerViewCustomWidthAdaptiveTopRightShowing = state;
        }
        else if (_view == bannerViewCustomWidthAdaptiveCentre)
        {
            bannerViewCustomWidthAdaptiveCentreLoaded = state;
            bannerViewCustomWidthAdaptiveCentreShowing = state;
        }
        else if (_view == bannerViewCustomWidthAdaptiveBottom)
        {
            bannerViewCustomWidthAdaptiveBottomLoaded = state;
            bannerViewCustomWidthAdaptiveBottomShowing = state;
        }
        else if (_view == bannerViewCustomWidthAdaptiveBottomLeft)
        {
            bannerViewCustomWidthAdaptiveBottomLeftLoaded = state;
            bannerViewCustomWidthAdaptiveBottomLeftShowing = state;
        }
        else if (_view == bannerViewCustomWidthAdaptiveBottomRight)
        {
            bannerViewCustomWidthAdaptiveBottomRightLoaded = state;
            bannerViewCustomWidthAdaptiveBottomRightShowing = state;
        }
        else if (_view == bannerViewSmartTop)
        {
            bannerViewSmartTopLoaded = state;
            bannerViewSmartTopShowing = state;
        }
        else if(_view==bannerViewSmartBottom)
        {
            bannerViewSmartBottomLoaded = true;
            bannerViewSmartBottomShowing = true;
        }
    }

    private void ListenToAdEvents(BannerView _view)
    {
        _view.OnBannerAdLoaded += () =>
        {
            changeBannerBools(_view, true);
        };

        _view.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            changeBannerBools(_view, false);
        };
    }

    void hideAdMobBanner(BannerView view)
    {

        if (view != null)
            view.Hide();
    }
    public void showAdMobBannerTop()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewTopLoaded)
        {
            bannerViewTop = showAdmobBanner(bannerViewTop, AdSize.Banner, AdPosition.Top);
        }
        else
        {
            if (bannerViewTop != null)
            {
                bannerViewTop.Show();
                bannerViewTopShowing = true;
            }
        }
    }
    public void showAdMobBannerBottom()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;
        if (!bannerViewBottomLoaded)
        {
            bannerViewBottom = showAdmobBanner(bannerViewBottom, AdSize.Banner, AdPosition.Bottom);
        }
        else
        {
            if (bannerViewBottom != null)
            {
                bannerViewBottom.Show();
                bannerViewBottomShowing = true;
            }
        }
    }
    public void showAdMobBannerTopLeft()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewTopLeftLoaded)
        {
            bannerViewTopLeft = showAdmobBanner(bannerViewTopLeft, AdSize.Banner, AdPosition.TopLeft);
        }
        else
        {
            if (bannerViewTopLeft != null)
            {
                bannerViewTopLeft.Show();
                bannerViewTopLeftShowing = true;
            }
        }

    }
    public void showAdMobBannerTopRight()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewTopRightLoaded)
        {
            bannerViewTopRight = showAdmobBanner(bannerViewTopRight, AdSize.Banner, AdPosition.TopRight);
        }
        else
        {
            if (bannerViewTopRight != null)
            {
                bannerViewTopRight.Show();
                bannerViewTopRightShowing = true;
            }
        }


    }
    public void showAdMobBannerBottomLeft()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewBottomLeftLoaded)
        {
            bannerViewBottomLeft = showAdmobBanner(bannerViewBottomLeft, AdSize.Banner, AdPosition.BottomLeft);
        }
        else
        {
            if (bannerViewBottomLeft != null)
            {
                bannerViewBottomLeft.Show();
                bannerViewBottomLeftShowing = true;
            }
        }

    }
    public void showAdMobBannerBottomRight()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewBottomRightLoaded)
        {
            bannerViewBottomRight = showAdmobBanner(bannerViewBottomRight, AdSize.Banner, AdPosition.BottomRight);
        }
        else
        {
            if (bannerViewBottomRight != null)
            {
                bannerViewBottomRight.Show();
                bannerViewBottomRightShowing = true;
            }
        }

    }
    public void showAdMobBannerCenter()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;
        if (!bannerViewCenterLoaded)
        {
            bannerViewCenter = showAdmobBanner(bannerViewCenter, AdSize.Banner, AdPosition.Center);

        }
        else
        {
            if (bannerViewCenter != null)
            {
                bannerViewCenter.Show();
                bannerViewCenterShowing = true;
            }
        }


    }
    public void showAdMobRectangleBannerTop()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        //if (bannerViewTop != null)
        // bannerViewTop.Destroy();
        if (!bannerViewRectangleTopLoaded)
        {
            bannerViewRectangleTop = showAdmobBanner(bannerViewRectangleTop, AdSize.MediumRectangle, AdPosition.Top);

        }
        else
        {
            if (bannerViewRectangleTop != null)
            {
                bannerViewRectangleTop.Show();
                bannerViewRectangleTopShowing = true;
            }
        }

    }
    public void showAdMobRectangleBannerBottom()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewRectangleBottomLoaded)
        {
            bannerViewRectangleBottom = showAdmobBanner(bannerViewRectangleBottom, AdSize.MediumRectangle, AdPosition.Bottom);
        }
        else
        {
            if (bannerViewRectangleBottom != null)
            {
                bannerViewRectangleBottom.Show();
                bannerViewRectangleBottomShowing = true;
            }
        }


    }
    public void showAdMobRectangleBannerTopLeft()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewRectangleTopLeftLoaded)
        {
            bannerViewRectangleTopLeft = showAdmobBanner(bannerViewRectangleTopLeft, AdSize.MediumRectangle, AdPosition.TopLeft);
        }
        else
        {
            if (bannerViewRectangleTopLeft != null)
            {
                bannerViewRectangleTopLeft.Show();
                bannerViewRectangleTopLeftShowing = true;
            }
        }


    }
    public void showAdMobRectangleBannerTopRight()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewRectangleTopRightLoaded)
        {
            bannerViewRectangleTopRight = showAdmobBanner(bannerViewRectangleTopRight, AdSize.MediumRectangle, AdPosition.TopRight);
        }
        else
        {
            if (bannerViewRectangleTopRight != null)
            {
                bannerViewRectangleTopRight.Show();
                bannerViewRectangleTopRightShowing = true;
            }
        }


    }
    public void showAdMobRectangleBannerBottomLeft()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewRectangleBottomLeftLoaded)
        {
            bannerViewRectangleBottomLeft = showAdmobBanner(bannerViewRectangleBottomLeft, AdSize.MediumRectangle, AdPosition.BottomLeft);
        }
        else
        {
            if (bannerViewRectangleBottomLeft != null)
            {
                bannerViewRectangleBottomLeft.Show();
                bannerViewRectangleBottomLeftShowing = true;
            }
        }


    }
    public void showAdMobRectangleBannerBottomRight()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewRectangleBottomRightLoaded)
        {
            bannerViewRectangleBottomRight = showAdmobBanner(bannerViewRectangleBottomRight, AdSize.MediumRectangle, AdPosition.BottomRight);
        }
        else
        {
            if (bannerViewRectangleBottomRight != null)
            {
                bannerViewRectangleBottomRight.Show();
                bannerViewRectangleBottomRightShowing = true;
            }
        }


    }
    public void showAdMobRectangleBannerCenter()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewRectangleCenterLoaded)
        {
            bannerViewRectangleCenter = showAdmobBanner(bannerViewRectangleCenter, AdSize.MediumRectangle, AdPosition.Center);
        }
        else
        {
            if (bannerViewRectangleCenter != null)
            {
                bannerViewRectangleCenter.Show();
                bannerViewRectangleCenterShowing = true;
            }
        }
    }

    public void showAdmobAdpativeBannerTop()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewFullAdaptiveTopLoaded)
        {
            bannerViewFullAdaptiveTop = showAdmobBanner(bannerViewFullAdaptiveTop, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth), AdPosition.Top);
        }
        else
        {
            if (bannerViewFullAdaptiveTop != null)
            {
                bannerViewFullAdaptiveTop.Show();
                bannerViewFullAdaptiveTopShowing = true;
            }
        }

    }

    public void showAdmobAdpativeBannerBottom()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;


        if (!bannerViewFullAdaptiveBottomLoaded)
        {
            bannerViewFullAdaptiveBottom = showAdmobBanner(bannerViewFullAdaptiveBottom, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth), AdPosition.Bottom);
        }
        else
        {
            if (bannerViewFullAdaptiveBottom != null)
            {
                bannerViewFullAdaptiveBottom.Show();
                bannerViewFullAdaptiveBottomShowing = true;
            }
        }


    }

    public void showAdmobAdpativeBannerCenter()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewFullAdaptiveCentreLoaded)
        {
            bannerViewFullAdaptiveBottom = showAdmobBanner(bannerViewFullAdaptiveCentre, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth), AdPosition.Center);

        }
        else
        {
            if (bannerViewFullAdaptiveCentre != null)
            {
                bannerViewFullAdaptiveCentre.Show();
                bannerViewFullAdaptiveCentreShowing = true;
            }
        }

    }
    public void showAdmobAdpativeBannerTopCustomWidth()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewCustomWidthAdaptiveTopLoaded)
        {
            bannerViewCustomWidthAdaptiveTop = showAdmobBanner(bannerViewCustomWidthAdaptiveTop, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(500), AdPosition.Top);

        }
        else
        {
            if (bannerViewCustomWidthAdaptiveTop != null)
            {
                bannerViewCustomWidthAdaptiveTop.Show();
                bannerViewCustomWidthAdaptiveTopShowing = true;
            }
        }

    }
    public void showAdmobAdpativeBannerTopLeftCustomWidth()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewCustomWidthAdaptiveTopLeftLoaded)
        {
            bannerViewCustomWidthAdaptiveTopLeft = showAdmobBanner(bannerViewCustomWidthAdaptiveTopLeft, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(500), AdPosition.TopLeft);


        }
        else
        {
            if (bannerViewCustomWidthAdaptiveTopLeft != null)
            {
                bannerViewCustomWidthAdaptiveTopLeft.Show();
                bannerViewCustomWidthAdaptiveTopLeftShowing = true;
            }
        }

    }
    public void showAdmobAdpativeBannerTopRightCustomWidth()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewCustomWidthAdaptiveTopRightLoaded)
        {
            bannerViewCustomWidthAdaptiveTopRight = showAdmobBanner(bannerViewCustomWidthAdaptiveTopRight, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(500), AdPosition.TopRight);
        }
        else
        {
            if (bannerViewCustomWidthAdaptiveTopRight != null)
            {
                bannerViewCustomWidthAdaptiveTopRight.Show();
                bannerViewCustomWidthAdaptiveTopRightShowing = true;
            }
        }

    }
    public void showAdmobAdpativeBannerBottomLeftCustomWidth()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;


        if (!bannerViewCustomWidthAdaptiveBottomLeftLoaded)
        {
            bannerViewCustomWidthAdaptiveBottomLeft = showAdmobBanner(bannerViewCustomWidthAdaptiveBottomLeft, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(500), AdPosition.BottomLeft);

        }
        else
        {
            if (bannerViewCustomWidthAdaptiveBottomLeft != null)
            {
                bannerViewCustomWidthAdaptiveBottomLeft.Show();
                bannerViewCustomWidthAdaptiveBottomLeftShowing = true;
            }
        }

    }
    public void showAdmobAdpativeBannerBottomRightCustomWidth()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewCustomWidthAdaptiveBottomRightLoaded)
        {
            bannerViewCustomWidthAdaptiveBottomRight = showAdmobBanner(bannerViewCustomWidthAdaptiveBottomRight, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(500), AdPosition.BottomRight);
        }
        else
        {
            if (bannerViewCustomWidthAdaptiveBottomRight != null)
            {
                bannerViewCustomWidthAdaptiveBottomRight.Show();
                bannerViewCustomWidthAdaptiveBottomRightShowing = true;
            }
        }

    }
    public void showAdmobAdpativeBannerBottomCustomWidth()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewCustomWidthAdaptiveBottomLoaded)
        {
            bannerViewCustomWidthAdaptiveBottom = showAdmobBanner(bannerViewCustomWidthAdaptiveBottom, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(500), AdPosition.Bottom);
        }
        else
        {
            if (bannerViewCustomWidthAdaptiveBottom != null)
            {
                bannerViewCustomWidthAdaptiveBottom.Show();
                bannerViewCustomWidthAdaptiveBottomShowing = true;
            }
        }
    }

    public void showAdmobSmartBannerTop()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewSmartTopLoaded)
        {
            bannerViewSmartTop = showAdmobBanner(bannerViewSmartTop, AdSize.SmartBanner, AdPosition.Top);
        }
        else
        {
            if (bannerViewSmartTop != null)
            {
                bannerViewSmartTop.Show();
                bannerViewSmartTopShowing = true;
            }
        }
    }
    public void showAdmobSmartBannerBottom()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!bannerViewSmartBottomLoaded)
        {
            bannerViewSmartBottom = showAdmobBanner(bannerViewSmartBottom, AdSize.SmartBanner, AdPosition.Bottom);
        }
        else
        {
            if (bannerViewSmartBottom != null)
            {
                bannerViewSmartBottom.Show();
                bannerViewSmartBottomShowing = true;
            }
        }
    }
    public void hideAllAdmobBanners()
    {
        hideAdmobTopBanner();
        hideAdmobBottomBanner();
        hideAdmobBottomLeftBanner();
        hideAdmobBottomRightBanner();
        hideAdmobCenterBanner();
        hideAdmobTopLeftBanner();
        hideAdmobTopRightBanner();
    }
    public void hideAdmobTopBanner()
    {
        hideAdMobBanner(bannerViewTop);
        bannerViewTopShowing = false;
        hideAdMobBanner(bannerViewRectangleTop);
        bannerViewRectangleTopShowing = false;
        hideAdMobBanner(bannerViewFullAdaptiveTop);
        bannerViewFullAdaptiveTopShowing = false;
        hideAdMobBanner(bannerViewCustomWidthAdaptiveTop);
        bannerViewCustomWidthAdaptiveTopShowing = false;
        hideAdMobBanner(bannerViewSmartTop);
        bannerViewSmartTopShowing = false;

    }
    public void hideAdmobBottomBanner()
    {
        hideAdMobBanner(bannerViewBottom);
        bannerViewBottomShowing = false;
        hideAdMobBanner(bannerViewRectangleBottom);
        bannerViewRectangleBottomShowing = false;
        hideAdMobBanner(bannerViewFullAdaptiveBottom);
        bannerViewFullAdaptiveBottomShowing = false;
        hideAdMobBanner(bannerViewCustomWidthAdaptiveBottom);
        bannerViewCustomWidthAdaptiveBottomShowing = false;
        hideAdMobBanner(bannerViewSmartBottom);
        bannerViewSmartBottomShowing = false;
    }
    public void hideAdmobTopLeftBanner()
    {
        hideAdMobBanner(bannerViewTopLeft);
        bannerViewTopLeftShowing = false;
        hideAdMobBanner(bannerViewRectangleTopLeft);
        bannerViewRectangleTopLeftShowing = false;
        hideAdMobBanner(bannerViewCustomWidthAdaptiveTopLeft);
        bannerViewCustomWidthAdaptiveTopLeftShowing = false;
    }
    public void hideAdmobTopRightBanner()
    {
        hideAdMobBanner(bannerViewTopRight);
        bannerViewTopRightShowing = false;
        hideAdMobBanner(bannerViewRectangleTopRight);
        bannerViewRectangleTopRightShowing = false;
        hideAdMobBanner(bannerViewCustomWidthAdaptiveTopRight);
        bannerViewCustomWidthAdaptiveTopRightShowing = false;
    }
    public void hideAdmobBottomLeftBanner()
    {
        hideAdMobBanner(bannerViewBottomLeft);
        bannerViewBottomLeftShowing = false;
        hideAdMobBanner(bannerViewRectangleBottomLeft);
        bannerViewRectangleBottomLeftShowing = false;
        hideAdMobBanner(bannerViewCustomWidthAdaptiveBottomLeft);
        bannerViewCustomWidthAdaptiveBottomLeftShowing = false;
    }
    public void hideAdmobBottomRightBanner()
    {
        hideAdMobBanner(bannerViewBottomRight);
        bannerViewBottomRightShowing = false;
        hideAdMobBanner(bannerViewRectangleBottomRight);
        bannerViewRectangleBottomRightShowing = false;
        hideAdMobBanner(bannerViewCustomWidthAdaptiveBottomRight);
        bannerViewCustomWidthAdaptiveBottomRightShowing = false;
    }
    public void hideAdmobCenterBanner()
    {
        hideAdMobBanner(bannerViewCenter);
        bannerViewCenterShowing = false;
        hideAdMobBanner(bannerViewRectangleCenter);
        bannerViewRectangleCenterShowing = false;
        hideAdMobBanner(bannerViewFullAdaptiveCentre);
        bannerViewFullAdaptiveCentreShowing = false;
        hideAdMobBanner(bannerViewCustomWidthAdaptiveCentre);
        bannerViewCustomWidthAdaptiveCentreShowing = false;
    }



    #endregion

    #region Interstitials

    void tryToLoadAllInterstitials()
    {
        Invoke("loadAdMobInterstital0", 0.5f);
        Invoke("loadAdMobInterstital1", 1f);
        Invoke("loadAdMobInterstital2", 1.5f);
        Invoke("loadAdMobInterstital3", 2f);
        Invoke("loadAdMobInterstital4", 2.5f);
    }

    void loadInterstitialAdCustom0(string _adUnit)
    {
        InterstitialAd.Load(_adUnit, CreateAdRequest(),
         (InterstitialAd ad, LoadAdError error) =>
         {
             if (error != null || ad == null)
             {
                 return;
             }
             interstitialAd0 = ad;
             RegisterInterEvents0(ad);
         });
    }
    void loadInterstitialAdCustom1(string _adUnit)
    {
        InterstitialAd.Load(_adUnit, CreateAdRequest(),
         (InterstitialAd ad, LoadAdError error) =>
         {
             if (error != null || ad == null)
             {
                 return;
             }
             interstitialAd1 = ad;
             RegisterInterEvents1(ad);
         });
    }
    void loadInterstitialAdCustom2(string _adUnit)
    {
        InterstitialAd.Load(_adUnit, CreateAdRequest(),
         (InterstitialAd ad, LoadAdError error) =>
         {
             if (error != null || ad == null)
             {
                 return;
             }
             interstitialAd2 = ad;
             RegisterInterEvents2(ad);
         });
    }
    void loadInterstitialAdCustom3(string _adUnit)
    {
        InterstitialAd.Load(_adUnit, CreateAdRequest(),
         (InterstitialAd ad, LoadAdError error) =>
         {
             if (error != null || ad == null)
             {
                 return;
             }
             interstitialAd3 = ad;
             RegisterInterEvents3(ad);
         });
    }
    void loadInterstitialAdCustom4(string _adUnit)
    {
        InterstitialAd.Load(_adUnit, CreateAdRequest(),
         (InterstitialAd ad, LoadAdError error) =>
         {
             if (error != null || ad == null)
             {
                 return;
             }
             interstitialAd4 = ad;
             RegisterInterEvents4(ad);
         });
    }
    void loadAdMobInterstital0()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (interstitialAd0 != null)
        {
            interstitialAd0.Destroy();
            interstitialAd0 = null;
        }

        if (ShowTestAds)
        {
            loadInterstitialAdCustom0("ca-app-pub-3940256099942544/1033173712");
        }
        else
        {
            if (interstitialAd0 != null)
            {
                if (!interstitialAd0.CanShowAd())
                {
                    if (admobInterstitialIds.Length > 0)
                    {
                        loadInterstitialAdCustom0(admobInterstitialIds[0]);
                    }
                }
            }
            else
            {
                if (admobInterstitialIds.Length > 0)
                {
                    loadInterstitialAdCustom0(admobInterstitialIds[0]);
                }
            }
        }
    }



    void loadAdMobInterstital1()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (interstitialAd1 != null)
        {
            interstitialAd1.Destroy();
            interstitialAd1 = null;
        }

        if (ShowTestAds)
        {
            loadInterstitialAdCustom1("ca-app-pub-3940256099942544/1033173712");
        }
        else
        {
            if (interstitialAd1 != null)
            {
                if (!interstitialAd1.CanShowAd())
                {
                    if (admobInterstitialIds.Length > 1)
                    {
                        loadInterstitialAdCustom1(admobInterstitialIds[1]);
                    }
                }
            }
            else
            {
                if (admobInterstitialIds.Length > 1)
                {
                    loadInterstitialAdCustom1(admobInterstitialIds[1]);
                }
            }
        }
    }
    void loadAdMobInterstital2()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (interstitialAd2 != null)
        {
            interstitialAd2.Destroy();
            interstitialAd2 = null;
        }

        if (ShowTestAds)
        {
            loadInterstitialAdCustom2("ca-app-pub-3940256099942544/1033173712");
        }
        else
        {
            if (interstitialAd2 != null)
            {
                if (!interstitialAd2.CanShowAd())
                {
                    if (admobInterstitialIds.Length > 2)
                    {
                        loadInterstitialAdCustom2(admobInterstitialIds[2]);
                    }
                }
            }
            else
            {
                if (admobInterstitialIds.Length > 2)
                {
                    loadInterstitialAdCustom2(admobInterstitialIds[2]);
                }
            }
        }
    }
    void loadAdMobInterstital3()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (interstitialAd3 != null)
        {
            interstitialAd3.Destroy();
            interstitialAd3 = null;
        }

        if (ShowTestAds)
        {
            loadInterstitialAdCustom3("ca-app-pub-3940256099942544/1033173712");
        }
        else
        {
            if (interstitialAd3 != null)
            {
                if (!interstitialAd3.CanShowAd())
                {
                    if (admobInterstitialIds.Length > 3)
                    {
                        loadInterstitialAdCustom3(admobInterstitialIds[3]);
                    }
                }
            }
            else
            {
                if (admobInterstitialIds.Length > 3)
                {
                    loadInterstitialAdCustom3(admobInterstitialIds[3]);
                }
            }
        }
    }
    void loadAdMobInterstital4()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (interstitialAd4 != null)
        {
            interstitialAd4.Destroy();
            interstitialAd4 = null;
        }

        if (ShowTestAds)
        {
            loadInterstitialAdCustom4("ca-app-pub-3940256099942544/1033173712");
        }
        else
        {
            if (interstitialAd4 != null)
            {
                if (!interstitialAd4.CanShowAd())
                {
                    if (admobInterstitialIds.Length > 4)
                    {
                        loadInterstitialAdCustom4(admobInterstitialIds[4]);
                    }
                }
            }
            else
            {
                if (admobInterstitialIds.Length > 4)
                {
                    loadInterstitialAdCustom4(admobInterstitialIds[4]);
                }
            }
        }
    }
    private void RegisterInterEvents0(InterstitialAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {

            isAdAlreadyShowing = true;
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            wentToBackground = false;
            loadAdMobInterstital0();
            isAdAlreadyShowing = false;
        };
    }
    private void RegisterInterEvents1(InterstitialAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {

            isAdAlreadyShowing = true;
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            wentToBackground = false;
            loadAdMobInterstital1();
            isAdAlreadyShowing = false;
        };
    }
    private void RegisterInterEvents2(InterstitialAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {
           
            isAdAlreadyShowing = true;
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            wentToBackground = false;
            loadAdMobInterstital2();
            isAdAlreadyShowing = false;
        };
    }
    private void RegisterInterEvents3(InterstitialAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {
           
            isAdAlreadyShowing = true;
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            wentToBackground = false;
            loadAdMobInterstital3();
            isAdAlreadyShowing = false;
        };
    }
    private void RegisterInterEvents4(InterstitialAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {
         
            isAdAlreadyShowing = true;
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            wentToBackground = false;
            loadAdMobInterstital4();
            isAdAlreadyShowing = false;
        };
    }

    public void showAdmobInterstitial()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;
        if (showAdmobInter0())
            return;
        if (showAdmobInter1())
            return;
        if (showAdmobInter2())
            return;
        if (showAdmobInter3())
            return;
        if (showAdmobInter4())
            return;
        tryToLoadAllInterstitials();
    }

    bool showAdmobInter0()
    {
        if (interstitialAd0 != null)
        {
            if (interstitialAd0.CanShowAd())
            {
                interstitialAd0.Show();
                tryToLoadAllInterstitials();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool showAdmobInter1()
    {
        if (interstitialAd1 != null)
        {
            if (interstitialAd1.CanShowAd())
            {
                interstitialAd1.Show();
                tryToLoadAllInterstitials();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    bool showAdmobInter2()
    {
        if (interstitialAd2 != null)
        {
            if (interstitialAd2.CanShowAd())
            {
                interstitialAd2.Show();
                tryToLoadAllInterstitials();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    bool showAdmobInter3()
    {
        if (interstitialAd3 != null)
        {
            if (interstitialAd3.CanShowAd())
            {
                interstitialAd3.Show();
                tryToLoadAllInterstitials();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    bool showAdmobInter4()
    {
        if (interstitialAd4 != null)
        {
            if (interstitialAd4.CanShowAd())
            {
                interstitialAd4.Show();
                tryToLoadAllInterstitials();
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region RewardedInterstitials
   public void loadAdmobRewardedInterstitial()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (rewardedInterstitialAD != null)
        {
            rewardedInterstitialAD.Destroy();
            rewardedInterstitialAD = null;
        }

        RewardedInterstitialAd.Load(ShowTestAds ? "ca-app-pub-5770035071370331/8533846856" : admobRewardedInterstitialID, CreateAdRequest(),
          (RewardedInterstitialAd ad, LoadAdError error) =>
          {
              if (error != null || ad == null)
              {
                  return;
              }
              rewardedInterstitialAD = ad;
              RegisterRewardedInterEventHandlers(ad);
          });

    }

    void RegisterRewardedInterEventHandlers(RewardedInterstitialAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {
           
            isAdAlreadyShowing = true;
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            wentToBackground = false;
            isAdAlreadyShowing = false;
            loadAdmobRewardedInterstitial();
        };

    }

    public bool checkIfAdmobRewardedInterstitialIsLoaded()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return false;

        if (rewardedInterstitialAD != null && rewardedInterstitialAD.CanShowAd())
        {
            return true;
        }
        return false;
    }


    public void ShowAdmobRewardedInterstitial()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;
        if (rewardedInterstitialAD != null)
        {
            if (rewardedInterstitialAD.CanShowAd())
            {
                rewardedInterstitialAD.Show((Reward reward) =>
                {
                    OnWathcVideo.Invoke();
                    // TODO: Reward the user.            
                });
            }
            else
            {
                loadAdmobRewardedInterstitial();
            }
        }
        else
        {
            loadAdmobRewardedInterstitial();
        }
    }

    #endregion

    #region AppOpenAd

    bool isShowingAppOpenAd;


    private bool IsAdAvailable
    {
        get
        {
            return thisAppOpenAd != null && thisAppOpenAd.CanShowAd();
        }
    }

    public void LoadAppOpenAd()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;
        if (thisAppOpenAd != null)
        {
            thisAppOpenAd.Destroy();
            thisAppOpenAd = null;
        }
        AppOpenAd.Load(ShowTestAds ? "ca-app-pub-3940256099942544/9257395921" : appOpenAdID, CreateAdRequest(),
       (AppOpenAd ad, LoadAdError error) =>
       {
           if (error != null || ad == null)
           {
               Debug.LogError(error);
               return;
           }

           thisAppOpenAd = ad;
           RegisterAppOpenEventHandlers(ad);
       });
    }

    private void RegisterAppOpenEventHandlers(AppOpenAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {
            
            isShowingAppOpenAd = true;
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            wentToBackground = false;
            thisAppOpenAd = null;
            isShowingAppOpenAd = false;

            LoadAppOpenAd();
            ShowBannersForAppOpenAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            thisAppOpenAd = null;
            isShowingAppOpenAd = false;
            ShowBannersForAppOpenAd();
        };
    }

    public void ShowAdmobAppOpenAd()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (!IsAdAvailable)
            LoadAppOpenAd();

        if (!IsAdAvailable || isShowingAppOpenAd)
            return;

        thisAppOpenAd.Show();
        HideBannersForAppOpenAd();
    }

    void HideBannersForAppOpenAd()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;
        if (bannerViewTop != null && bannerViewTopShowing)
            bannerViewTop.Hide();
        if (bannerViewTopLeft != null && bannerViewTopLeftShowing)
            bannerViewTopLeft.Hide();
        if (bannerViewTopRight != null && bannerViewTopRightShowing)
            bannerViewTopRight.Hide();
        if (bannerViewCenter != null && bannerViewCenterShowing)
            bannerViewCenter.Hide();
        if (bannerViewBottom != null && bannerViewBottomShowing)
            bannerViewBottom.Hide();
        if (bannerViewBottomLeft != null && bannerViewBottomLeftShowing)
            bannerViewBottomLeft.Hide();
        if (bannerViewBottomRight != null && bannerViewBottomRightShowing)
            bannerViewBottomRight.Hide();
        if (bannerViewRectangleTop != null && bannerViewRectangleTopShowing)
            bannerViewRectangleTop.Hide();
        if (bannerViewRectangleTopLeft != null && bannerViewRectangleTopLeftShowing)
            bannerViewRectangleTopLeft.Hide();
        if (bannerViewRectangleTopRight != null && bannerViewRectangleTopRightShowing)
            bannerViewRectangleTopRight.Hide();
        if (bannerViewRectangleCenter != null && bannerViewRectangleCenterShowing)
            bannerViewRectangleCenter.Hide();
        if (bannerViewRectangleBottom != null && bannerViewRectangleBottomShowing)
            bannerViewRectangleBottom.Hide();
        if (bannerViewRectangleBottomLeft != null && bannerViewRectangleBottomLeftShowing)
            bannerViewRectangleBottomLeft.Hide();
        if (bannerViewRectangleBottomRight != null && bannerViewRectangleBottomRightShowing)
            bannerViewRectangleBottomRight.Hide();
        if (bannerViewFullAdaptiveTop != null && bannerViewFullAdaptiveTopShowing)
            bannerViewFullAdaptiveTop.Hide();
        if (bannerViewFullAdaptiveBottom != null && bannerViewFullAdaptiveBottomShowing)
            bannerViewFullAdaptiveBottom.Hide();
        if (bannerViewFullAdaptiveCentre != null && bannerViewFullAdaptiveCentreShowing)
            bannerViewFullAdaptiveCentre.Hide();
        if (bannerViewCustomWidthAdaptiveTop != null && bannerViewCustomWidthAdaptiveTopShowing)
            bannerViewCustomWidthAdaptiveTop.Hide();
        if (bannerViewCustomWidthAdaptiveTopLeft != null && bannerViewCustomWidthAdaptiveTopLeftShowing)
            bannerViewCustomWidthAdaptiveTopLeft.Hide();
        if (bannerViewCustomWidthAdaptiveTopRight != null && bannerViewCustomWidthAdaptiveTopRightShowing)
            bannerViewCustomWidthAdaptiveTopRight.Hide();
        if (bannerViewCustomWidthAdaptiveCentre != null && bannerViewCustomWidthAdaptiveCentreShowing)
            bannerViewCustomWidthAdaptiveCentre.Hide();
        if (bannerViewCustomWidthAdaptiveBottom != null && bannerViewCustomWidthAdaptiveBottomShowing)
            bannerViewCustomWidthAdaptiveBottom.Hide();
        if (bannerViewCustomWidthAdaptiveBottomLeft != null && bannerViewCustomWidthAdaptiveBottomLeftShowing)
            bannerViewCustomWidthAdaptiveBottomLeft.Hide();
        if (bannerViewCustomWidthAdaptiveBottomRight != null && bannerViewCustomWidthAdaptiveBottomRightShowing)
            bannerViewCustomWidthAdaptiveBottomRight.Hide();
        if (bannerViewSmartBottom != null && bannerViewSmartBottomShowing)
            bannerViewSmartBottom.Hide();
        if (bannerViewSmartTop != null && bannerViewSmartTopShowing)
            bannerViewSmartTop.Hide();
    }

    void ShowBannersForAppOpenAd()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;
        if (bannerViewTop != null && bannerViewTopShowing)
            bannerViewTop.Show();
        if (bannerViewTopLeft != null && bannerViewTopLeftShowing)
            bannerViewTopLeft.Show();
        if (bannerViewTopRight != null && bannerViewTopRightShowing)
            bannerViewTopRight.Show();
        if (bannerViewCenter != null && bannerViewCenterShowing)
            bannerViewCenter.Show();
        if (bannerViewBottom != null && bannerViewBottomShowing)
            bannerViewBottom.Show();
        if (bannerViewBottomLeft != null && bannerViewBottomLeftShowing)
            bannerViewBottomLeft.Show();
        if (bannerViewBottomRight != null && bannerViewBottomRightShowing)
            bannerViewBottomRight.Show();
        if (bannerViewRectangleTop != null && bannerViewRectangleTopShowing)
            bannerViewRectangleTop.Show();
        if (bannerViewRectangleTopLeft != null && bannerViewRectangleTopLeftShowing)
            bannerViewRectangleTopLeft.Show();
        if (bannerViewRectangleTopRight != null && bannerViewRectangleTopRightShowing)
            bannerViewRectangleTopRight.Show();
        if (bannerViewRectangleCenter != null && bannerViewRectangleCenterShowing)
            bannerViewRectangleCenter.Show();
        if (bannerViewRectangleBottom != null && bannerViewRectangleBottomShowing)
            bannerViewRectangleBottom.Show();
        if (bannerViewRectangleBottomLeft != null && bannerViewRectangleBottomLeftShowing)
            bannerViewRectangleBottomLeft.Show();
        if (bannerViewRectangleBottomRight != null && bannerViewRectangleBottomRightShowing)
            bannerViewRectangleBottomRight.Show();
        if (bannerViewFullAdaptiveTop != null && bannerViewFullAdaptiveTopShowing)
            bannerViewFullAdaptiveTop.Show();
        if (bannerViewFullAdaptiveBottom != null && bannerViewFullAdaptiveBottomShowing)
            bannerViewFullAdaptiveBottom.Show();
        if (bannerViewFullAdaptiveCentre != null && bannerViewFullAdaptiveCentreShowing)
            bannerViewFullAdaptiveCentre.Show();
        if (bannerViewCustomWidthAdaptiveTop != null && bannerViewCustomWidthAdaptiveTopShowing)
            bannerViewCustomWidthAdaptiveTop.Show();
        if (bannerViewCustomWidthAdaptiveTopLeft != null && bannerViewCustomWidthAdaptiveTopLeftShowing)
            bannerViewCustomWidthAdaptiveTopLeft.Show();
        if (bannerViewCustomWidthAdaptiveTopRight != null && bannerViewCustomWidthAdaptiveTopRightShowing)
            bannerViewCustomWidthAdaptiveTopRight.Show();
        if (bannerViewCustomWidthAdaptiveCentre != null && bannerViewCustomWidthAdaptiveCentreShowing)
            bannerViewCustomWidthAdaptiveCentre.Show();
        if (bannerViewCustomWidthAdaptiveBottom != null && bannerViewCustomWidthAdaptiveBottomShowing)
            bannerViewCustomWidthAdaptiveBottom.Show();
        if (bannerViewCustomWidthAdaptiveBottomLeft != null && bannerViewCustomWidthAdaptiveBottomLeftShowing)
            bannerViewCustomWidthAdaptiveBottomLeft.Show();
        if (bannerViewCustomWidthAdaptiveBottomRight != null && bannerViewCustomWidthAdaptiveBottomRightShowing)
            bannerViewCustomWidthAdaptiveBottomRight.Show();
        if (bannerViewSmartBottom != null && bannerViewSmartBottomShowing)
            bannerViewSmartBottom.Show();
        if (bannerViewSmartTop != null && bannerViewSmartTopShowing)
            bannerViewSmartTop.Show();
    }
    #endregion

    #region AdmobRewardedVideo
    void LoadVideoAD()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;


        if (videoAD != null)
        {
            videoAD.Destroy();
            videoAD = null;
        }

        RewardedAd.Load(ShowTestAds ? "ca-app-pub-3940256099942544/5224354917" : admobRewardedVideoID, CreateAdRequest(),
          (RewardedAd ad, LoadAdError error) =>
          {
              if (error != null || ad == null)
              {
                  return;
              }
              videoAD = ad;
              RegisterRewardedVideoEventHandlers(ad);
          });


    }
    private void RegisterRewardedVideoEventHandlers(RewardedAd ad)
    {
        ad.OnAdImpressionRecorded += () =>
        {
            isAdAlreadyShowing = true;
        };
        ad.OnAdFullScreenContentClosed += () =>
        {
            wentToBackground = false;
            isAdAlreadyShowing = false;
            LoadVideoAD();
        };
    }
    public bool CheckIfAdmobRewardVideoRewardInterIsLoaded()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return false;

        if (videoAD != null)
        {
            if (videoAD.CanShowAd())
                return true;
            else if (checkIfAdmobRewardedInterstitialIsLoaded())
                return true;
            return false;
        }
        else if (checkIfAdmobRewardedInterstitialIsLoaded())
        {
            return true;
        }
        return false;
    }

    public void ShowAdmobRewardVideoRewardInter()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (videoAD != null)
        {
            if (videoAD.CanShowAd())
                ShowAdmobRewardedVideoAd();
            else if (checkIfAdmobRewardedInterstitialIsLoaded())
            {
                ShowAdmobRewardedInterstitial();
                LoadVideoAD();
            }
            else
            {
                LoadVideoAD();
                Invoke("loadAdmobRewardedInterstitial", 1f);
            }

        }
        else if (checkIfAdmobRewardedInterstitialIsLoaded())
        {
            ShowAdmobRewardedInterstitial();
            LoadVideoAD();
        }
        else
        {
            LoadVideoAD();
            Invoke("loadAdmobRewardedInterstitial", 1f);
        }
    }

    public void ShowAdmobRewardedVideoAd()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return;

        if (videoAD != null)
        {
            if (videoAD.CanShowAd())
            {

                videoAD.Show((Reward reward) =>
                {
                    // TODO: Reward the user.
                });

            }
            else
            {
                LoadVideoAD();
            }
        }
        else
        {
            LoadVideoAD();
        }
    }

    public bool CheckIfAdmobRewardedVideoIsReady()
    {
        if (StopShowingAdsOnLowEndDevices && deviceRam < minRamSize)
            return false;

        if (videoAD != null)
        {
            if (videoAD.CanShowAd())
            {
                return true;
            }
            else
            {
                LoadVideoAD();
                return false;
            }

        }
        return false;
    }
    #endregion


   
    public void DelaygrantCoins()
    {
        Invoke(nameof(GrantCoins), 0.2f);
    }

    public void GrantCoins()
    {
        int alreadycoins = ValStorage.GetCoins();
        ValStorage.SetCoins(alreadycoins + 300);

        if (SceneManager.GetActiveScene().name == "MM")
        {
            MMManager.Instance.SetCoins();
        }


    }
}
