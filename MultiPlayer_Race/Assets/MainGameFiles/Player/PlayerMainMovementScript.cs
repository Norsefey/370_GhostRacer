using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMainMovementScript : MonoBehaviour
{
    public bool Singleplayer;

    public bool _playerFinished = false;

    [Space(5)]
    [Header ("Main Movement")]
    
    private float _jogSpeed = 15;
    public float JogSpeed { set { _jogSpeed = value; } }
    public float TopSprintSpeed { set { _topSprintSpeed = value; } }
    private float _topSprintSpeed = 25;
    [SerializeField]
    private float _sprintSpeedMultiplier;
    private float _finalSprintSpeedMultiplier;
    private float _finalSpeed;

    private float _xMove;
    private float _zMove;

    private bool _canMove = false;
    public bool CanMove { get { return _canMove; } set { _canMove = value; } }
    private bool _canLook = true;
    public bool CanLook { get { return _canLook; } set { _canLook = value; } }

    [Space(5)]
    [Header("Stamina")]
    
   
    [SerializeField]
    float _staminaDepletionMultiplier;
    [SerializeField]
    float _staminaRecoveryRate;
    
    
    private float _maxStamina = 10;
    private float _currentStamina;
    public float CurrentStamina { get { return _currentStamina; } set { _currentStamina = value; } }

    private bool _exhausted = false;
    public bool Exhausted { get { return _exhausted; } set { _exhausted = value; } }


    [Space(5)]
    [Header("Camera Sensitivity")]
    

    private float _xSensitivity = 180;
    public float XSensitivity { get { return _xSensitivity; } set { _xSensitivity = value; } }
   

    private float _yMaxClamp = 45;
    private float _yMinClamp = -45;
    private float _vertlook = 0;
    
    [SerializeField]
    float _defaultFOV;
    [SerializeField]
    float _sprintFOV;

    [Space(5)]

    [Header("Jumping")]
    [SerializeField]
    float _initialVertForce;
    float _finalVertForce;

    [SerializeField]
    Transform _groundCheck;
    
    [SerializeField]
    float _gravityScale;
    float _gravity = -9.81f;
    bool _isFalling = false;
    bool _canJump = true;

    Camera _mainCam;
    CharacterController _characterController;
    //variables red ghost will use to mess with player
    public float _cameraZRotation = 0;//turns camerea upside
    public float _invertCam = 1;//inverst controls
    public float _slowDown = 1;


    //Shooting
    public bool canShoot = false;
    public int _shotCounter;

    private void Awake()
    {
        if (Singleplayer)
        {
            JogSpeed = WorldStats._finalBaseSpeed;
            //locks mouse at start
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

       
        _topSprintSpeed = _jogSpeed * 2;
        CanMove = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        _mainCam = Camera.main;
        _characterController = GetComponent<CharacterController>();
        
        _finalSpeed = _jogSpeed;
        _mainCam.fieldOfView = _defaultFOV;
        _currentStamina = _maxStamina;

        _jogSpeed = WorldStats._finalBaseSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (_canLook == true)
        {
            CameraLook();
            ShootingMethod();
        }
        

        if (_canMove)
        {
            MovementInput();
            MovementSpeedManager();

            Jump();


        }
        else
        {
            _finalSpeed= _jogSpeed;
            _currentStamina= _maxStamina;
        }
        
        
    }

    void Jump()
    {
        //checks if player is off ground//so that player cannot jump mid-air
        if(!Physics.Raycast(_groundCheck.position, Vector3.down, 1.5f) && _finalVertForce < 0 && !_characterController.isGrounded)
        {
            //Debug.Log("Grounded");
            _isFalling= true;
        }

        //when touching ground, no longer falling//reset vertical force
        if(_characterController.isGrounded)
        {
            _canJump= true;
            _isFalling= false;
            _finalVertForce = 0;
        }
        //player can only jump if that are grounded
        if (Input.GetKeyDown(KeyCode.Space) && _canJump)
        {
            //adds upward force//take initial verical force multiplies it by gravity and inverses it by a factor of 3
            _finalVertForce += Mathf.Sqrt((_initialVertForce * -1 * _gravity));
            _canJump= false;
        }
        //gravity is -9.81 ms^2, as such Time.deltaTime is implemented twice, here, and once again in movement method
        _finalVertForce += _gravity * _gravityScale * Time.deltaTime;
    }

    void MovementInput()
    {
        _finalSpeed = _finalSpeed / _slowDown;

        //input variables
        _xMove = Input.GetAxis("Horizontal") * _invertCam;
        _zMove = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(_xMove,0,_zMove) * _finalSpeed;

        moveDir = Vector3.ClampMagnitude(moveDir, _finalSpeed);

        //so that forward is the direction the player is facing
        moveDir = transform.TransformDirection(moveDir);

        moveDir.y += _finalVertForce;
      

        _characterController.Move(moveDir * Time.deltaTime);
    }

    void MovementSpeedManager()
    {
        

        if(_currentStamina >= _maxStamina)
        {
            _exhausted = false;
        }
        else if (!_exhausted && _currentStamina >= 3 )
        {//when stamina is greater return to default state
           
            _finalSprintSpeedMultiplier = _sprintSpeedMultiplier;
            _topSprintSpeed = _jogSpeed * 2;
           
        }
        //living on the edge, when stamina is low, change color to red, second wind State, player speeds up faster and has higher top speed
        else if (!_exhausted && _currentStamina < 3)
        {
            
            _topSprintSpeed = (_jogSpeed * 2) + 10;
            _finalSprintSpeedMultiplier = _sprintSpeedMultiplier + 2;
        }

       
        //player can only start sprinting if they are already moving, and Stamina isnt zero
        if (!_exhausted && Input.GetKey(KeyCode.LeftShift) && (_xMove != 0 || _zMove != 0) && !_isFalling)
        {
            //if player lets stamina reach 0, they become exhausted
            if(_currentStamina <= 0)
            {
                _exhausted = true;
            }
            //decrease stamina at set rate, while it is above 0
            if (_currentStamina > 0)
                _currentStamina -= _staminaDepletionMultiplier * Time.deltaTime;

            //players Field of View is increase, to give illusion of fast speed
            float fov = _mainCam.fieldOfView;
            if(fov < _sprintFOV)
            {//increases by a factor of 4
                _mainCam.fieldOfView += 4 * Time.deltaTime;
            }

            _finalSpeed += Time.deltaTime * _finalSprintSpeedMultiplier;
            //clamp sprint speed, so it cannot rise higher then top speed
            _finalSpeed = Mathf.Clamp(_finalSpeed, _jogSpeed + 1, _topSprintSpeed);

        }else if(!_isFalling)//when player stops sprinting
        {
            
            //if they are still moving, decreass the movement speed until it is back to jogspeed
            if ((_xMove != 0 || _zMove != 0) && _finalSpeed != _jogSpeed)
            {
                //player's fov is returned to normal when they stop sprinting
                float fov = _mainCam.fieldOfView;
                if (fov > _defaultFOV)
                {
                    _mainCam.fieldOfView -= 4 * Time.deltaTime;
                }

                //replenish Stamina, if it is not full
                if (_currentStamina < _maxStamina)
                {
                    _currentStamina += _staminaRecoveryRate * Time.deltaTime;
                }

                if (_exhausted)//when exhausted move speed decreases drastically
                {
                    _finalSpeed = 1;
                }
                else//if not exhausted, return to normal jogging speed
                {
                    
                    _finalSpeed -= Time.deltaTime * _finalSprintSpeedMultiplier * 2;
                    //clamp speed, until it reaches _jogspeed
                    _finalSpeed = Mathf.Clamp(_finalSpeed, _jogSpeed, _topSprintSpeed);
                }

                
            }
            else//if player stops moveing all together, instantly set movespeed to jogspeed
            {
                //player's fov is returned to normal immediatly when they stop moving
                float fov = _mainCam.fieldOfView;
                if (fov > _defaultFOV)
                {
                    _mainCam.fieldOfView -= 10 * Time.deltaTime;
                }
                else if(fov < _defaultFOV)
                {
                    _mainCam.fieldOfView = _defaultFOV;
                }

                //replenish Stamina at faster the rate
                if (_currentStamina < _maxStamina && _currentStamina != 0)
                    _currentStamina += _staminaRecoveryRate * 1.5f * Time.deltaTime;

                if(_exhausted)//when exhausted move speed decreases drastically
                {
                    _finalSpeed = 1;
                }
                else//if not exhausted, return to normal jogging speed
                {
                    _finalSpeed = _jogSpeed;
                }
                
            }
            
        }
    }

    void CameraLook()
    {
        //rotates whole body, player can look left and right, and move in direction it is looking in
        transform.Rotate(0, Input.GetAxis("Mouse X") * _invertCam * _xSensitivity * Time.deltaTime,0);

        //only rotates camera on x axis, so that player body does not tilt
        _vertlook -= Input.GetAxis("Mouse Y");
        _vertlook = Mathf.Clamp(_vertlook, _yMinClamp, _yMaxClamp);//locks how high/low player can look
        
        //will set new angle for camera, as such need refrence to previous y rotation
        float camHorRotation = _mainCam.transform.localEulerAngles.y;

        _mainCam.transform.localEulerAngles = new Vector3(_vertlook, camHorRotation, _cameraZRotation) * _invertCam;
    }

    void ShootingMethod()
    {
        if (_shotCounter > 2)
            _shotCounter = 2;
        else if (_shotCounter < 0)
            _shotCounter = 0;

        if (_shotCounter > 0)
        {
            canShoot = true;
        }
        else
        {
            canShoot = false;
        }

        

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 point = new Vector3(_mainCam.pixelWidth / 2, _mainCam.pixelHeight / 2, 0);
            Ray ray = _mainCam.ScreenPointToRay(point);
            RaycastHit hit;

            if (Singleplayer)
            {//in single player player can shoot trickster to avoid a debuff
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject hitObject = hit.transform.gameObject;

                    TricksterGhost target = hitObject.GetComponent<TricksterGhost>();

                    if (target != null)
                    {
                        //target will disappear
                        target.CalledOut();
                        Debug.Log("Trickster Called out");
                    }

                }
            }
            else if(canShoot && _shotCounter >= 0)
            {//in multiplayer player can shoot other players to debuff them//they get one shot//which can be recharged by pick up Mysteryboxes
                if (Physics.Raycast(ray, out hit))
                {
                    _shotCounter--;
                    GameObject hitObject = hit.transform.gameObject;
                    PhotonPlayer target = hitObject.GetComponent<PhotonPlayer>();

                    if(target != null)
                    {//if it hits a player, give that player a random status effect
                        target.CallIRS();
                    }
                    else
                    {//if player misses//shot is used up and spawn in a bullet effect at hit point
                        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "BulletEffect"), hit.point, Quaternion.identity);
                    }

                }
                
            }
            
        }
    }



    
}
