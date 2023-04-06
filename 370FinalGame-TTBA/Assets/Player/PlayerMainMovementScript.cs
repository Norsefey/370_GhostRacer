using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMainMovementScript : MonoBehaviour
{
    [SerializeField]
    bool testing;


    [Header ("Main Movement")]
    
    private float _jogSpeed = 10;
    private float _topSprintSpeed = 25;
    [SerializeField]
    private float _sprintSpeedMultiplier;
    
    private float _finalSpeed;

    private float _xMove;
    private float _zMove;

    public bool _canMove = false;
    public bool _canLook = true;

    [Space(5)]

    [Header("Stamina")]
    
    [SerializeField]
    Image _exhaustedIndicator;

    [SerializeField]
    Slider _staminaSlider;
    [SerializeField]
    Image _stamina_SliderColor;
    [SerializeField]
    float _staminaDepletionMultiplier;
    [SerializeField]
    float _staminaRecoveryRate;
    
    
    float _maxStamina = 10;
    public float _currentStamina;

    public bool _exhausted = false;

    [Space(5)]
    [Header("Camera Sensitivity")]
    [SerializeField]
    public float _xSensitivity;
    [SerializeField]
    Slider _xSensesitivitySlider;
    [SerializeField]
    TMP_Text _xSensitivityText;
   

    private float _yMaxClamp = 45;
    private float _yMinClamp = -45;
    private float _vertlook = 0;
    
    [SerializeField]
    float _defaultFOV;
    [SerializeField]
    float _sprintFOV;

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


    Camera _mainCam;
    CharacterController _characterController;
    
    
    //variables red ghost will use to mess with player
    public float _cameraZRotation = 0;//turns camerea upside
    public float _invertCam = 1;//inverst controls
    public float _slowDown = 1;

    private void Awake()
    {
        if (!testing)
        {
            //assigns player attributes
            gameObject.name = PlayerStats._finalPlayerName;
            _jogSpeed = PlayerStats._finalBaseSpeed;
            
        }
        _topSprintSpeed = _jogSpeed * 2;

    }

    // Start is called before the first frame update
    void Start()
    {

        _mainCam = Camera.main;
        _characterController = GetComponent<CharacterController>();
        
        //locks mouse at start
        Cursor.lockState= CursorLockMode.Locked;
        Cursor.visible= false;
        _finalSpeed = _jogSpeed;
        _mainCam.fieldOfView = _defaultFOV;
        _currentStamina = _maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        

        if (_canLook == true)
        {
            CameraLook();
            CalloutRedGhostAim();
        }
        

        if (_canMove)
        {
            MovementInput();
            MovementSpeedManager();
            Jump();
        }
        
        
    }

    void Jump()
    {
        //checks if player is off ground//so that player cannot jump mid-air
        if(!Physics.Raycast(_groundCheck.position, Vector3.down, 1.5f))
        {
            //Debug.Log("Grounded");
            _isFalling= true;
        }

        //when touching ground, no longer falling//reset vertical force
        if(_characterController.isGrounded)
        {
            _isFalling= false;
            _finalVertForce = 0;
        }
        //player can only jump if that are grounded
        if (Input.GetKey(KeyCode.Space)&& !_isFalling)
        {
            //adds upward force//take initial verical force multiplies it by gravity and inverses it by a factor of 3
            _finalVertForce += Mathf.Sqrt((_initialVertForce * -1 * _gravity));

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
        //if(_staminaText.enabled)
        //_staminaText.text = _currentStamina.ToString("F0");

        if (_staminaSlider.enabled)
            _staminaSlider.value = _currentStamina;

        if (_currentStamina >= _maxStamina)
        {
            _exhaustedIndicator.enabled = false;
            _exhausted = false;
            _stamina_SliderColor.color = Color.yellow;
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

            float fov = _mainCam.fieldOfView;

            if(fov < _sprintFOV)
            {
                _mainCam.fieldOfView += 4 * Time.deltaTime;
            }
            
             


            _finalSpeed += Time.deltaTime * _sprintSpeedMultiplier;
            //clamp sprint speed, so it cannot rise higher then top speed
            _finalSpeed = Mathf.Clamp(_finalSpeed, _jogSpeed + 1, _topSprintSpeed);

        }else if(!_isFalling)//when player stops sprinting
        {
            //if they are still moving, decreass the movement speed until it is back to jogspeed
            if((_xMove != 0 || _zMove != 0) && _finalSpeed != _jogSpeed)
            {
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
                    _exhaustedIndicator.enabled = true;
                    _stamina_SliderColor.color = Color.gray;
                    _finalSpeed = 1;
                }
                else//if not exhausted, return to normal jogging speed
                {
                    
                    _finalSpeed -= Time.deltaTime * _sprintSpeedMultiplier * 2;
                    //clamp speed, until it reaches _jogspeed
                    _finalSpeed = Mathf.Clamp(_finalSpeed, _jogSpeed, _topSprintSpeed);
                }

                
            }
            else//if player stops moveing all together, instantly set movespeed to jogspeed
            {
                float fov = _mainCam.fieldOfView;
                if (fov > _defaultFOV)
                {
                    _mainCam.fieldOfView -= 10 * Time.deltaTime;
                }
                else if(fov < _defaultFOV)
                {
                    _mainCam.fieldOfView = _defaultFOV;
                }

                //replenish Stamina at double the rate
                if (_currentStamina < _maxStamina && _currentStamina != 0)
                    _currentStamina += _staminaRecoveryRate * 2 * Time.deltaTime;

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

    void CalloutRedGhostAim()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 point = new Vector3(_mainCam.pixelWidth / 2, _mainCam.pixelHeight / 2, 0);
            Ray ray = _mainCam.ScreenPointToRay(point);
            RaycastHit hit;

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
    }

    public void MouseSensitivityChange()
    {
        _xSensitivity = _xSensesitivitySlider.value;
        _xSensitivityText.text = _xSensitivity.ToString();
    }
}
