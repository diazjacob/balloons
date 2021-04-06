using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : ManagedMonobehaviour
{
    //DIRECTION ADJUSTMENTS FOR INPUT TO WORLD
    public static Vector2 RIGHT_DIR = new Vector2(-1, 1).normalized;
    public static Vector2 UP_DIR = new Vector2(-1, -1).normalized;
    //Screen corner to center offset
    private static Vector2 _screenTouchOffset = new Vector2(Screen.width / 2f, Screen.height / 2f);

    [SerializeField] private GameObject _balloonObj;        //The balloon object (for rotation)

    [Header("Movement Settings: ")]
    [SerializeField] private float _moveSpeed;  //Movement speed based on input
    [SerializeField] private float _accelCap;   //Maximum single-frame speed change
    [SerializeField] private float _speedCap;   //Maximum speed
    [SerializeField] private float _speedHaltThreshold;     //When we slow down the balloon to stop at small speeds
    [Space]
    [SerializeField] private float _verticalMoveSpeed;      //Vertical speed based on input
    [SerializeField] private float _verticalSpeedCap;       //Vertical speed max
    [SerializeField] private float _verticalAccelCap;       //Vertical max single-frame speed delta
    [SerializeField] private float _verticalSpeedBleed;     //A bleed of vertical speed
    [SerializeField] private float _verticalSpeedHaltThreshold;     //Removal threshold of small vertical speeds
    [Space]
    [SerializeField] private float _rotationCoeff;      //How much to rotate the balloon based on speed
    [Space]
    [SerializeField] private float _maxHeight;      //The max height we'll still allow vertical input

    [Header("Landing Settings: ")]
    [SerializeField] private float _landingWarningDist; //The speed warning distance
    [SerializeField] private float _landingDetectionDist; //The detection distance
    [SerializeField] private float _validLandingHeight; //Valid terrain landing height

    [SerializeField] private GameObject _landingDeathParticleObj;   //Death particles
    private ParticleSystem _landingDeathParticles;

    [SerializeField] private GameObject[] _landingTethers;      //Teather objects
    [SerializeField] private float _maxTetherAngle;         //Max random tether angle

    [Header("Flight Settings: ")]
    [SerializeField] private float _maxInputChangeHeight;       //Max height you can change the input mode to lateral

    //UI
    [Header("UI Settings: ")]
    [SerializeField] private GameObject _touchBase; //The base lateral movement UI object
    [SerializeField] private GameObject _touchTracker;  //The tracking lateral movement Ui object
    [SerializeField] private float _maxDragDist;    //The max drag distance 
    [Space]
    [SerializeField] private GameObject _verticalThrustKnob;    //Vertical thruster UI knob
    [SerializeField] private GameObject _verticalThrustKnobHolder;      //Vertical thruster UI knob holder
    [SerializeField] private float _verticalThrustKnobRange;    
    [Space]
    [SerializeField] private GameObject _verticalThrustUI;      //Vertical thrust ui cluster
    [SerializeField] private GameObject _moveThrustUI;          //Move thrust ui cluster
    [Space]

    [Space]
    [SerializeField] private float _tapTimeMax;         //Maximum time to still register a touch as a tap
    [SerializeField] private float _doubleTapDeltaMax;      //Max time to register a double tap


    private bool _isDead;   //If we're dead

    private float _touchTimer;      //The timer for a single tap
    private float _doubleTouchTimer;    //The timer for a double tap
    private bool _singleTouch;  //if a single touch has happened
            

    private bool _thrusterModeVertical; //Thruster mode

    private Vector2 _touchPos;  //Initial touch pos


    private bool _inventoryOpen = false; //If the inventory is being used
    private void InventoryClosed() { _inventoryOpen = false; }
    private void InventoryOpen() { _inventoryOpen = true; }

    private void Awake()
    {
        GetAllRefs();
    }

    private void Start()
    {
        _landingDeathParticles = _landingDeathParticleObj.GetComponent<ParticleSystem>();

        _thrusterModeVertical = false;

        CameraController.OnInventoryOpened += InventoryOpen;
        CameraController.OnInventoryClosed += InventoryClosed;
    }

    private void Update()
    {
        if (!MPlayer().GetGamePaused()) 
        {
            if (!_isDead) //Not paused not dead         :)
            {
                CheckLanding();
                if (!MPlayer().GetLanded() && MPlayer().CanPlayerMove()) ProcessInput();
                else ClearInputUI();

#if UNITY_EDITOR
                ProcessTapsPC();
#else
                ProcessTapsMobile();
#endif
            }

            //APPLY ROTATION
            ProcessRotation();
        }
        else ClearInputUI();

    }

    //We have dual input based on compilation target, so it's a lil complicated
    private void ProcessInput()
    {

        //SETTING UP TO GET MOVE INPUT
        Vector2 moveDir = Vector2.zero;
        float vertDir = 0;

        //Processing Input;
#if UNITY_EDITOR
        if(!_inventoryOpen)
        {
            if (!_thrusterModeVertical) moveDir = ProcessMoveInputPC();
            else vertDir = ProcessVerticalInputPC();
        }
#else
        if(!_inventoryOpen)
        {
            if (!_thrusterModeVertical) moveDir = ProcessMoveInputMobile();
            else vertDir = ProcessVerticalInputMobile();
        }
#endif

        //CHANGE MOVE MODE based on height
        //CheckHeight();

        //APPLYING LATERAL VELOCITY
        Vector2 vel = MPlayer().GetVelocity();

        vel += moveDir;
        vel = Vector2.ClampMagnitude(vel, _speedCap);
        MPlayer().SetVelocity(vel);

        //APPLYING VERTICAL VELOCITY
        if(transform.position.y < _maxHeight || vertDir < 0) //limiting the height
        {
            float vertVel = MPlayer().GetVerticalVelocity();
            vertVel += vertDir;
            vertVel = Mathf.Clamp(vertVel, -_verticalSpeedCap, _verticalSpeedCap);

            MPlayer().SetVerticalVelocity(vertVel);
        }
    }


#region TAPS

    //Process tapping MOBILE
    private void ProcessTapsMobile()
    {
        if (_singleTouch)
        {
            _doubleTouchTimer += Time.deltaTime;

            
            if (_doubleTouchTimer > _doubleTapDeltaMax)
            {
                //SINGLE TAP RECOTGNITION
                SingleTap(_touchPos);



                _singleTouch = false;
                _doubleTouchTimer = 0;
                _touchTimer = 0;
            }
        }

        int touchCount = Input.touchCount;
        Touch[] touches = Input.touches;

        if (touchCount == 1)
        {
            if (touches[0].phase == TouchPhase.Began)
            {
                _touchTimer = 0;
                _touchPos = Input.mousePosition;
            }
            else if (touches[0].phase == TouchPhase.Moved || touches[0].phase == TouchPhase.Stationary) _touchTimer += Time.deltaTime;
            else if (touches[0].phase == TouchPhase.Ended)
            {
                if (_touchTimer < _tapTimeMax)
                {
                    if (_singleTouch)
                    {
                        if (_doubleTouchTimer < _doubleTapDeltaMax)
                        {
                            _touchPos = Input.mousePosition;
                            //DOUBLE TAP RECOGNITION
                            DoubleTap();
                            // ----------------
                        }

                        _singleTouch = false;
                        _doubleTouchTimer = 0;
                        _touchTimer = 0;
                    }
                    else
                    {
                        _doubleTouchTimer = 0;
                        _singleTouch = true;
                    }
                }

            }
        }
    }

    private void ProcessTapsPC()
    {
        if (_singleTouch)
        {
            _doubleTouchTimer += Time.deltaTime;

            //SINGLE TAP RECOTGNITION
            if (_doubleTouchTimer > _doubleTapDeltaMax)
            {
                //SINGLE TAP RECOTGNITION
                SingleTap(_touchPos);
                // ----------------


                _singleTouch = false;
                _doubleTouchTimer = 0;
                _touchTimer = 0;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            _touchTimer = 0;
            _touchPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0)) _touchTimer += Time.deltaTime;
        else if (Input.GetMouseButtonUp(0))
        {
            if (_touchTimer < _tapTimeMax)
            {
                if (_singleTouch)
                {

                    if (_doubleTouchTimer < _doubleTapDeltaMax)
                    {
                        _touchPos = Input.mousePosition;
                        //DOUBLE TAP RECOGNITION
                        DoubleTap();
                        // ----------------
                    }

                    _singleTouch = false;
                    _doubleTouchTimer = 0;
                    _touchTimer = 0;
                }
                else
                {
                    _doubleTouchTimer = 0;
                    _singleTouch = true;
                }
            }
        }

    }

    public void ToggleThrustMode()
    {
        ClearInputUI();

        _thrusterModeVertical = !_thrusterModeVertical;

        _verticalThrustUI.SetActive(_thrusterModeVertical);
        _moveThrustUI.SetActive(!_thrusterModeVertical);
    }

    private void DoubleTap()
    {
        if (!MPlayer().GetLanded()) ToggleThrustMode();
        else Takeoff();
    }

    private void SingleTap(Vector2 pos)
    {
        Ray ray = Camera.main.ScreenPointToRay(pos);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 1f); //Debug Draw ray
        if(Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            HandleTapHit(hit.collider.gameObject);
        }
    }

    private void HandleTapHit(GameObject obj)
    {
        switch (obj.tag)
        {
            case "House":

                Debug.Log("Tapped on House");
                HouseControl controller = obj.gameObject.GetComponentInParent<HouseControl>();
                controller.Tapped();

                break;
            case "Player":

                Debug.Log("Tapped on Player");
                //if(!MPlayer().GetLanded()) MCameraController().ToggleCameraOrtho();
                MCameraController().ToggleCameraOrtho();


                break;

            case "Item":

                Debug.Log("Tapped on Inventory Item");
                bool closeInv = MInventoryManager().UseItem(obj);
                if(closeInv) MCameraController().ToggleCameraOrtho();
                
                break;

            case "Enemy":

                break;
        }
    }

