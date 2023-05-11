using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryBox : MonoBehaviour
{
    GameObject mesh;
   

    private void Start()
    {
        mesh = transform.GetChild(0).gameObject;
      
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            mesh.SetActive(false);
            if (other.GetComponent<PlayerMainMovementScript>().isActiveAndEnabled)
            {
                other.GetComponent<PlayerMainMovementScript>().canShoot = true;
            }
           
            StartCoroutine(Reactivate());

        }
        else if (other.CompareTag("NPC"))
        {
            mesh.SetActive(false);
            StartCoroutine(Reactivate());
        }
    }


    IEnumerator Reactivate()
    {
        //after some time reenabled gameobject

        yield return new WaitForSeconds(3);
        mesh.SetActive(true);
    }
}
