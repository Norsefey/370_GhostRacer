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

    void PlayerCounterUpdate()
    {//when a player joins/leaves, the player count is chnaged//and the names displayed is also changed
        playerCount = PhotonNetwork.PlayerList.Length;
        playerCountMax = PhotonNetwork.CurrentRoom.MaxPlayers;

        playerCount_Text.text = playerCount + " / " + playerCountMax;
        playerNamesText.text = "";
        for (int x = 0; x < playerCount; x++)
        {
            if (PhotonNetwork.PlayerList[x].NickName == "")
            {//if the player did not input a name, gives players a random name
                int playerId = Random.Range(0, 1000000);
                string randName = "Player: " + playerId.ToString();
                playerNamesText.text += randName;
                PhotonNetwork.PlayerList[x].NickName = randName;
            }
            else//if player did names themsleves, display the name they chose
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
        {//when room has more than one player, allow host to start game
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public void StartGame()
    {//host starts game
        startingGame = true;


        if(!PhotonNetwork.IsMasterClient)
        {//since scenes are synced only host needs to load scene
            return;
        }

        startButton.SetActive(false);
        PhotonNetwork.CurrentRoom.IsOpen = false;//closes room
        PhotonNetwork.LoadLevel(multiplayerGameSceneIndex);//loads game scene
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {//when a player enters room update the player count and names displayed
        PlayerCounterUpdate();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {//when a player leaves room update the player count and names displayed
        PlayerCounterUpdate();
    }

    public void CancelJoinGame()
    {//allows players to leave room
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(menuSceneIndex);
    }

    

}
