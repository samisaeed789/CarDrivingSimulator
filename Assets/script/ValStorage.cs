using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValStorage 
{
    public static string modeSel;
    public static int selLevel;
    public static int TrnsparVal;

    public static float timerforlane;

   public static int GetCoins() 
   {
      return  PlayerPrefs.GetInt("Coins");
   }


    public static void SetCar(string val)
    {
        PlayerPrefs.SetString("SelectedCar", val);
    }
    
    public static string GetCar()
    {

       return PlayerPrefs.GetString("SelectedCar");
    }

    public static void SetCoins(int coin)
    {

        PlayerPrefs.SetInt("Coins",coin);
    }

    public static void SetTransparency(int val) 
    {
       
        PlayerPrefs.SetInt("TransparentVal",val);
    }

    public static int GetTransparency()
    {
        if (!PlayerPrefs.HasKey("TransparentVal"))
        {
            PlayerPrefs.SetInt("TransparentVal", 5);
        }
        return  PlayerPrefs.GetInt("TransparentVal");
    }
    
    public static float GetSVolume()
    {
        if (!PlayerPrefs.HasKey("SoundVol"))
        {
            PlayerPrefs.SetFloat("SoundVol", 0.5f);
        }
        return  PlayerPrefs.GetFloat("SoundVol");
    }
    
    public static void SetSVolume(float val)
    {

        PlayerPrefs.SetFloat("SoundVol",val);
    }
    public static float GetMVolume()
    {

        if (!PlayerPrefs.HasKey("MusicVol"))
        {
            PlayerPrefs.SetFloat("MusicVol", 0.5f);
        }
        return  PlayerPrefs.GetFloat("MusicVol");
    }
    
    public static void SetMVolume(float val)
    {
         PlayerPrefs.SetFloat("MusicVol",val);
    }
    
    public static int GetControls()
    {
        return  PlayerPrefs.GetInt("Controls");
    }
   
    public static void SetControls(int val)
    {
        PlayerPrefs.SetInt("Controls", val);
    }
    public static int GetGQuality()
    {
        return  PlayerPrefs.GetInt("GQuality");
    }
   
    public static void SetGQuality(int val)
    {
        PlayerPrefs.SetInt("GQuality", val);
    }

    public static void SetUnlockedLevels(int val)
    {
        PlayerPrefs.SetInt("UnlockedLevels", val);
    }

    public static int GetUnlockedLevels()
    {
       return PlayerPrefs.GetInt("UnlockedLevels", 0);
    } 
    
   
 



}
