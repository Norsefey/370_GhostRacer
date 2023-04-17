using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcidPit : MonoBehaviour
{//when player fall in pit, delay player by 3 secs, then reset postion to respawn point

    [SerializeField]
    Transform _respawnPoint;

    IEnumerator CanMoveAgain(Collider other)
    {
        
        yield return new WaitForSeconds(1);
        other.transform.position = _respawnPoint.transform.position;
        Debug.Log("Released ");
        yield return new WaitForSeconds(1);
        other.GetComponent<PlayerMainMovementScript>()._canMove = true;
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Touching: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLayer Fell in ");
            
            other.GetComponent<PlayerMainMovementScript>()._canMove= false;
           


            StartCoroutine(CanMoveAgain(other));
            
            
        }
    }
}
