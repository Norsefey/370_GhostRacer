using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameNetworkController : MonoBehaviourPunCallbacks
{

    [Header("Player")]
    //for player handling
    [SerializeField]
    List<Transform> spawnPoints = new List<Transform>();//player spawn points
    
   public List<GameObject> playerOwnedObjects = new List<GameObject>();//active players
    
    public int playerIndex;
    GameObject playerObj;

    [Space(5)]
    [Header("Track")]
    //for track genaration
    [SerializeField]
    GameObject[] trackParts;
    [SerializeField]
    Transform startingCP;
    
    private float trackLength = 10;
    int preIndex = 0;
    int leftCounter;
    int rightCounter;
    public List<GameObject> ActiveTrackParts = new List<GameObject>();

    

    [Space(5)]
    [Header("NPC")]
    //for NPC Handling
    NPC_TargetPoints npc_TargetPoints;
    public List<GameObject> npcs = new List<GameObject>();


    //Race Handling
    [Space(5)]
    [Header("Race")]
    public List<GameObject> Racers = new List<GameObject>();//all racers including players//for finding race position
    public List<GameObject> finalRacerPos = new List<GameObject>();//final Position of all racers//add to as racers cross finish line
    bool _raceStarted = false;
    public bool RaceStarted { get { return _raceStarted; } }

    bool _raceFinished = false;
    private float _timer = 10;
    public float Timer { get { return _timer; } }

    PhotonView view;

    private void Awake()
    {
        trackLength = WorldStats._finalTrackLength;
        view = GetComponent<PhotonView>();

          npc_TargetPoints = GetComponent<NPC_TargetPoints>();
        if (PhotonNetwork.IsMasterClient)
            CreateTrack();


        //playerOwnedObjects.Clear();
        BeforeRacePrep();
        CreatePlayer();
    }
    private void Start()
    {
        
        

    }

    private void Update()
    {

        if (!_raceFinished)
        {
            if (!_raceStarted && _timer > 0)
                _timer -= 2 * Time.deltaTime;

            if (!_raceStarted && _timer < 0)
            {
                _raceStarted = true;
                StartRace();
            }
            
        }


    }

    void StartRace()
    {
    
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        else
        {
            for (int n = 0; n < npcs.Count; n++)
            {
                npcs[n].gameObject.GetComponent<NPCManager>().enabled = true;
                npcs[n].GetComponent<NPCManager>()._active = true;
            }
        }

    }


    public void EndRace()
    {
        Debug.Log("ENding Race");
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        else
        {
            for (int n = 0; n < npcs.Count; n++)
            {
                //npcs[n].gameObject.GetComponent<NPCManager>().enabled = true;
                npcs[n].GetComponent<NPCManager>()._active = false;
            }
        }
    }

    void BeforeRacePrep()
    {
        Renderer _ghostRenderer;
        Color[] ghostColors =
        {
        Color.red,
        Color.yellow,
        Color.white,
        Color.magenta,
        Color.green,
        Color.blue,
        Color.black,
        Color.gray

         };
        string[] NPCNamesArray =
    {//about 45 names//must end with in//its a neccesity
        "Jin",
        "Fin",
        "Grin",
        "Lin",
        "Bin",
        "Sin",
        "Tin",
        "Pin",
        "Yin",
        "Win",
        "Kin",
        "Din",
        "Cin",
        "Rin",
        "Justin",
        "Bidin",
        "Putin",
        "Potin",
        "Tousin",
        "Lousin",
        "Mousin",
        "Carsin",
        "Vinsin",
        "Ricklin",
        "Starin",
        "Racin",
        "Laycin",
        "Pracin",
        "Xin",
        "Zin",
        "Inuin",
        "Unpin",
        "Pokin",
        "Fistin",
        "Kristin",
        "Joustin",
        "Krillin",
        "Grillin",
        "Pillin",
        "Goin",
        "Dunkin",
        "Punkin",
        "Slunkin",
        "Trickin",
        "Frikin",
        "Franklin",
        "Jankin",
        "Editin",
        "Edisin",
        "Dilin"
    };

        ShuffleColors(ghostColors);
        ShuffleNames(NPCNamesArray);
        for (int n = 0; n < npcs.Count; n++)
        {
            npcs[n].gameObject.name = NPCNamesArray[n];
            _ghostRenderer = npcs[n].transform.GetChild(0).transform.GetChild(1).GetComponent<Renderer>();//assign a color to the NPC
            _ghostRenderer.material.color = ghostColors[n];
            
            Racers.Add(npcs[n]);//add npcs to active racer list

        }
    }

    private void CreatePlayer()
    {//spawns in player
        
        for (int x = 0; x < PhotonNetwork.PlayerList.Length; x++)
        {
            if (PhotonNetwork.PlayerList[x].NickName == PhotonNetwork.NickName)
            {
                
                playerIndex = x;
                Debug.Log(playerIndex);
            }

        }

        var playerPawn = (PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerPrefab"), spawnPoints[playerIndex].position, spawnPoints[playerIndex].rotation));
        playerObj = playerPawn.gameObject;
        playerOwnedObjects.Add(playerPawn);
     
        Racers.Add(playerPawn);
    }


    void CreateTrack()
    {
        ActiveTrackParts.Clear();

        //spawn in a striaght at start
        ActiveTrackParts.Add(PhotonNetwork.InstantiateRoomObject(Path.Combine("TrackParts", trackParts[0].name), startingCP.position, startingCP.rotation));
        AssignTargetPoints(ActiveTrackParts[0].transform);

        for (int x = 0; x < trackLength; x++)
        {
            //random track selection
            int trackIndex = Random.Range(0, trackParts.Length);
            //the spawn point for the new track part//gotten from previous track part
            Transform conectionPoint = ActiveTrackParts[x].gameObject.transform.GetChild(0);

            if (x >= trackLength - 1)
            {//when no more tracks to spawn//spawn finish line
                Debug.Log("Finish it");
                SpawnFinishLine(conectionPoint);
                
            }
            else if (!CheckCollisionWithTrack(conectionPoint, conectionPoint.forward))
            {
                trackIndex = CurveChecker(trackIndex);

                var track = PhotonNetwork.InstantiateRoomObject(Path.Combine("TrackParts", trackParts[trackIndex].name), conectionPoint.position, conectionPoint.rotation);
                ActiveTrackParts.Add(track);
                AssignTargetPoints(track.transform);

            }
            else
            {
                Debug.Log("No Room");
                Destroy(ActiveTrackParts[x]);
                conectionPoint = ActiveTrackParts[x - 1].gameObject.transform.GetChild(0);
                SpawnFinishLine(conectionPoint);
                break;
            }

        }


    }

    int CurveChecker(int trackIndex)
    {
        if ((trackIndex == 1 || trackIndex == 2))
        {
            
            switch (trackIndex)
            {
                case 1:
                    if(rightCounter < 1)
                    {
                        rightCounter++;
                        leftCounter = 0;
                        return 1;//allow right curve to spawn
                    }
                    else
                    {
                        leftCounter++;
                        rightCounter = 0;
                        return 2;//spawn left curve instead//prevents U Turns
                    }
                    
                case 2: 
                    if(leftCounter < 1)
                    {
                        leftCounter++;
                        rightCounter = 0;
                        return 2;//allow left curve to spawn
                    }
                    else
                    {
                        rightCounter++;
                        leftCounter = 0;
                        return 1;//spawn right curve instead
                    }
                    
                    
                default: return 0;//all esle spawn straight
            }
            
        }
        else
        {
            preIndex = trackIndex;
            return trackIndex;
        }
    }

    bool CheckCollisionWithTrack(Transform cp, Vector3 direction)
    {
        //Debug.Log("checking if blocked");
        Ray ray = new Ray(cp.position, direction);
        RaycastHit hit;

        return Physics.Raycast(ray, out hit, 200);
    }

    void AssignTargetPoints(Transform trackPart)
    {//takes in the transform of a track part, and looks at children, if child is TargetPoints, assigns it to list



        for (int j = 0; j < trackPart.childCount; j++)
        {
            //Debug.Log(j+ " / " + trackPart.childCount);
            Transform AOA = trackPart.transform.GetChild(j);

            if (AOA.CompareTag("TP"))
            {
                AOA.name = trackPart.name + "/" + j;
                npc_TargetPoints.ArrayOfArrays.Add(AOA.gameObject);
            }

        }


    }

    void SpawnFinishLine(Transform conectionPoint)
    {//spawn in finihs line//set gamemanger, which holds collider to finihsline position
        var trackPart = PhotonNetwork.InstantiateRoomObject(Path.Combine("TrackParts", "FinishLine"), conectionPoint.position, conectionPoint.rotation); ;

        ActiveTrackParts.Add(trackPart);

        AssignTargetPoints(trackPart.transform);

        //finishPoint = trackPart.transform.GetChild(0);
    }

    public void LeaveGame()
    {
        Destroy(playerObj);
        if (!PhotonNetwork.IsMasterClient)
        {
            Racers.RemoveAll(x => x.gameObject == null);
        }
        
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);

    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("New Master");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene(0);
            //base.OnMasterClientSwitched(newMasterClient);
        }

    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(1);

        base.OnLeftRoom();
    }
    public void JoinNewGame()
    {
        _raceFinished = true;
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();

    }

    private Color[] ShuffleColors(Color[] Colors)
    {//Knuth Shuffle, runs throuhg entire array, switching value posistions at random
        Color[] newArray = Colors.Clone() as Color[];

        for (int i = 0; i < newArray.Length; i++)
        {
            Color temp = newArray[i];

            int r = Random.Range(i, newArray.Length);
            newArray[i] = newArray[r];
            newArray[r] = temp;
        }
        return newArray;
    }

    private string[] ShuffleNames(string[] names)
    {//Knuth Shuffle, runs throuhg entire array, switching value posistions at random
        string[] newArray = names.Clone() as string[];

        for (int i = 0; i < newArray.Length; i++)
        {
            string temp = newArray[i];

            int r = Random.Range(i, newArray.Length);
            newArray[i] = newArray[r];
            newArray[r] = temp;
        }
        return newArray;
    }


}
