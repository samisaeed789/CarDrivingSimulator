using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class mngr : MonoBehaviour
{

    public GameObject[] CSs;
    public GameObject selection;
    public Image fade;
    // Start is called before the first frame update
    public void Select(int i) 
    {
        selection.SetActive(false);
       
        Color c = fade.color;
        c.a = 0;
        fade.color = c;
       


        foreach (GameObject g in CSs) 
        {
            g.SetActive(false);
        }
        CSs[i].gameObject.SetActive(true);
    }

    public void opesel()
    {
        selection.SetActive(true);
    }


    public void fadeoff()
    {
        //fade = GetComponent<Image>();

        
    }
}
