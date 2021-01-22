using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class POIManager : ManagedMonobehaviour
{
    [Header("POI Settings: ")]
    [SerializeField] private ObjMultiPool _housePool;
    [SerializeField] private ObjPool _exclimationPool;
    [SerializeField] private ObjPool _shopPool;

    private Vector2 _shopPosition;

    public Vector2 GetShopPosition() { return _shopPosition; }

    private void Awake()
    {
        GetAllRefs();
    }

    void Start()
    {
        _housePool.Initalize(transform);
        _exclimationPool.Initalize(transform);
        _shopPool.Initalize(transform);

        Player.OnLeaveWorld += ResetAll;
    }

    public void ResetAll()
    {
        _housePool.Reset();
        _exclimationPool.Reset();
        _shopPool.Reset();
    }

    //This INTERNAL function then sets the objects directley from the contract POIs
    public void InitalizeWorldObjects()
    {
        //General initalization

        ResetAll();

        Vector2 pos = MPlayer().GetContractFirstPOI();
        int incrementor = 0;
        bool isValid = true;


        //Spawn the shop
        isValid = _shopPool.Assign(pos);
        _shopPosition = pos;
        pos = MPlayer().GetContractNextPOI();
        incrementor++;

        //Then just fill up the rest with houses.
        while (pos != Vector2.zero && isValid)
        {
            isValid = _housePool.Assign(pos);
            

            pos = MPlayer().GetContractNextPOI();
            incrementor++;
        }
    }
}

[Serializable]
public class ObjPool
{
    [SerializeField] private GameObject _objPrefab;
    [SerializeField] private int _objsToSpawn;

    private int _numReady;

    private WorldObject[] _pool;

    public ObjPool(GameObject prefab, int objsToSpawn, Transform initTrans)
    {
        _objPrefab = prefab;
        _objsToSpawn = objsToSpawn;

        Initalize(initTrans);
    }

    public void Initalize(Transform parent)
    {
        _pool = new WorldObject[_objsToSpawn];

        for (int i = 0; i < _objsToSpawn; i++)
        {

            GameObject obj = GameObject.Instantiate(_objPrefab, Player.OBJ_HOLD_POINT, Quaternion.identity) as GameObject;
            WorldObject worldObj = obj.GetComponent<WorldObject>();

            obj.transform.SetParent(parent);
            _pool[i] = worldObj;
        }
    }

    public void Reset()
    {
        for (int i = 0; i < _pool.Length; i++)
        {
            _pool[i].Reset();
        }
    }

    public bool IsAvailable()
    {
        bool result = false;

        for (int i = 0; i < _pool.Length; i++)
        {
            if (!_pool[i].IsInitalized())
            {
                result = true;
            }
        }

        return result;
    }

    public bool Assign(Vector2 pos)
    {
        bool result = false;

        WorldObject obj = null;
        for(int i = 0; i < _pool.Length; i++)
        {
            if(!_pool[i].IsInitalized())
            {
                obj = _pool[i];
            }
        }

        if(obj != null)
        {
            obj.InitalizeWorldPresense(pos);
            result = true;
        }
        else
        {
            Debug.LogError("ERROR: POIManager ObjPool was not able to fufill a spawn request");
        }

        return result;
    }
}

[Serializable]
public class ObjMultiPool
{
    [SerializeField] private GameObject[] _poolObjs;
    [SerializeField] private int _objsToSpawn;

    private ObjPool[] _objPools;

    public void Initalize(Transform parent)
    {

        _objPools = new ObjPool[_poolObjs.Length];

        for (int i = 0; i < _poolObjs.Length; i++)
        {
            _objPools[i] = new ObjPool(_poolObjs[i], _objsToSpawn, parent);
        }
    }

    public void Reset()
    {
        for (int i = 0; i < _objPools.Length; i++)
        {
            _objPools[i].Reset();
        }
    }

    public bool Assign(Vector2 pos)
    {
        bool result = false;

        int poolNum = Random.Range(0, _objPools.Length);
        int counter = 0;

        do
        {
            result = _objPools[poolNum].Assign(pos);

            poolNum++;
            poolNum %= _objPools.Length;

            counter++;
        }
        while (!result && counter < _objPools.Length);


        return result;
    }
}