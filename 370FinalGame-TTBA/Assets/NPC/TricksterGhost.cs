using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TricksterGhost : MonoBehaviour
{
    public bool _isTrickstering = true;


    Collider _myCollider;
    GameObject _mesh;

    [Header("Respawn Time")]
    [SerializeField]
    float _minRespawn;
    [SerializeField]
    float _maxRespawn;

    [Header("Positioning")]
    [SerializeField]
    float _spawnRadius;
    [SerializeField]
    float _moveSpeed;

    [Header("UI Elements")]
    [SerializeField]
    Image _tricksterActiveIndicator;
    [SerializeField]
    Image _tricksterStatusIndicator;

    //refrences to player
    GameObject _playerRef;
    PlayerMainMovementScript _playerMainMovementScript;
    
    // Start is called before the first frame update
    void Start()
    {
        _mesh = gameObject.transform.GetChild(0).gameObject;
        _myCollider = GetComponent<Collider>();

        if (_playerRef == null)//gets refrence to player to be added to list
        {
            _playerRef = GameObject.FindGameObjectWithTag("Player");
            _playerMainMovementScript = _playerRef.GetComponent<PlayerMainMovementScript>();
            
        }
        _moveSpeed = PlayerStats._finalBaseSpeed;

        _tricksterActiveIndicator.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawLine(_playerRef.transform.position, ((Random.insideUnitSphere * _spawnRadius + _playerRef.transform.position)), Color.red);

        if (_isTrickstering)
        {//is active// will move towards the player
            _tricksterActiveIndicator.enabled= true;
            if (Vector3.Distance(transform.position, _playerRef.transform.position) > 40)
            {//if far enoguh will despawn
                CalledOut();
            }

            
            transform.position = Vector3.MoveTowards(transform.position, _playerRef.transform.position, _moveSpeed * Time.deltaTime);
            transform.LookAt(_playerRef.transform);
        }
    }

    IEnumerator Respawn()
    {//respawn at random intervals
       
        //Debug.Log("Performing Trick");
        
        float delta = Random.Range(_minRespawn, _maxRespawn);

        yield return new WaitForSeconds(delta);



        //find a point at a distance in front of the player
        Vector3 spawnArea = _playerRef.transform.position + (_playerRef.transform.forward * (PlayerStats._finalBaseSpeed * 2));
        Vector3 spawnPoint = (Random.insideUnitSphere * _spawnRadius + spawnArea);

        spawnPoint.y = _playerRef.transform.position.y;
        Debug.Log(spawnPoint);
        transform.position = spawnPoint;

        

        //reenable visuals
        _myCollider.enabled = true;
        _mesh.SetActive(true);
        _isTrickstering= true;
    }
    public void CalledOut()
    {//ghost disapear
        _tricksterActiveIndicator.enabled = false;
        _isTrickstering = false;

        _myCollider.enabled = false;
        _mesh.SetActive(false);

        StartCoroutine(Respawn());
    }
   

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {//ghost gives player a status then disapears
            Debug.Log("touched Player");
            InflictRandomStatus();
            CalledOut();
        }
    }

    IEnumerator ResetPlayerEffects(float effectDuration, int delta)
    {
        yield return new WaitForSeconds(effectDuration);
        _tricksterStatusIndicator.enabled = false;
        switch (delta)
        {
            case 1:
                _playerMainMovementScript._cameraZRotation = 0;
                break;
            case 2:
                _playerMainMovementScript._invertCam = 1;
                break;
            case 3:
                _playerMainMovementScript._slowDown = 1;
                break;
            default:
                break;
        }
    }

    void InflictRandomStatus()
    {
        //will inflict a random status effect on player temporarly
        int delta = Random.Range(0, 4);
        _tricksterStatusIndicator.enabled = true;
        switch (delta)
        {
            case 0://drains all stamina and makes player exhausted

                _playerMainMovementScript._currentStamina = 0;
                _playerMainMovementScript._exhausted = true;
                StartCoroutine(ResetPlayerEffects(0, delta));
                break;
            case 1://flips camera upside down
                _playerMainMovementScript._cameraZRotation = 180;
                StartCoroutine(ResetPlayerEffects(2, delta));
                break;
            case 2://Inverts controls
                _playerMainMovementScript._invertCam = -1;
                StartCoroutine(ResetPlayerEffects(2, delta));
                break;
            case 3://slows movement speed
                _playerMainMovementScript._slowDown = 2;//divides finalmovespeed by slowdown//in this case cuts speed in half
                StartCoroutine(ResetPlayerEffects(2, delta));
                break;
         default: 
           break;
        }
    }
}
