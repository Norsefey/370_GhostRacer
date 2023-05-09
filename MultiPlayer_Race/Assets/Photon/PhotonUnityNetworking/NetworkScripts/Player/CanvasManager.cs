using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CanvasManager : MonoBehaviour
{//player script is now indpendent of canvas//this script hadns all UI elements
    
    [SerializeField]
    GameNetworkController gameNetworkController;
    PlayerMainMovementScript myMovementScript;

    [Header("Stamina")]
    private float _maxStamina = 10;
    [SerializeField]
    Image _exhaustedIndicator;
    [SerializeField]
    Image _actionLines;
    [SerializeField]
    Slider _staminaSlider;
    [SerializeField]
    Image _stamina_SliderColor;

    [Header("Options Menu")]
    [SerializeField]
    Image _background;
    [SerializeField]
    Slider _xSensesitivitySlider;
    [SerializeField]
    TMP_Text _xSensitivityText;

    [SerializeField]
    GameObject _leaveButton;
    [SerializeField]
    GameObject _replayButton;

    [Header("general UI")]
    [SerializeField]
    TMP_Text _countDownText;
    [SerializeField]
    TMP_Text _racerPositionText;
    [SerializeField]
    TMP_Text _raceTimerText;
    [SerializeField]
    Image shootAvaiable;
    float _raceTimerSec = 0;
    float _raceTimerMin = 0;
    bool _finishedUIShown = false;

    private bool _mouseLocked = true;

    GameObject finishLine;

    // Start is called before the first frame update
    void Start()
    {
        //local player will always be counted first in list, even if not host
        myMovementScript = gameNetworkController.playerOwnedObjects[0].GetComponent<PlayerMainMovementScript>();

        

        //locks mouse at start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _leaveButton.SetActive(false);
        _xSensesitivitySlider.gameObject.SetActive(false);
        _replayButton.SetActive(false);

        _staminaSlider.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        

        if (myMovementScript._playerFinished && !_finishedUIShown)
        {
            _racerPositionText.gameObject.SetActive(false);

            _staminaSlider.gameObject.SetActive(false);

            _leaveButton.SetActive(true);
            _replayButton.SetActive(true);
            _countDownText.gameObject.SetActive(true);

            //unlocks mouse at start
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _finishedUIShown = true;
        }
        


        if(!gameNetworkController.RaceStarted)
        {//display countdown
            _countDownText.text = gameNetworkController.Timer.ToString("F0");
            
        }
        else if( _countDownText.gameObject.activeSelf && gameNetworkController.RaceStarted && !myMovementScript._playerFinished)
        {//runs only once, when countdown reachs zero//allows player to start moving//hids countdown text
            

            finishLine = GameObject.FindGameObjectWithTag("FinishLine");
            myMovementScript.CanMove = true;
            _raceTimerText.gameObject.SetActive(true);
            _racerPositionText.gameObject.SetActive(true);
            _countDownText.gameObject.SetActive(false);
            _countDownText.text = "";
            
        }
        else if (gameNetworkController.RaceStarted & !myMovementScript._playerFinished)
        {//when the race is going on//and the player has not finished
            
            RaceTimerManager();//once player finishes stop timer
            SortPositionList();

            //DisplayRacersPosition();
            DisplayerRacers();
        }
        //Settings Menu//Does not pause game time
        if (Input.GetKeyDown(KeyCode.P))
        {
            
            if (_mouseLocked)
            {//for entering pause menu//unlock Mouse
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                _mouseLocked = false;

                myMovementScript.CanLook = false;


                _background.enabled = true;
                _xSensesitivitySlider.gameObject.SetActive(true);
                _leaveButton.SetActive(true);
                _replayButton.SetActive(true);

                _staminaSlider.gameObject.SetActive(false);

            }
            else
            {//for exiting pause menu//lock Mouse
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _mouseLocked = true;
                
                myMovementScript.CanLook = true;

                _background.enabled = false;
                _xSensesitivitySlider.gameObject.SetActive(false);
                _leaveButton.SetActive(false);
                _replayButton.SetActive(false);

                _staminaSlider.gameObject.SetActive(true);
                
            }

        }

        //updates player UI(Stamina/debuffs)
        if (_staminaSlider.gameObject.activeSelf)
        {
            _staminaSlider.value = myMovementScript.CurrentStamina;
            
            if (myMovementScript.CurrentStamina >= _maxStamina)
            {
                _exhaustedIndicator.enabled = false;
                _actionLines.enabled = false;
                _stamina_SliderColor.color = Color.yellow;
            }
            else if (!myMovementScript.Exhausted && myMovementScript.CurrentStamina >= 3)
            {//when stamina is greater return to default state
                _exhaustedIndicator.enabled = false;
                _actionLines.enabled = false;
                _stamina_SliderColor.color = Color.yellow;
            }
            //living on the edge, when stamina is low, change color to red, second wind State, player speeds up faster and has higher top speed
            else if (!myMovementScript.Exhausted && myMovementScript.CurrentStamina < 3)
            {
                _actionLines.enabled = true;
                
                _stamina_SliderColor.color = Color.red; 
            }
            if(myMovementScript.canShoot)
            {
                shootAvaiable.enabled = true;
            }
            else
            {
                shootAvaiable.enabled = false;
            }

            if (myMovementScript.Exhausted)
            {
                _actionLines.enabled = false;
                _exhaustedIndicator.enabled = true;
                _stamina_SliderColor.color = Color.gray;
                
            }
        }
        

    }

    void SortPositionList()
    {
        //list.Sort takes in two parameters x and y, then tell it how to compare those parameters
        //in this case compare x's and y's distance to finishpoint
        //Determines race postion by checking who is closest to finish point
        gameNetworkController.Racers.RemoveAll(x => x.gameObject == null);//for when a client player leaves//null breaks sort system
        
        gameNetworkController.Racers.Sort((x, y) =>
           Vector3.Distance(x.transform.position, finishLine.transform.position).CompareTo
           (Vector3.Distance(y.transform.position, finishLine.transform.position)));
        
    }

    void DisplayerRacers()
    {
        _racerPositionText.text = "";
        for (int x = 0; x < gameNetworkController.Racers.Count(); x++)
        {

            if (gameNetworkController.Racers[x].name == myMovementScript.gameObject.name)
            {

                _racerPositionText.text += "<color=green>" + "<b>" + "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name + "</color>" + "</b>";
            }
            else
            {
                _racerPositionText.text += "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name;
            }
                
                
        }
        
    }


    void DisplayRacersPosition()
    {
        //resets text
        _racerPositionText.text = "";
        int racerCount = gameNetworkController.Racers.Count;

        //adds racer names in order form list
        for (int x = 0; x < racerCount; x++)
        {
            if (gameNetworkController.Racers[x].name == myMovementScript.gameObject.name)
            {
                
              _racerPositionText.text += "<color=green>" + "<b>" + "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name + "</color>" + "</b>";
                

            }else 
            {
                if(!PhotonNetwork.IsMasterClient)
                {//for the non master clients
                    for (int j = 0; j < PhotonNetwork.PlayerList.Length; j++)
                    {
                        if (PhotonNetwork.PlayerList[j].NickName == gameNetworkController.Racers[x].name)
                        {
                            /*if (gameNetworkController.Racers[x].GetComponent<PhotonPlayer>().finished)
                            {
                                _racerPositionText.text += "<color=white>" + "<b>" + "\n" + "Finished" +  " : " + gameNetworkController.Racers[x].name + "</color>" + "</b>";
                            }
                            else
                            {
                                _racerPositionText.text += "<color=blue>" + "<b>" + "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name + "</color>" + "</b>";
                            }*/
                            _racerPositionText.text += "<color=blue>" + "<b>" + "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name + "</color>" + "</b>";
                            break;
                        }
                        else
                        {
                            if (x < 10)
                            {
                                /*if (gameNetworkController.Racers[x].GetComponent<NPC_Finished>() != null)
                                {
                                    if(gameNetworkController.Racers[x].GetComponent<NPC_Finished>().finished)
                                        _racerPositionText.text += "\n" + "<color=white>Finished" + " : " + gameNetworkController.Racers[x].name + "</color>";
                                    else
                                    {
                                        _racerPositionText.text += "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name;

                                    }
                                }*/
                                _racerPositionText.text += "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name;
                                break;
                            }
                        }


                    }
                }
                else//for the master client
                {
                    for (int j = 1; j < PhotonNetwork.PlayerList.Length; j++)
                    {
                        if (PhotonNetwork.PlayerList[j].NickName == gameNetworkController.Racers[x].name)
                        {
                            /*if (gameNetworkController.Racers[x].GetComponent<PhotonPlayer>().finished)
                            {
                                _racerPositionText.text += "<color=white>" + "<b>" + "\n" + "Finished" + " : " + gameNetworkController.Racers[x].name + "</color>" + "</b>";
                            }
                            else
                            {
                                _racerPositionText.text += "<color=blue>" + "<b>" + "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name + "</color>" + "</b>";
                            }*/
                            _racerPositionText.text += "<color=blue>" + "<b>" + "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name + "</color>" + "</b>";
                            break;
                        }
                        else
                        {
                            if (x < 10)
                            {
                                /*if (gameNetworkController.Racers[x].GetComponent<NPC_Finished>().finished)
                                {
                                    _racerPositionText.text += "\n" + "<color=white>Finished" + " : " + gameNetworkController.Racers[x].name + "</color>";

                                }
                                else
                                {
                                    _racerPositionText.text += "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name;

                                }*/
                                _racerPositionText.text += "\n" + (x + 1) + " : " + gameNetworkController.Racers[x].name;
                                break;
                            }
                        }


                    }
                }
                
                
                    
            }


        }
    }

   public void DisplayeFinalPosition(int pos, string name)
    {
        _countDownText.text += pos.ToString("F0") + " : " + name + "\n";
    }

    void RaceTimerManager()
    {
        _raceTimerSec += Time.deltaTime;
        if (_raceTimerSec >= 60)
        {
            _raceTimerMin++;
            _raceTimerSec = 0;
        }


        _raceTimerText.text = _raceTimerMin + " : " + _raceTimerSec.ToString("00");
    }
    public void MouseSensitivityChange()
    {//allows player to change sensitivity of mouse
        myMovementScript.XSensitivity = _xSensesitivitySlider.value;
        _xSensitivityText.text = myMovementScript.XSensitivity.ToString();
    }


   

}
