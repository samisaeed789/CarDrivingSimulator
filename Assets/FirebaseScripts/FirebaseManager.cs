using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
public class FirebaseManager
{
    public static void LevelPause(string Level)
    {
        Level = Level.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("level_pause_" + Level);
            Debug.Log("Analytics: LevelPauseAnalytics :" + Level);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in LevelPauseAnalytics :" + e.ToString());
        }
    }

    public static void LevelResume(string Level)
    {
        Level = Level.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("level_resume_" + Level);
            Debug.Log("Analytics: LevelResumeAnalytics :" + Level);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in LevelResumeAnalytics :" + e.ToString());
        }
    }


    public static void LevelRestart(string Level)
    {
        Level = Level.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("level_restart_" + Level);
            Debug.Log("Analytics: LevelRestartAnalytics :" + Level);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in LevelRestartAnalytics :" + e.ToString());
        }
    }

    public static void LevelHome(string Level)
    {
        Level = Level.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("level_home_" + Level);
            Debug.Log("Analytics: LevelHomeAnalytics" + Level);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in LevelHomeAnalytics" + e.ToString());
        }
    }

    public static void LevelComplete(string Level)
    {
        Level = Level.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("level_complete_" + Level);
            Debug.Log("Analytics: LevelCompleteAnalytics :" + Level);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in LevelCompleteAnalytics :" + e.ToString());
        }
    }

    public static void LevelFail(string Level)
    {
        Level = Level.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("leve_fail_" + Level);
            Debug.Log("Analytics: LevelFailAnalytics :" + Level);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in LevelFailAnalytics :" + e.ToString());
        }
    }

    public static void LevelStart(string Level)
    {
        Level = Level.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("level_start_" + Level);
            Debug.Log("Analytics: LevelStartAnalytics :" + Level);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in LevelStartAnalytics :" + e.ToString());
        }
    }


    public static void LevelSelect(string Level)
    {
        Level = Level.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("level_select_" + Level);
            Debug.Log("Analytics: LevelSelectAnalytics :" + Level);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in LevelSelectAnalytics :" + e.ToString());
        }
    }


    public static void LevelNext(string Level)
    {
        Level = Level.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("level_next_" + Level);
            Debug.Log("Analytics: LevelNextAnalytics :" + Level);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in LevelNextAnalytics :" + e.ToString());
        }
    }


    public static void SelectVehicle(string VehicleIndex)
    {
        VehicleIndex = VehicleIndex.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("select_vehicle_" + VehicleIndex);
            Debug.Log("Analytics: SelectVehicleAnalytics:" + VehicleIndex);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in SelectVehicleAnalytics:" + e.ToString());
        }
    }

    public static void SelectMode(string modeIndex)
    {
        modeIndex = modeIndex.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("select_mode_" + modeIndex);
            Debug.Log("Analytics: SelectModeAnalytics:" + modeIndex);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in SelectModeAnalytics:" + e.ToString());
        }
    }

    public static void ModeLevelStart(string modeIndex, string levelIndex)
    {
        modeIndex = modeIndex.Replace("..", "_");
        levelIndex = levelIndex.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("select_mode_" + modeIndex + "_level_start_" + levelIndex);
            Debug.Log("Analytics: SelectModeAnalytics:" + modeIndex + "SelectedLevelAnalytics:" + levelIndex);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in SelectMode&LevelAnalytics:" + e.ToString());
        }
    }

    public static void ModeLevelPause(string modeIndex, string levelIndex)
    {
        modeIndex = modeIndex.Replace("..", "_");
        levelIndex = levelIndex.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("select_mode_" + modeIndex + "_level_pause_" + levelIndex);
            Debug.Log("Analytics: SelectModeAnalytics:" + modeIndex + "LevelPauseAnalytics :" + levelIndex);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in ModeLevelPauseAnalytics :" + e.ToString());
        }
    }

    public static void ModeLevelComplete(string modeIndex, string levelIndex)
    {
        modeIndex = modeIndex.Replace("..", "_");
        levelIndex = levelIndex.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("select_mode_" + modeIndex + "_level_complete_" + levelIndex);
            Debug.Log("Analytics: SelectModeAnalytics:" + modeIndex + "LevelCompleteAnalytics :" + levelIndex);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in ModeLevelCompleteAnalytics :" + e.ToString());
        }
    }
    public static void ModeLevelFail(string modeIndex, string levelIndex)
    {
        modeIndex = modeIndex.Replace("..", "_");
        levelIndex = levelIndex.Replace("..", "_");

        try
        {
            FirebaseAnalytics.LogEvent("select_mode_" + modeIndex + "_leve_fail_" + levelIndex);
            Debug.Log("Analytics: SelectModeAnalytics:" + modeIndex + "LevelFailAnalytics :" + levelIndex);
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in ModeLevelFailAnalytics :" + e.ToString());
        }
    }

    public static void Splash()
    {
        try
        {
            FirebaseAnalytics.LogEvent("splash_scene");
            Debug.Log("Analytics: SplashSceneAnalytics:");
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in SplashSceneAnalytics:" + e.ToString());
        }
    }

    public static void MainMenu()
    {
        try
        {
            FirebaseAnalytics.LogEvent("main_menu");
            Debug.Log("Analytics: MainMenuAnalytics:");
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in MainMenuAnalytics:" + e.ToString());
        }
    }
    
    public static void Garage()
    {
        try
        {
            FirebaseAnalytics.LogEvent("garage");
            Debug.Log("Analytics: GarageAnalytics:");
        }
        catch (Exception e)
        {
            Debug.Log("Analytics: Error in GarageAnalytics:" + e.ToString());
        }
    }

}
