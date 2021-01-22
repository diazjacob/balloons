using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindController : ManagedMonobehaviour
{
    private static float THRESHOLD = 0.01f;

    [Header("Wind Settings: ")]
    [SerializeField] private float _windForce;
    [SerializeField] private float _forceHeightMax;
    [Space]
    [SerializeField] private Vector2 _windAngleInterval;
    [SerializeField] private Vector2 _windSpeedInterval;
    //[SerializeField] private Vector2 _windNoiseUpdateInterval;
    [Space]
    [SerializeField] private Vector2 _windSpeeds;
    [Space]
    [SerializeField] private float _windAngleLerp;
    [SerializeField] private float _windSpeedLerp;
    [Space]
    [SerializeField] private float _windEmissionCoeff;
    [SerializeField] private int _windEmissionConstant;
    [SerializeField] private float _windNoiseCoeff;
    [SerializeField] private bool _windTimeProportionalToSpeed;

    private float _currAngleInterval; //Time to update
    private float _currSpeedInterval; //Time to update

    private float _currWindSpeed;  //current wind speed
    private float _currentWindDir; //0-360 angle

    private float _windSpeedTarget;

    private float _angleTimer = 0;
    private float _speedTimer = 0;


    private ParticleSystem _windParticles;

    public Vector2 GetForce()
    {
        return new Vector2(transform.forward.x, transform.forward.z) * -_windForce * _currWindSpeed;
    }
    public Quaternion GetRotation()
    {
        return transform.rotation;
    }


    private void Awake()
    {
        GetAllRefs();

        //Latch onto palette update
        Player.OnPaletteUpdate += ColorUpdate;
    }

    private void Start()
    {
        _windParticles = GetComponentInChildren<ParticleSystem>();

        Initalize();
        ColorUpdate();
    }

    [ContextMenu("Randomize Wind")]
    private void Initalize()
    {

        //Starting the wind, generating all random values, grabbing particle data

        _windSpeedTarget = Random.Range(_windSpeeds.x, _windSpeeds.y);
        _currSpeedInterval = Random.Range(_windSpeedInterval.x, _windSpeedInterval.y);
        _currWindSpeed = _windSpeedTarget;

        _currentWindDir = 360 * Random.value;
        transform.rotation = Quaternion.Euler(0, _currentWindDir, 0);
        _currAngleInterval = Random.Range(_windAngleInterval.x, _windAngleInterval.y);

        var main = _windParticles.main;
        main.startSpeed = _currWindSpeed;

        var emis = _windParticles.emission;
        emis.rateOverTime = _currWindSpeed * _windEmissionCoeff + _windEmissionConstant;

        _windParticles.Play();
    }

    private void Update()
    {
        if(MPlayer().PlayerInWorld())
        {
            AngleUpdate();
            SpeedUpdate();
            NoiseUpdate();

            if (!MPlayer().GetLanded()) ApplyForce();
        }
        else
        {
            var emis = _windParticles.emission;
            emis.rateOverTime = 0;
            _windParticles.Clear();
        }
    }

    private void ApplyForce()
    {
        if (_forceHeightMax > MPlayer().GetVerticalPosition())
            MPlayer().AddVelocity(new Vector2(transform.forward.x, transform.forward.z) * -_windForce * _currWindSpeed);
    }



    private void AngleUpdate()
    {
        Quaternion target = Quaternion.Euler(0, _currentWindDir, 0);

        if (Quaternion.Angle(transform.rotation, target) > THRESHOLD) //if we lerp
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * _windAngleLerp);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0); //clamp other angles
        }

        if (_angleTimer > _currAngleInterval)
        {
            _currentWindDir = 360 * Random.value;

            _currAngleInterval = Random.Range(_windAngleInterval.x, _windAngleInterval.y);
            _angleTimer = 0;
        }

        _angleTimer += Time.deltaTime;
    }

    private void SpeedUpdate()
    {
        if(Mathf.Abs(_currWindSpeed - _windSpeedTarget) > THRESHOLD)
        {
            _currWindSpeed -= Mathf.Sign(_currWindSpeed - _windSpeedTarget) * _windSpeedLerp * Time.deltaTime;

            var main = _windParticles.main;
            main.startSpeed = _currWindSpeed;

            var emis = _windParticles.emission;
            emis.rateOverTime = _currWindSpeed * _windEmissionCoeff + _windEmissionConstant;
        }


        if(_speedTimer > _currSpeedInterval)
        {
            _windSpeedTarget = Random.Range(_windSpeeds.x, _windSpeeds.y);

            _currSpeedInterval = Random.Range(_windSpeedInterval.x, _windSpeedInterval.y);
            if(_windTimeProportionalToSpeed) _currSpeedInterval /= _windSpeedTarget;

            _speedTimer = 0;
        }

        _speedTimer += Time.deltaTime;
    }

    private void NoiseUpdate()
    {
        float newNoise = _windNoiseCoeff / _currWindSpeed;
        var noise = _windParticles.noise;
        noise.strength = newNoise;
    }

    private void ColorUpdate()
    {
        var m = _windParticles.GetComponent<ParticleSystemRenderer>();
        m.trailMaterial.color = MPlayer().GetPalette().GetHighlightColor();
    }
}
