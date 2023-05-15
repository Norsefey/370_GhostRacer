using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryBox : MonoBehaviour
{
    GameObject mesh;
    Collider myCollider;

    private void Start()
    {
        mesh = transform.GetChild(0).gameObject;
        myCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            mesh.SetActive(false);
            myCollider.enabled = false;

            if (other.GetComponent<PlayerMainMovementScript>().isActiveAndEnabled)
            {
                other.GetComponent<PlayerMainMovementScript>()._shotCounter++;
            }
           
            StartCoroutine(Reactivate());

        }
        else if (other.CompareTag("NPC"))
        {
            mesh.SetActive(false);
            myCollider.enabled = false;

            StartCoroutine(Reactivate());

        }
    }


    IEnumerator Reactivate()
    {
        //after some time reenabled gameobject

        yield return new WaitForSeconds(3);
        myCollider.enabled = true;
        mesh.SetActive(true);
    }
}
