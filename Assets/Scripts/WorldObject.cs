using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : ManagedMonobehaviour
{
    [SerializeField] private GameObject _visuals;       //The visual object
    [SerializeField] private GameObject _indicator;     //The flight indicator
    [Space]
    [SerializeField] private bool _visualsColoredToBase;    //If the visual object's colors are colored to the palette base color
    [Space]
    [SerializeField] private bool _indicatorColoredToBase;  //If the indicatr's object colors are colored to the palette base color
    [Space]
    [Header("Living Settings: (Leave Blank If Building Or Something)")]
    [SerializeField] private bool _influencedByWind;
    [SerializeField] private float _windInfluenceMultiplier = 0.1f;
    [SerializeField] private int _damageAmount = 0;     //The amount of damamge upon collision

    //The collision particle system
    [SerializeField] private GameObject _collisionParticleObj;
    private ParticleSystem _collisionParticle;


    private Vector2 _position;  //Object position
    private float _verticalPos; //Object position

    private Vector2 _moveDir;   //The move direction (here magnatude denotes speed, it matters)
    private float _verticalVel = 0; //The vetical velocity

    private bool _isInitalized = false; //If the object is in the world
    private bool _isMoving = false; //If the object moves;
    private bool _inAir = false;    //If the object is flying

    private bool _cleanup = false;  //If the object is in the world but needs to be cleaned by a manager.

    //Getters
    public bool IsInitalized() { return _isInitalized; }
    public bool Cleanup() { return _cleanup; }
    public Vector2 GetPosition() { return _position; } 
    public void SetIsMoving(bool newMoving) { _isMoving = newMoving; }

    private void Awake()
    {
        GetAllRefs();
    }

    private void Start()
    {
        Player.OnPaletteUpdate += UpdateColors;
        UpdateColors();

        transform.position = Player.OBJ_HOLD_POINT;

        if (_collisionParticleObj != null)
        {
            _collisionParticle = _collisionParticleObj.GetComponent<ParticleSystem>();
            var main = _collisionParticle.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }
    }

    private void Update()
    {
        if (_isInitalized && !MPlayer().GetGamePaused())
        {
            //Velocity addition
            if (_isMoving)
            {
                _position += _moveDir * Time.deltaTime;
                _verticalPos += _verticalVel * Time.deltaTime;
                if (_influencedByWind) _moveDir += (MWindController().GetForce() * _windInfluenceMultiplier);
            }

            //Set position
            Vector2 worldPos = MPlayer().GetPlayerReletivePosition(_position);
            transform.position = new Vector3(worldPos.x, _verticalPos, worldPos.y);

            //Set indicator
            if (_indicator != null && _inAir)
            {
                float verticalPos = MPlayer().GetTerrainDisplayValue(_position.x, _position.y);
                _indicator.transform.position = new Vector3(transform.position.x, verticalPos, transform.position.z);
            }
        }
    }

    [ContextMenu("Reset Object")]
    public void Reset() //Resetting everything
    {
        HouseControl house = GetComponentInChildren<HouseControl>();
        if (house != null) house.Reset();


        transform.position = Player.OBJ_HOLD_POINT;
        _isMoving = false;
        _isInitalized = false;
        _inAir = false;
        _cleanup = false;

        _position = Vector2.zero;
        _verticalPos = 0;
        _moveDir = Vector2.zero;

        if (_visuals != null) _visuals.transform.rotation = Quaternion.identity;
        if (_indicator != null) _indicator.transform.position = Player.OBJ_HOLD_POINT;
    }

    //Set the rotation of the object
    public void SetVisualRotation(float rot)
    {
        if(_visuals != null)
            _visuals.transform.eulerAngles = new Vector3(_visuals.transform.eulerAngles.x, rot, _visuals.transform.eulerAngles.z);
    }

    //Initalize in the world (for initalizing the position)
    public void InitalizeWorldPresense(Vector2 position, float vert = 0)
    {
        HouseControl house = GetComponentInChildren<HouseControl>();
        if (house != null) house.Initalize();

        _position = position;
        transform.rotation = Quaternion.Euler(0, Random.value * 360, 0);

        if (vert == 0)
        {
            _verticalPos = MPlayer().GetTerrainDisplayValue(_position.x, _position.y);
            _inAir = false;
        }
        else
        {
            _verticalPos = vert;
            _inAir = true;
        }

        _isInitalized = true;
    }

    //Initalizing the movement
    public void InitalizeVelocity(Vector2 dir, float vel = 0)
    {
        _moveDir = dir;
        _verticalVel = vel;
        _isMoving = true;
    }

    //Visuals
    private void UpdateColors() 
    {
        //Indicator Color
        if (_visuals != null)
        {
            var meshRend = _visuals.GetComponent<MeshRenderer>();

            if(meshRend != null)
            {
                if (_visualsColoredToBase) meshRend.material.color = MPlayer().GetPalette().GetBaseColor();
                else meshRend.material.color = MPlayer().GetPalette().GetHighlightColor();
            }
            else
            {
                for(int i = 0; i < _visuals.transform.childCount; i++)
                {
                    meshRend = _visuals.transform.GetChild(i).GetComponent<MeshRenderer>();

                    if(meshRend != null)
                    {
                        if (_visualsColoredToBase) meshRend.material.color = MPlayer().GetPalette().GetBaseColor();
                        else meshRend.material.color = MPlayer().GetPalette().GetHighlightColor();
                    }

                }
            }

        }

        //Indicator color
        if (_indicator != null)
        {
            if(_indicatorColoredToBase) _indicator.GetComponent<MeshRenderer>().material.color = MPlayer().GetPalette().GetBaseColor();
            else _indicator.GetComponent<MeshRenderer>().material.color = MPlayer().GetPalette().GetHighlightColor();
        }
        

    }

    //A helper function
    public float DistToPlayer()
    {
        return Vector2.Distance(_position, MPlayer().GetPosition());
    }

    //When we collide
    public void PlayerCollision()
    {
        //Playing the particles
        if (_collisionParticle != null)
        {
            _collisionParticle.transform.parent = transform.parent;
            _collisionParticle.transform.position = transform.position;
            _collisionParticle.Play();
        }

        Reset();

        //When we collide we have to stick around for a couple seconds so we have this secondary
        //Cleanup variable to make sure a collided obj is cleaned.
        _cleanup = true;
    }
}
