using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using System.ComponentModel;

public class PhotonPlayer : MonoBehaviour
{
    [SerializeField]
    bool singlePlayer = false;
    public bool finished = false;
    private PhotonView pv;
    private PlayerMainMovementScript movementScript;
    private GameObject cam;
    GameNetworkController gameController;
    ParticleSystem debuffEffect;
    public bool isMine;
    // Start is called before the first frame update
    void Start()
    {
        if (!singlePlayer)
        {
            gameController = FindObjectOfType<GameNetworkController>();
            pv = GetComponent<PhotonView>();
            movementScript = GetComponent<PlayerMainMovementScript>();
            cam = transform.GetChild(0).gameObject;

            if (pv.IsMine)
            {
                isMine = pv.IsMine;
                cam.gameObject.SetActive(true);
                movementScript.enabled = true;
                gameObject.name = PhotonNetwork.PlayerList[gameController.playerIndex].NickName;
            }
            else
            {
                isMine = pv.IsMine;

                gameObject.name = pv.Owner.NickName;

                gameController.playerOwnedObjects.Add(gameObject);

                gameController.Racers.Add(gameObject);
            }
        }

        debuffEffect = transform.GetChild(3).GetComponent<ParticleSystem>();
    }

    public void CallIRS()
    {
        ///InflictRandomStatus();
        pv.RPC("InflictRandomStatus", RpcTarget.All);
    }


    [PunRPC]
    void InflictRandomStatus()
    {
        
        //will inflict a random status effect on player temporarly
        int delta = Random.Range(0, 4);
        debuffEffect.Stop();
        debuffEffect.Play();
        Debug.Log("Status Inflicted" + delta);
        switch (delta)
        {
            case 0://drains all stamina and makes player exhausted

                movementScript.CurrentStamina = 0;
                movementScript.Exhausted = true;
                StartCoroutine(ResetPlayerEffects(0, delta));
                break;
            case 1://flips camera upside down
                movementScript._cameraZRotation = 180;
                StartCoroutine(ResetPlayerEffects(2, delta));
                break;
            case 2://Inverts controls
                movementScript._invertCam = -1;
                StartCoroutine(ResetPlayerEffects(2, delta));
                break;
            case 3://slows movement speed
                movementScript._slowDown = 2;//divides finalmovespeed by slowdown//in this case cuts speed in half
                StartCoroutine(ResetPlayerEffects(2, delta));
                break;
            default:
                break;
        }
    }

    IEnumerator ResetPlayerEffects(float effectDuration, int delta)
    {
        yield return new WaitForSeconds(effectDuration);
        debuffEffect.Stop();
        switch (delta)
        {
            case 1:
                movementScript._cameraZRotation = 0;
                break;
            case 2:
                movementScript._invertCam = 1;
                break;
            case 3:
                movementScript._slowDown = 1;
                break;
            default:
                break;
        }
    }

}
