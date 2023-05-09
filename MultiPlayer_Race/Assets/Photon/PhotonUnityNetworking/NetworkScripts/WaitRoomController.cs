using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class WaitRoomController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    GameObject startButton;
    
    [SerializeField]
    Text roomName_Text;
    [SerializeField]
    Text playerNamesText;
    [SerializeField]
    Text playerCount_Text;

    [SerializeField]
    int multiplayerGameSceneIndex;
    [SerializeField]
    int menuSceneIndex;
    int playerCount;
    int playerCountMax;

    [SerializeField]
    GameObject gameSettings;

    bool startingGame = false;

    PhotonView view;
    float finalSpeed;
    // Start is called before the first frame update
    void Start()
    {
        roomName_Text.text = PhotonNetwork.CurrentRoom.Name;
        PlayerCounterUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayerCounterUpdate()
    {
        playerCount = PhotonNetwork.PlayerList.Length;
        playerCountMax = PhotonNetwork.CurrentRoom.MaxPlayers;

        playerCount_Text.text = playerCount + " / " + playerCountMax;
        playerNamesText.text = "";
        for (int x = 0; x < playerCount; x++)
        {
            if (PhotonNetwork.PlayerList[x].NickName == "")
            {
                int playerId = Random.Range(0, 1000000);
                string randName = "Player: " + playerId.ToString();
                playerNamesText.text += randName;
                PhotonNetwork.PlayerList[x].NickName = randName;
            }
            else
                playerNamesText.text += PhotonNetwork.PlayerList[x].NickName + "\n";
        }

        //only owner can start game
        if (!PhotonNetwork.IsMasterClient)
        {
            gameSettings.SetActive(false);
            return;
        }

        gameSettings.SetActive(true);

        if (playerCount > 1)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public void StartGame()
    {
        startingGame = true;


        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(multiplayerGameSceneIndex);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerCounterUpdate();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerCounterUpdate();
    }

    public void CancelJoinGame()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(menuSceneIndex);
    }

    

}
