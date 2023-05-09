using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    Transform _finishPoint;

    [Header("UI Text")]
    [SerializeField]
    TMP_Text _positionText;
    [SerializeField]
    TMP_Text _countDownText;
    [SerializeField]
    TMP_Text _raceTimerText;
    [SerializeField]
    TMP_Text _sensitivityText;
    

    [Header("UI Panels")]
    
    [SerializeField]
    Image _overlayPanel;
    [SerializeField]
    Image _crosshair;

    [Header("Buttons")]
    [SerializeField]
    Button _mainMenuButton;
    [SerializeField]
    Button _replayButton;

    [Header("Sliders")]
    [SerializeField]
    Slider _mouseSensitivitySlider;
   
    

    bool _uiIshown = false;

    public List<GameObject> ActiveRacersPosList = new List<GameObject>();

     private List<GameObject> FinalRacerPosList= new List<GameObject>();

    //names to be given randomly to NPCs//each NPC gets unique name
    private List<string> NPCNamesList = new List<string>()
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

    //race Conditions
    bool gamePaused = false;
    bool _raceStarted = false;
    public bool _raceFinished = false;
    bool _playerFinished = false;
    //race start timer
    bool _startCountDown = false;
    
    public float _countDown = 10;

    //timer during race
    private float _raceTimerSeconds = 0;
    float _raceTimerMinutes = 0;

    //refernce to player, to assign player to list of racers
    GameObject _playerRef;
    PlayerMainMovementScript _pMMScript;
    string _playerName;

    GameObject _trickster;

    [Header("Player Stamina Handler")]
    private float _maxStamina = 10;
    [SerializeField]
    Image _exhaustedIndicator;
    [SerializeField]
    Image _actionLines;
    [SerializeField]
    Slider _staminaSlider;
    [SerializeField]
    Image _stamina_SliderColor;

    private void Awake()
    {
        if (_playerRef == null)//gets refrence to player to be added to list
        {
            _playerRef = GameObject.FindGameObjectWithTag("Player");
        }
        _pMMScript = _playerRef.GetComponent<PlayerMainMovementScript>();
        if (_trickster == null)
        {
            _trickster = GameObject.FindGameObjectWithTag("Trickster");
        }

        //store refrence to final player name, for position identifier
        
        
        //Hide UI

        Time.timeScale = 1;

        _mainMenuButton.gameObject.SetActive(false);
        _replayButton.gameObject.SetActive(false);
        _mouseSensitivitySlider.gameObject.SetActive(false);
        _staminaSlider.gameObject.SetActive(false); 
        _positionText.enabled = false;
        _raceTimerText.enabled = false;
        _crosshair.enabled = false;
        
    }


    // Start is called before the first frame update
    void Start()
    {
        
        ActiveRacersPosList.Add(_playerRef);
        _playerName = _playerRef.name;

        //adds all active NPC's to list
        foreach (GameObject NPC in GameObject.FindGameObjectsWithTag("NPC"))
        {
            //gives each NPC a random name from list//for as long as it has names
            if (NPCNamesList.Count != 0)
            {
                
                int delta = Random.Range(0, NPCNamesList.Count);
           
                NPC.gameObject.name = NPCNamesList[delta];
                NPCNamesList.RemoveAt(delta);//no repeated names

               
            }
            NPC.transform.GetChild(0).transform.GetChild(1).GetComponent<Renderer>().material.color = Random.ColorHSV();
            //adds NPC's now with names to active racers list
            ActiveRacersPosList.Add(NPC);
            
        }

        //displays the names of the player an NPCs
        DisplayRacersPosition();

        //starts the race coutdown
        _startCountDown= true;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if(gamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }


        RaceTimerManager();
        RaceFinishManager();


        //timer at the begining of the race
        if (_startCountDown && !_raceStarted)
        {
            _countDown -= 2 * Time.deltaTime;
            _countDownText.text = _countDown.ToString("F0");
            
        }

        //when the timer reachs 0 races starts//all NPCs are activated, and player is allowed to move
        if(!_raceStarted && !_raceFinished && _countDown <= 0)
        {
            //hid countdown and starting UI
            _startCountDown = false;
            _overlayPanel.enabled = false;
            _countDownText.enabled = false;
            _countDownText.enabled= false;
            
            //enable race UI
            _positionText.enabled = true;
            _staminaSlider.gameObject.SetActive(true);
            _raceTimerText.enabled = true;
            _crosshair.enabled = WorldStats._activeTrickster;
            
            

            _playerRef.GetComponent<PlayerMainMovementScript>().CanMove = true;
            for (int x = 0; x < ActiveRacersPosList.Count; x++)
            {
                if(ActiveRacersPosList[x].GetComponent<NPCManager>() != null)
                    ActiveRacersPosList[x].GetComponent<NPCManager>()._active = true;
            }
            _raceStarted = true;
            _trickster.GetComponent<TricksterGhost>()._isTrickstering = WorldStats._activeTrickster;
        }

        if (_staminaSlider.gameObject.activeSelf)
        {
            _staminaSlider.value = _pMMScript.CurrentStamina;

            if (_pMMScript.CurrentStamina >= _maxStamina)
            {
                _exhaustedIndicator.enabled = false;
                _actionLines.enabled = false;
                _stamina_SliderColor.color = Color.yellow;
            }
            else if (!_pMMScript.Exhausted && _pMMScript.CurrentStamina >= 3)
            {//when stamina is greater return to default state
                _exhaustedIndicator.enabled = false;
                _actionLines.enabled = false;
                _stamina_SliderColor.color = Color.yellow;
            }
            //living on the edge, when stamina is low, change color to red, second wind State, player speeds up faster and has higher top speed
            else if (!_pMMScript.Exhausted && _pMMScript.CurrentStamina < 3)
            {
                _actionLines.enabled = true;
                
                _stamina_SliderColor.color = Color.red;
            }


            if (_pMMScript.Exhausted)
            {
                _actionLines.enabled = false;
                _exhaustedIndicator.enabled = true;
                _stamina_SliderColor.color = Color.gray;
                
            }
        }



        RaceManager();
    }

    public void MouseSensitivityChange()
    {//allows player to change sensitivity of mouse
        _pMMScript.XSensitivity = _mouseSensitivitySlider.value;
        _sensitivityText.text = _pMMScript.XSensitivity.ToString();
    }
    void RaceManager()
    {
        //while the race is going keep track of racers position and update display
        if (_raceStarted)
        {

            if (!_playerFinished)
                _raceTimerSeconds += Time.deltaTime;

            //when all racers have passsed the finish line end race
            if (FinalRacerPosList.Count == ActiveRacersPosList.Count)
            {
                _raceFinished = true;
                _raceStarted = false;
            }


            //list.Sort takes in two parameters x and y, then tell it how to compare those parameters
            //in this case compare x's and y's distance to finishpoint
            //Determines race postion by checking who is closest to finish point
            ActiveRacersPosList.Sort((x, y) =>
                Vector3.Distance(x.transform.position, _finishPoint.position).CompareTo
                (Vector3.Distance(y.transform.position, _finishPoint.position)));

            //displays list in set order
            DisplayRacersPosition();
        }
    }

    void RaceFinishManager()
    {
        if (_playerFinished && !_raceFinished)
        {
            _trickster.SetActive(false);
            //hide race UI
            if (!_uiIshown)
            {
                _positionText.enabled = false;
                _staminaSlider.gameObject.SetActive(false);
                
                _mainMenuButton.gameObject.SetActive(true);
                _replayButton.gameObject.SetActive(true);

                //unlocks mouse at start
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                _playerRef.GetComponent<PlayerMainMovementScript>().CanLook = false;
                _uiIshown = true;
            }
            




            //resets text
            _countDownText.text = "";

            //adds racers that have finished
            int racersFinished = FinalRacerPosList.Count;
            
            
            //adds racer names in order form list
            for (int x = 0; x < racersFinished; x++)
            {
                if (FinalRacerPosList[x].name == _playerName)
                {

                    _countDownText.text += "<color=green>" + "<b>" + "\n" + (x + 1) + " : " + FinalRacerPosList[x].name + "</color>" + "</b>";
                    
                }
                else
                {
                    if (x < 10)
                    {//keeps list to only top 10
                        _countDownText.text += "\n" + (x + 1) + " : " + FinalRacerPosList[x].name;
                    }
                    
                }


            }

           
            _overlayPanel.enabled = true;
            _countDownText.enabled = true;
        }
    }

    void DisplayRacersPosition()
    {
        //resets text
        _positionText.text = "";
        int racerCount = ActiveRacersPosList.Count;
        
        //adds racer names in order form list
        for (int x = 0; x < racerCount; x++)
        {
            if (ActiveRacersPosList[x].name == _playerName)
            {

                _positionText.text += "<color=green>" + "<b>" + "\n" + (x + 1) + " : " + ActiveRacersPosList[x].name + "</color>" + "</b>";

            }
            else
            {
                if (x < 10)
                    _positionText.text += "\n" + (x + 1) + " : " + ActiveRacersPosList[x].name;
            }

            
        }
    }

    void RaceTimerManager()
    {
        if(_raceTimerSeconds >= 60)
        {
            _raceTimerMinutes++;
            _raceTimerSeconds= 0;
        }


        _raceTimerText.text = _raceTimerMinutes + " : " + _raceTimerSeconds.ToString("00");
    }

    public void AddFinalRacerPosition(GameObject racer)
    {
        FinalRacerPosList.Add(racer);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Entered");

        //when an NPC enters the trigger//get root and assign to finalpos list
        if (other.CompareTag("NPC"))
        {
            //since the child has the collider
            //other.gameObject.GetComponent<NPCManager>()._active = false;
            StartCoroutine(NPCDeactivate(other));
            AddFinalRacerPosition(other.gameObject);
            
        }else if (other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerMainMovementScript>().CanMove = false;
            AddFinalRacerPosition(other.gameObject);
           _playerFinished = true;
            Time.timeScale = 2f;
        }
    }

    IEnumerator NPCDeactivate(Collider other)
    {
        yield return new WaitForSeconds(2);
        other.gameObject.GetComponent<NPCManager>()._active = false;
    }


    void PauseGame()
    {
        //set bool which tracks current state of game
        gamePaused= true;

        //disable player camera movement//since it is independent of time.deltatime
        _playerRef.GetComponent<PlayerMainMovementScript>().CanLook = false;

        //free the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //hid current racer positions//since it overlaps with buttons
        _positionText.enabled = false;
        _overlayPanel.enabled = true;
        //show menu buttons and sensitivity sliders
        _mainMenuButton.gameObject.SetActive(true);
        _replayButton.gameObject.SetActive(true);
        _mouseSensitivitySlider.gameObject.SetActive(true);
        //prevents everything from moving
        Time.timeScale = 0;
    }

    void ResumeGame()
    {
        //set bool which tracks current state of game
        gamePaused = false;
        //reenable player camera movement//since it was disabled
        _playerRef.GetComponent<PlayerMainMovementScript>().CanLook = true;
        //lock the mouse back up
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //return racer position text
        _positionText.enabled = true;
        _overlayPanel.enabled = false;
        //hid all buttons and non race UI
        _mainMenuButton.gameObject.SetActive(false);
        _replayButton.gameObject.SetActive(false);
        _mouseSensitivitySlider.gameObject.SetActive(false);
        //resume time
        Time.timeScale = 1;
    }
}
