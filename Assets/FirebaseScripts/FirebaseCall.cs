using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseCall : MonoBehaviour
{
    int LevelIndex;

    public void LevelPause()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            FirebaseManager.LevelPause(LevelIndex.ToString());
    }

    public void LevelResume()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            FirebaseManager.LevelResume(LevelIndex.ToString());
    }

    public void LevelRestart()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            FirebaseManager.LevelRestart(LevelIndex.ToString());
    }

    public void LevelHome()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            FirebaseManager.LevelHome(LevelIndex.ToString());
    }

    public void LevelComplete()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            FirebaseManager.LevelComplete(LevelIndex.ToString());
    }

    public void LevelFail()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            FirebaseManager.LevelFail(LevelIndex.ToString());
    }

    public void LevelStart()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            FirebaseManager.LevelComplete(LevelIndex.ToString());
    }

    public void LevelSelect()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            FirebaseManager.LevelFail(LevelIndex.ToString());
    }

    public void LevelNext()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            FirebaseManager.LevelFail(LevelIndex.ToString());
    }

    public void SelectVehicle()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
            FirebaseManager.LevelFail(LevelIndex.ToString());
    }

}
