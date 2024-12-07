using UnityEngine;
using UnityStandardAssets.Utility;

public class CarLandingEffect : MonoBehaviour
{
  


    AutoMoveAndRotate scr;


    private void Start()
    {
        scr=GetComponent<AutoMoveAndRotate>();
    }

    private void OnDisable()
    {
        scr.enabled = false;
    }


}
