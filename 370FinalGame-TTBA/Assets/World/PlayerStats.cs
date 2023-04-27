using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PlayerStats : MonoBehaviour
{//should be renamed to GameStats


    [Header("Name Input")]
    //using input field to edit final player name
    [SerializeField]
    TMP_InputField _playerName;
    public static string _finalPlayerName = "Player";


    [Space(5)]

    [Header("Base Speed")]
    [SerializeField]
    TMP_Text _baseSpeedPointsText;
    
    float _tempBaseSpeedPoints = 15;
    public static float _finalBaseSpeed;

    [Space(5)]
    [Header("TrackLength")]
    [SerializeField]
    TMP_Text _trackLength;

    float _tempTrackLength = 20;
    public static float _finalTrackLength;

    [Space(5)]
    [Header("NPC Count")]
    [SerializeField]
    TMP_Text _NPCCountPointsText;

    float _tempNPCCountPoints = 10;
    public static float _finalNPCCount;

    [Space(5)]
    [Header("Trickster")]
    [SerializeField]
    TMP_Text _tricksterActivatedText;
    public static bool _activeTrickster = true;

    // Start is called before the first frame update
    void Start()
    {
        ActivateTrickster();
    }

    // Update is called once per frame
    void Update()
    {
        PointsDisplayManager();
    }


    void PointsDisplayManager()
    {//shows points to player
        _baseSpeedPointsText.text = _tempBaseSpeedPoints.ToString();
        _trackLength.text = _tempTrackLength.ToString();
        _NPCCountPointsText.text = _tempNPCCountPoints.ToString();
        
    }

    public void AddPoint(int point)
    {//add to point, 0 base speed, 1 sprint speed, 2 stamina//assigned on button
        
       
            switch (point)
            {
                case 0:
                if (_tempBaseSpeedPoints < 30)
                {
                    _tempBaseSpeedPoints++;
                    
                    break;
                }
                else
                    break;
                    
                case 1:
                if (_tempNPCCountPoints < 40)
                {
                    _tempNPCCountPoints += 5;
                    
                    break;
                }
                else
                    break;
                    
                    
                case 2:
                    _tempTrackLength++;
                    
                    break;

                default:
                    break;
            }
        
        
        
    }

    public void SubtractPoint(int point)
    {//subtract from point, 0 base speed, 1 sprint speed, 2 stamina//assigned on button
        switch (point)
        {
            case 0:
                if(_tempBaseSpeedPoints > 10)
                {
                    _tempBaseSpeedPoints--;
                    
                }
                
                break;
            case 1:
                if (_tempNPCCountPoints > 10)
                {
                    _tempNPCCountPoints -= 5;
                    
                }

                break;
            case 2:
                if (_tempTrackLength > 10)
                {
                    _tempTrackLength--;
                    
                }
                
                
                break;

            default:
                break;
        }

    }


    public void StartRace()
    {//finalizes all values, and starts game
        _finalBaseSpeed = _tempBaseSpeedPoints;
        _finalTrackLength= _tempTrackLength;
        _finalNPCCount= _tempNPCCountPoints;

        SceneManager.LoadScene(1);
    }

    public void EnterPlayerName(string input)
    {
        
        _finalPlayerName = input;
        

    }
    
    public void ActivateTrickster()
    {
        _activeTrickster= true;
        _tricksterActivatedText.text = "<color=green>Yes</color>";
    }

    public void DeactivateTrickster()
    {
        _activeTrickster = false;
        _tricksterActivatedText.text = "<color=red>No</color>";
    }

    public void LoadInstructions()
    {
        SceneManager.LoadScene(2);
    }
}
