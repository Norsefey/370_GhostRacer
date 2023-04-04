using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class NPCManager : MonoBehaviour
{
    
    public bool _active = false;
    [SerializeField]
    bool testing;

    [Header("Target Points")]

    [SerializeField]//distance to target point needed to change to new target point
    float _minDistance;

    float _currentDistance;
    public int _AOAIndex = 0;

    NPC_TargetPoints _TargetPoints;

    public List<GameObject> _listOfPoints;//hold points within current ArrayOfArrays

    Transform _targetPoint;

    int _LOPIndex;

    [Space(5f)]

    [Header("Movement")]

   
    private float _baseMoveSpeed = 10;

    private float _sprintSpeed = 25;

    float _FinalMoveSpeed;

    [SerializeField]
    float _maxSprintTime;

    float _sprintTime;

    [SerializeField]
    float _maxSprintCoolDown;

    float _sprintCoolDown;


    [SerializeField]
    float _coolDownMultiplier;

    bool isSprinting = false;
    bool isResting = false;

    [Header("Resting Values")]
    [SerializeField]
    float _minRecoverTime;
    [SerializeField]
    float _maxRecoverTime;
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (!testing)
        {
            //npcs inherite the same base 
            _baseMoveSpeed = PlayerStats._finalBaseSpeed;
            //since the npc's sprint speed does not build up like the player, their srpint speed is reduced
            _sprintSpeed = (_baseMoveSpeed * 2) - 5;
        }
        
        //add some vareiation to the NPC's  starting move speed
        
        _FinalMoveSpeed = Random.Range(_baseMoveSpeed, _sprintSpeed - 2);
        
        
        //set timers
        _sprintCoolDown = _maxSprintCoolDown;
        _sprintTime = _maxSprintTime;


        //find the target point script of the scene
        if (_TargetPoints == null)
        {
            _TargetPoints = FindObjectOfType<NPC_TargetPoints>();
        }
        

        //at begining set points and select random index//increase index
        _TargetPoints.FindPointsInArray(_AOAIndex, _listOfPoints);
        RandomPointSelect();
        _AOAIndex++;
    }

    // Update is called once per frame
    void Update()
    {
        

        if (_active)
        {
            gameObject.GetComponent<CapsuleCollider>().enabled = true;
            SprintManager();
            NPCMovement();
            DistanceChecker();
        }
        else
        {
            gameObject.GetComponent<CapsuleCollider>().enabled= false;
        }
        

       
    }

    IEnumerator Resting(float minRecoverTime, float maxRecoverTime, float minFinalMoveSpeed, float maxFinalMoveSpeed)
    {//while Resting the NPC cannot move or move very slowly for a random time

        //how long it will stay in this stats
        float restTime = Random.Range(minRecoverTime, maxRecoverTime);
        
        //mainly add for when hitting obstacle, enough time to get past, but will then stop moving//dependent on animation time
        yield return new WaitForSeconds(.5f);
        
        _FinalMoveSpeed = Random.Range(minFinalMoveSpeed, maxFinalMoveSpeed);
        
        
        //Debug.Log("Recovering for: " + restTime);

        yield return new WaitForSeconds(restTime);

        //Debug.Log("Stop Resting");

        //once time has passed, revert to default jogging state and set sprint cooldown
        _sprintCoolDown = Random.Range(.5f, _maxSprintCoolDown);
        isSprinting = false;
        isResting = false;
    }

    void SprintManager()
    {
        if (isSprinting)
        {
            if (_sprintTime <= 0 && !isResting)
            {
                float delta = Random.Range(0, 100);

                if(delta <= 70)
                {
                    //Debug.Log("Stop Sprinting: " + delta);
                    _sprintCoolDown = Random.Range(.5f, _maxSprintCoolDown);
                    isSprinting = false;
                }
                else if ( delta > 70 && !isResting)
                {
                    
                    isResting = true;
                 
                    //Debug.Log("Taking a break" + delta);
                    StartCoroutine(Resting(_minRecoverTime, _maxRecoverTime, 0,1));

                }
               
            }

            if(!isResting)
            {
                _FinalMoveSpeed = _sprintSpeed;
                _sprintTime -= _coolDownMultiplier * Time.deltaTime;
            }
            
        }
        else
        {
            if(_sprintCoolDown <= 0)
            {
                //Debug.Log("Is Sprinting");
                _sprintTime = Random.Range(.5f, _maxSprintTime);
                isSprinting = true;
            }


            if (!isResting)
            {
                _FinalMoveSpeed = _baseMoveSpeed;

                _sprintCoolDown -= _coolDownMultiplier * Time.deltaTime;
            }
           
        }
    }

    void DistanceChecker()
    {
        //when close enough to target point select new point to move to
        _currentDistance = Vector3.Distance(transform.position, _targetPoint.position);

        if (_AOAIndex < _TargetPoints.AOALength && _currentDistance < _minDistance)
        {
            //Debug.Log("New Target");
            _TargetPoints.FindPointsInArray(_AOAIndex, _listOfPoints);
            RandomPointSelect();
            _AOAIndex++;
        }
        else if (_AOAIndex >= _TargetPoints.AOALength && _currentDistance <= .5)
        {
            _active = false;
        }
    }
    void NPCMovement()
    {
       
      //set target to move to from current list values using randomly selected index variable
      _targetPoint = _listOfPoints[_LOPIndex].transform;
        //simple movetowards
        Debug.DrawLine(transform.position, _targetPoint.position, Color.red);
        
        transform.position = Vector3.MoveTowards(transform.position, _targetPoint.position, _FinalMoveSpeed * Time.deltaTime);
        transform.LookAt(_targetPoint);
    }
    void RandomPointSelect()
    {//sets index to random value within list range
        _LOPIndex = Random.Range(0, _listOfPoints.Count);
    }

    private void OnTriggerEnter(Collider other)
    {
        //when NPC collides with another NPC or PLayer, change target if far enough
        if (other.CompareTag("NPC") || other.CompareTag("Player"))
        {
            if(_currentDistance > _minDistance + 2)
            {
                //Debug.Log("Changing Target");
                RandomPointSelect();
            }

        }else if (other.CompareTag("Obstacle"))
        {
           // Debug.Log("Hit an Obstacle");
            
            float delta = Random.Range(0, 100);
            
            if(delta <= 70)
            {
                //Play jumping Animation
                Debug.Log("Jump");
            }else if(delta > 70)
            {
                //Debug.Log("Could not Dodge");

                isResting = true;//Stops movement states from overriding move speed
                

                //recover time will be dependent on animation
                StartCoroutine(Resting(2, 2, 0, 0));
            }

        }
    }
    
}