#endregion


#region MOVEMENT

    private Vector2 ProcessMoveInputMobile()
    {
        Vector2 returnVal = Vector2.zero;

        int touchCount = Input.touchCount;
        Touch[] touches = Input.touches;

        if(touchCount == 1)
        {
            if(touches[0].phase == TouchPhase.Began)
            {

                _touchPos = Input.mousePosition;
                _touchBase.transform.localPosition = _touchPos - _screenTouchOffset;
                _touchTracker.transform.localPosition = _touchPos - _screenTouchOffset;
            }
            else if((touches[0].phase == TouchPhase.Moved || touches[0].phase == TouchPhase.Stationary) && _touchTimer > _tapTimeMax)
            {
                _touchBase.SetActive(true);
                _touchTracker.SetActive(true);

                Vector2 mainTouch = Input.mousePosition;

                //Set the visual
                if (Vector2.Distance(mainTouch, _touchPos) > _maxDragDist) mainTouch = _touchPos + ((mainTouch - _touchPos).normalized * _maxDragDist);

                _touchTracker.transform.localPosition = mainTouch - _screenTouchOffset;

                //Movement calcs
                Vector2 touchDelta = mainTouch - _touchPos;

                Vector2 moveDir = RIGHT_DIR * touchDelta.x * _moveSpeed;
                moveDir += UP_DIR * touchDelta.y * _moveSpeed;

                moveDir = Vector2.ClampMagnitude(moveDir, _accelCap);

                returnVal = moveDir;
            }
        }
        else
        {
            _touchBase.SetActive(false);
            _touchTracker.SetActive(false);

            if (MPlayer().GetVelocity().magnitude < _speedHaltThreshold) MPlayer().SetVelocity(Vector3.zero);
        }

        return returnVal;
    }

    private Vector2 ProcessMoveInputPC()
    {
        Vector2 returnVal = Vector2.zero;


        if (Input.GetMouseButtonDown(0))
        {
            _touchPos = Input.mousePosition;
            _touchBase.transform.localPosition = _touchPos - _screenTouchOffset;
            _touchTracker.transform.localPosition = _touchPos - _screenTouchOffset;
        }
        else if (Input.GetMouseButton(0) && _touchTimer > _tapTimeMax)
        {
            _touchBase.SetActive(true);
            _touchTracker.SetActive(true);

            Vector2 mainTouch = Input.mousePosition;

            //Set the visual
            if (Vector2.Distance(mainTouch, _touchPos) > _maxDragDist) mainTouch = _touchPos + ((mainTouch - _touchPos).normalized * _maxDragDist);

            _touchTracker.transform.localPosition = mainTouch - _screenTouchOffset;

            //Movement calcs
            Vector2 touchDelta = mainTouch - _touchPos;

            Vector2 moveDir = RIGHT_DIR * touchDelta.x * _moveSpeed;
            moveDir += UP_DIR * touchDelta.y * _moveSpeed;

            moveDir = Vector2.ClampMagnitude(moveDir, _accelCap);

            returnVal = moveDir;
        }
        else
        {
            _touchBase.SetActive(false);
            _touchTracker.SetActive(false);

            if (MPlayer().GetVelocity().magnitude < _speedHaltThreshold) MPlayer().SetVelocity(Vector3.zero);
        }

        return returnVal;
    }

    private float ProcessVerticalInputMobile()
    {
        float returnVal = 0;

        int touchCount = Input.touchCount;
        Touch[] touches = Input.touches;

        if (touchCount == 1)
        {
            if (touches[0].phase == TouchPhase.Began)
            {
                _touchPos = Input.mousePosition;

                _verticalThrustKnobHolder.transform.localPosition = _touchPos - _screenTouchOffset;
                _verticalThrustKnob.transform.position = Vector2.zero;
            }
            else if ((touches[0].phase == TouchPhase.Moved || touches[0].phase == TouchPhase.Stationary) && _touchTimer > _tapTimeMax)
            {
                _verticalThrustKnobHolder.SetActive(true);
                _verticalThrustKnob.SetActive(true);

                Vector2 mainTouch = Input.mousePosition;

                float touchPosDelta = mainTouch.y - _touchPos.y;

                //Set the visual
                if (touchPosDelta > _verticalThrustKnobRange) touchPosDelta = _verticalThrustKnobRange;
                if (touchPosDelta < -_verticalThrustKnobRange) touchPosDelta = -_verticalThrustKnobRange;

                _verticalThrustKnob.transform.localPosition = (Vector2.right * touchPosDelta);

                //Movement calcs

                float moveDelta = touchPosDelta * _verticalMoveSpeed;

                moveDelta = Mathf.Clamp(moveDelta, -_verticalAccelCap, _verticalAccelCap);

                returnVal = moveDelta;
            }
        }
        else
        {
            _verticalThrustKnobHolder.SetActive(false);
            _verticalThrustKnob.SetActive(false);

            returnVal = Mathf.Sign(MPlayer().GetVerticalVelocity()) * -_verticalSpeedBleed;
        }

        return returnVal;
    }

    private float ProcessVerticalInputPC()
    {
        float returnVal = 0;

        int touchCount = Input.touchCount;
        Touch[] touches = Input.touches;


        if (Input.GetMouseButtonDown(0))
        {
            _touchPos = Input.mousePosition;

            _verticalThrustKnobHolder.transform.localPosition = _touchPos - _screenTouchOffset;
            _verticalThrustKnob.transform.position = Vector2.zero;


        }
        else if (Input.GetMouseButton(0) && _touchTimer > _tapTimeMax)
        {
            _verticalThrustKnobHolder.SetActive(true);
            _verticalThrustKnob.SetActive(true);

            Vector2 mainTouch = Input.mousePosition;

            float touchPosDelta = mainTouch.y - _touchPos.y;

            //Set the visual
            if (touchPosDelta > _verticalThrustKnobRange) touchPosDelta = _verticalThrustKnobRange;
            if (touchPosDelta < -_verticalThrustKnobRange) touchPosDelta = -_verticalThrustKnobRange;

            _verticalThrustKnob.transform.localPosition = (Vector2.right * touchPosDelta);

            //Movement calcs

            float moveDelta = touchPosDelta * _verticalMoveSpeed;

            moveDelta = Mathf.Clamp(moveDelta, -_verticalAccelCap, _verticalAccelCap);

            returnVal = moveDelta;
        }
        else
        {
            _verticalThrustKnobHolder.SetActive(false);
            _verticalThrustKnob.SetActive(false);

            float vertVelocity = MPlayer().GetVerticalVelocity();

            if(Mathf.Abs(vertVelocity) > _verticalSpeedHaltThreshold) returnVal = Mathf.Sign(vertVelocity) * -_verticalSpeedBleed;
            else
            {
                returnVal = 0;
                MPlayer().SetVerticalVelocity(0);
            }
        }

        return returnVal;
    }

