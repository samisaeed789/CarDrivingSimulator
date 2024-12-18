using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowChangeCol : MonoBehaviour
{
    [SerializeField]GameObject Visual;
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0f);
        Transform Parent=this.transform.parent;
        Visual = Parent.GetChild(0).gameObject;

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Visual != null) 
            {
                MeshRenderer meshRenderer = Visual.GetComponent<MeshRenderer>();
                meshRenderer.material.color = Color.yellow;

            }

            // Change the material color to red
        }
    }
}
