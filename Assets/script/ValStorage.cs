using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ValStorage 
{
    public static string modeSel;
    public static int selLevel;

   public static int GetCoins() 
   {
      return  PlayerPrefs.GetInt("Coins");
   }

    public static void SetCoins(int coin)
    {
        PlayerPrefs.SetInt("Coins",coin);
    }
}
