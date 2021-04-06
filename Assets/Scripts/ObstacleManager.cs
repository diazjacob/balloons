using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : ManagedMonobehaviour
{
    private static float COMPARE_THRESH = 0.001f;
    private static float VIEW_BOUNDS = 6f;    //What radius is safley far away enough from the palyer to be out of view
    private static float BIRD_SPAWN_ADJUST = 0.1f; //how much to move positions when trying to make them not overlap;
    private static int SPAWN_ITERATION_MAX = 500;

    [Header("Spawn Settings: ")]
    [SerializeField] private float _spawnHeight;
    [SerializeField] private float _spawnHeightRange;
    [SerializeField] private float _maxSpawnDistance; //max distance to spawn from player
    [SerializeField] private float _spawnRecoupTime;

    private float _recoupTimer;

    [Space]
    [Header("Bird Pool: ")]
    [SerializeField] private int _birdPoolSize; //how many birds to pool
    [SerializeField] private GameObject _birdObj; //the prefab

    private WorldObject[] _birdPool; //the pool itself
    private int _birdsReady; //how many birds are ready to go

    [Space]
    [Header("Bird Settings:")]
    [SerializeField] private float _birdSize = 0.7f;
    [SerializeField] private Vector2 _birdSpawnTimeRange;

    private float _birdCurrSpawnTime;
    private float _birdSpawnTimer;

    [SerializeField] private Vector2Int _birdSpawnQuantityRange; //range of how many we can possibly spawn
    [SerializeField] private Vector2 _birdSpawnLateralRange; //the radius of the firs object
    [SerializeField] private Vector2 _birdMoveSpeedRange;
    [SerializeField] private float _birdSpawnVerticalRange; //How much to vary the height of the bird amoung a flock

    [Space]

    [Header("Enemy Balloon Pool: ")]
    [SerializeField] private int _balloonPoolSize;
    [SerializeField] private GameObject _balloonObj;

    private GameObject[] _balloonPool;
    private int _balloonsReady;

    [Space]

    [Header("Beacon Pool: ")]
    [SerializeField] private int _beaconPoolSize;
    [SerializeField] private GameObject _beaconObj;

    private WorldObject[] _beaconPool;
    private int _beaconsReady;

    [Space]

    [Header("Beacon Settings: ")]
    [SerializeField] private float _beaconSpawnVerticalPosDelta;
    [SerializeField] private float _beaconDecentRate;

    private void Awake()
    {
        GetAllRefs();
    }

    void Start()
    {
        InitalizeBirdPool();
        InitalizeBeaconPool();

        Player.OnLeaveWorld += ResetAllBirds;
        Player.OnLeaveWorld += ResetAllBeacons;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBirds();
    }


    #region BIRDS

    private void InitalizeBirdPool()
    {
        _birdPool = new WorldObject[_birdPoolSize];

        for (int i = 0; i < _birdPoolSize; i++)
        {
            GameObject obj = Instantiate(_birdObj, Player.OBJ_HOLD_POINT, Quaternion.identity);
            WorldObject worldObj = obj.GetComponent<WorldObject>();
            obj.GetComponentInChildren<MeshRenderer>().material.SetFloat("_Offset", Random.value);

            obj.transform.SetParent(transform);
            _birdPool[i] = worldObj;
        }
        _birdsReady = _birdPoolSize;

        _birdCurrSpawnTime = Random.Range(_birdSpawnTimeRange.x, _birdSpawnTimeRange.y);
    }

    private void UpdateBirds()
    {
        if (MPlayer().PlayerInWorld())
        {
            //RECOUP OBJECTS
            if (_recoupTimer > _spawnRecoupTime)
            {
                for (int i = 0; i < _birdPool.Length; i++)
                {
                    if ((_birdPool[i].IsInitalized() && _birdPool[i].DistToPlayer() > _maxSpawnDistance) || _birdPool[i].Cleanup())
                    {
                        _birdPool[i].Reset();
                        _birdsReady++;
                    }

                }
                _recoupTimer = 0;
            }
            _recoupTimer += Time.deltaTime;

            //BIRD TIMER
            if (_birdSpawnTimer > _birdCurrSpawnTime)
            {
                SpawnBirdGroup();
                _birdCurrSpawnTime = Random.Range(_birdSpawnTimeRange.x, _birdSpawnTimeRange.y);
                _birdSpawnTimer = 0;
            }
            _birdSpawnTimer += Time.deltaTime;
        }
    }

    private void ResetAllBirds()
    {
        for (int i = 0; i < _birdPool.Length; i++)
        {
            if (_birdPool[i].IsInitalized())
            {
                _birdPool[i].Reset();
                _birdsReady++;
            }
        }
    }

    private void SpawnBirdGroup()
    {
        //Calculations
        Vector2 playerPos = MPlayer().GetPosition(); //player pos

        Vector2 spawnOffset = Random.insideUnitCircle * VIEW_BOUNDS; //getting the random unit vector and upscaling it
        spawnOffset += spawnOffset.normalized * VIEW_BOUNDS; //Adding a gaurenteed extra view bound vector in the same direction
        spawnOffset = Vector2.ClampMagnitude(spawnOffset, _maxSpawnDistance); //clamping a max  magnatude

        spawnOffset += playerPos; //finalizing the positioning

        Vector2 moveDir = (playerPos - spawnOffset).normalized * Random.Range(_birdMoveSpeedRange.x, _birdMoveSpeedRange.y);
        float lookAngle = Vector2.SignedAngle(moveDir, -Vector2.up);

        //How many to spawn?
        int numberOfBirds = 0;
        if(_birdsReady >= _birdSpawnQuantityRange.x)
        {
            if (_birdsReady >= _birdSpawnQuantityRange.y) numberOfBirds = Random.Range(_birdSpawnQuantityRange.x, _birdSpawnQuantityRange.y + 1);
            else numberOfBirds = Random.Range(_birdSpawnQuantityRange.x, _birdsReady + 1);
        }

        //If we're able to spawn
        if(numberOfBirds > 0)
        {
            float spawnHeight = _spawnHeight + Random.Range(-_spawnHeightRange, _spawnHeightRange);

            Vector2[] birdLocations = new Vector2[numberOfBirds];
            float[] birdHeights = new float[numberOfBirds];

            for(int i = 0; i < numberOfBirds; i++)
            {
                birdHeights[i] = spawnHeight + Random.Range(-_birdSpawnVerticalRange, _birdSpawnVerticalRange);
            }

            for(int i = 0; i < numberOfBirds; i++)
            {
                birdLocations[i] = new Vector2(Random.Range(_birdSpawnLateralRange.x, _birdSpawnLateralRange.y),
                                               Random.Range(_birdSpawnLateralRange.x, _birdSpawnLateralRange.y));
                birdLocations[i] += spawnOffset;
            }


            //finalizing the positions with a while check
            bool tooClose = true;
            int iterations = 0;

            while (tooClose && iterations < SPAWN_ITERATION_MAX)
            {
                tooClose = false;
                int b1 = 0;
                int b2 = 0;

                for (int i = numberOfBirds - 1; i >= 0 && !tooClose; i--)
                {
                    for (int j = 0; j < i && !tooClose; j++)
                    {
                        tooClose = Vector2.Distance(birdLocations[i], birdLocations[j]) < _birdSize;
                        if(tooClose)
                        {
                            b1 = i;
                            b2 = j;
                        }
                    }
                }

                Vector2 dir = (birdLocations[b1] - birdLocations[b2]).normalized * BIRD_SPAWN_ADJUST;
                birdLocations[b1] += dir;
                birdLocations[b2] += -dir;

                iterations++;
            }


            for (int i = 0; i < numberOfBirds; i++)
            {
                int neededIndex = -1;

                for (int j = 0; j < _birdPool.Length && neededIndex == -1; j++)
                {
                    if (!_birdPool[j].IsInitalized())
                    {
                        neededIndex = j;
                    }
                }

                if(neededIndex != -1)
                {
                    _birdPool[neededIndex].InitalizeWorldPresense(birdLocations[i], birdHeights[i]);
                    _birdPool[neededIndex].InitalizeVelocity(moveDir);
                    _birdPool[neededIndex].SetVisualRotation(lookAngle);
                    _birdsReady--;
                }
            }
        }
    }

    #endregion


    #region BEACONS

    private void InitalizeBeaconPool()
    {
        _beaconPool = new WorldObject[_beaconPoolSize];

        for (int i = 0; i < _beaconPoolSize; i++)
        {
            GameObject obj = Instantiate(_beaconObj, Player.OBJ_HOLD_POINT, Quaternion.identity);
            WorldObject worldObj = obj.GetComponent<WorldObject>();

            obj.transform.SetParent(transform);
            _beaconPool[i] = worldObj;
        }
        _beaconsReady = _beaconPoolSize;
    }

    private void ResetAllBeacons()
    {
        for (int i = 0; i < _beaconPool.Length; i++)
        {
            if (_beaconPool[i].IsInitalized())
            {
                _beaconPool[i].Reset();
                _beaconsReady++;
            }
        }
    }

    [ContextMenu("Spawn Beacon")]
    public void SendBeacon()
    {
        int index = -1;

        if(_beaconsReady > 0)
        {
            for (int i = 0; i < _beaconPool.Length && index == -1; i++)
            {
                if (!_beaconPool[i].IsInitalized()) index = i;
            }

            if (index != -1)
            {
                _beaconsReady--;

                float vertSpeed = _beaconDecentRate + Mathf.Clamp(MPlayer().GetVerticalVelocity(), -1, 0);

                _beaconPool[index].InitalizeWorldPresense(MPlayer().GetPosition(), MPlayer().GetVerticalPosition() + _beaconSpawnVerticalPosDelta);
                _beaconPool[index].InitalizeVelocity(MPlayer().GetVelocity(), vertSpeed);
            }
        }
        else Debug.LogError("ERROR: Could not spawn beacon.");
        
    }

    public void SendMailBeacon(Mail mail)
    {
        int index = -1;

        if (_beaconsReady > 0)
        {
            for (int i = 0; i < _beaconPool.Length && index == -1; i++)
            {
                if (!_beaconPool[i].IsInitalized()) index = i;
            }

            if (index != -1)
            {
                _beaconsReady--;

                float vertSpeed = _beaconDecentRate + Mathf.Clamp(MPlayer().GetVerticalVelocity(), -1, 0);

                _beaconPool[index].InitalizeWorldPresense(MPlayer().GetPosition(), MPlayer().GetVerticalPosition() + _beaconSpawnVerticalPosDelta);
                _beaconPool[index].InitalizeVelocity(MPlayer().GetVelocity(), vertSpeed);

                Beacon controller = _beaconPool[index].GetComponent<Beacon>();

                controller.SetMail(mail);
            }
        }
        else Debug.LogError("ERROR: Could not spawn beacon.");

    }


    #endregion


}
