using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Network_Lobby : MonoBehaviourPunCallbacks
{
    [SerializeField]
    Text loadingText;
    [SerializeField]
    GameObject joinRandomButton;
    [SerializeField]
    GameObject createButton;
    [SerializeField]
    GameObject cancelButton;

    [SerializeField]
    GameObject joinSpecificButton;
    [SerializeField]
    InputField specificRoomInput;
    [SerializeField]
    InputField playerNameInput;
    [SerializeField]
    int roomSize;

    string roomName;
    string playerName;

    public override void OnConnectedToMaster()
    {//once connected display room menu
        Debug.Log("Connected");
        PhotonNetwork.AutomaticallySyncScene = true;


        loadingText.enabled = false;
        playerNameInput.gameObject.SetActive(true);
        
    }

    public void UpdatePlayerName()
    {//sets player's name, used in game scene for racer position
        PhotonNetwork.NickName = playerNameInput.text;
        PlayerPrefs.SetString("NickName", playerNameInput.text);
        //room menu hidden until player sets a name
        LoadMenuButtons();
    }

    private void LoadMenuButtons()
    {//displays the room options
        joinRandomButton.SetActive(true);
        createButton.SetActive(true);
        joinSpecificButton.SetActive(true);
        specificRoomInput.gameObject.SetActive(true);
    }

    public void JoinRandomOpenRoom()
    {//connects to a random open room
        joinRandomButton.SetActive(false);
        createButton.SetActive(false);
        joinSpecificButton.SetActive(false);
        specificRoomInput.gameObject.SetActive(false);

        cancelButton.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    public void JoinSpecificRoom()
    {//joins a room with room number inputed
        joinRandomButton.SetActive(false);
        createButton.SetActive(false);
        joinSpecificButton.SetActive(false);
        specificRoomInput.gameObject.SetActive(false);

        cancelButton.SetActive(true);
        PhotonNetwork.JoinRoom("Room: " + specificRoomInput.text);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {//when joining a room, if no room is avaible create a room
        
        CreateRoom();
    }

    public void CreateRoom()
    {//creates a room//gives room an Random ID number
        int roomID = Random.Range(0, 10000);
        RoomOptions roomOptions = new RoomOptions() {IsVisible = true, IsOpen = true, MaxPlayers = (byte)roomSize };
        roomName = "Room" + roomID.ToString();
        PhotonNetwork.CreateRoom("Room: " + roomID, roomOptions);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {//if room creation fails//try again
        int randomID = Random.Range(0, 10000);
        CreateRoom();
    }

    public void CancelJoin()
    {//leave//cancle join//returns to main screen
        joinRandomButton.SetActive(true);
        createButton.SetActive(true);
        joinSpecificButton.SetActive(true);
        specificRoomInput.gameObject.SetActive(true);

        cancelButton.SetActive(false);

        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }
}
