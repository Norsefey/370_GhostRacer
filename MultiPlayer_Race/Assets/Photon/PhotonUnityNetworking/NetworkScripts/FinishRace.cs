using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class FinishRace : MonoBehaviour
{

    GameNetworkController gameNetworkController;
    CanvasManager canvasMan;
    ValueCorrection comapreValues;
    public int counter = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameNetworkController = FindObjectOfType<GameNetworkController>();
        comapreValues = gameNetworkController.gameObject.GetComponent<ValueCorrection>();
        canvasMan = FindObjectOfType<CanvasManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        
        //when an NPC enters the trigger//get root and assign to finalpos list
        if (other.CompareTag("NPC"))
        {
            //since the child has the collider
            other.gameObject.GetComponent<NPC_Finished>().finished = true;
            StartCoroutine(NPCDeactivate(other));
            //gameNetworkController.AddFinalRacerPosition(other.gameObject);
            counter++;
            comapreValues.CompareAndFixValues(counter);
            canvasMan.DisplayeFinalPosition(counter, other.name);
        }
        else if (other.CompareTag("Player"))
        {
            
            counter++;
            comapreValues.CompareAndFixValues(counter);
            Debug.Log(other + " : "+counter.ToString());
            
            if (other.GetComponent<PhotonPlayer>().isMine)
            {
                other.gameObject.GetComponent<PlayerMainMovementScript>().CanMove = false;
                other.gameObject.GetComponent<PlayerMainMovementScript>().CanLook = false;
                other.gameObject.GetComponent<PlayerMainMovementScript>()._playerFinished = true;

                canvasMan.DisplayeFinalPosition(counter, "<color=green>" + "<b>" + other.name + "</color>" + "</b>");
            }
            else
            {
                other.gameObject.GetComponent<PhotonPlayer>().finished = true;
                canvasMan.DisplayeFinalPosition(counter, "<color=blue>" + "<b>" + other.name + "</color>" + "</b>");
            }
            //Time.timeScale = 2f;
        }

    }

    IEnumerator NPCDeactivate(Collider other)
    {//wait a bit,then deactivate the NPC
        yield return new WaitForSeconds(2);
        other.gameObject.GetComponent<NPCManager>()._active = false;
    }
}