#endregion

    private void CheckLanding()
    {
        float verticalDist = MPlayer().GetTrueAltitude();

        bool heightValid = MPlayer().GetTerrainDisplayValue() > _validLandingHeight;

        if (!MPlayer().GetLanded() && verticalDist < _landingWarningDist)
        {
            //If we can engage a land
            if (verticalDist < _landingDetectionDist)
            {
                Vector2 velocity = MPlayer().GetVelocity();
                float vertVelocity = MPlayer().GetVerticalVelocity();
                float maxSpd = MPlayer().GetMaxLandingSpeed();

                if (velocity.magnitude > maxSpd || vertVelocity > maxSpd)
                    FailedLand();
                else
                    Land();
            }
        }


    }

    private void FailedLand()
    {
        //We play the death particles and then bounce the balloon up
        //take damage here or something

        _landingDeathParticles.Play();
        //_balloonObj.SetActive(false);
        //_isDead = true;

        MPlayer().SetVerticalVelocity(_verticalSpeedCap);

        ClearInputUI();
    }

    private void Land()
    {
        MPlayer().SetVerticalVelocity(0);
        MPlayer().SetVelocity(Vector2.zero);
        MPlayer().SetLanded(true);

        SetLandingTethers(true);

        ClearInputUI();
    }

    private void Takeoff()
    {
        MPlayer().SetVerticalVelocity(_verticalSpeedCap/2f); //The full speed cap was too much :woozy:
        MPlayer().SetVelocity(Vector2.zero);
        MPlayer().SetLanded(false);

        SetLandingTethers(false);

        ClearInputUI();
    }

    private void SetLandingTethers(bool on)
    {
        if (on)
        {
            for (int i = 0; i < _landingTethers.Length; i++)
            {
                _landingTethers[i].SetActive(true);
                _landingTethers[i].transform.rotation = Quaternion.Euler(0, (90 * i) + (Random.value * 90), -Random.value * _maxTetherAngle);
            }
        }
        else
        {
            for (int i = 0; i < _landingTethers.Length; i++) _landingTethers[i].SetActive(false);
        }
    }

    private void ClearInputUI()
    {
        _verticalThrustKnobHolder.SetActive(false);
        _touchBase.SetActive(false);
        _touchTracker.SetActive(false);
    }

    private void ProcessRotation()
    {
        Vector2 velocity = MPlayer().GetVelocity();

        Quaternion newRot = Quaternion.AngleAxis(velocity.x * _rotationCoeff, Vector3.forward);
        newRot *= Quaternion.AngleAxis(-velocity.y * _rotationCoeff, Vector3.right);

        transform.rotation = newRot;
    }


}
