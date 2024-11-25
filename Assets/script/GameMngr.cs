using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameMngr : MonoBehaviour
{
    [Header("Panels")]
    public GameObject Complete;
    public GameObject Fail;
    public GameObject Pause;
    public GameObject Loading;

    [Header("GP UI")]
    public GameObject gearup;
    public GameObject geardown;
    public CanvasGroup ControllerBtns;



    IEnumerator Start()
    {
       
        yield return new WaitForSeconds(2); // fixed delay
        Loading.SetActive(false);
    }

    public void Enablegearactv(string s) 
    {
        if (s == "drive")
        {
            Gearactive(IsDrive: true);

        }
        if (s == "reverse") 
        {
            Gearactive(IsReverse:true);
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

    public void Next() 
    {

    }


}
