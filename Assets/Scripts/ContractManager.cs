using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ContractManager : ManagedMonobehaviour
{
    private static int CONTRACT_TYPE_NUM = 4;

    [Header("POI Settings:")]
    [SerializeField] private int _maxPointsOfInterest = 30;     //how many points of interest to generate
    [SerializeField] private int _maxPOIIterations = 5000;      //maximum point of interest generation random tries
    [SerializeField] private float _minimumPointSpacing = 1f;       //minimum point world-space spacing value
    [SerializeField] private float _minimumPointEdgeDistance = .2f;         //how close to edges points can get
    [SerializeField] private float _pointFindRadius = 8f;           //the radius to find points in
    [SerializeField] private float _pointVerticalThreshold = 0.95f;          //How high up vertically the points must be on a 0-1 scale
    [SerializeField] private float _pointThresholdDropdown = .05f;          //How fast the valid point threshold drops to find more points
    [Space]
    [SerializeField] private int _maxTargetingIterations = 50;      //maximum target generation random tries
    [SerializeField] [Range(0, .95f)] private float _targetingFracMin = 0.5f;

    [Header("EXPOSED VALUES. DONT CHANGE")]
    //THE TERRAIN
    [SerializeField] private Contract[] _possibleContracts;
    private Contract _currentContract;

    private void Awake()
    {
        GetAllRefs();
    }


    #region GET_SET

    public Vector2 GetFirstPOI()
    {
        Vector2 result = Vector2.one;
        if (_currentContract != null)
        {
            result = _currentContract.GetFirstPOI();
        }
        else Debug.LogError("ERROR: Could not get current contract's first POI because it is null");

        return result;
    }
    public Vector2 GetNextPOI()
    {
        Vector2 result = Vector2.one;
        if (_currentContract != null)
        {
            result = _currentContract.GetNextPOI();
        }
        else Debug.LogError("ERROR: Could not get current contract's next POI because it is null");

        return result;
    }
    public float GetCurrentTerrain(float x, float y)
    {
        float result = -1;
        if (_currentContract != null)
        {
            result = _currentContract.GetTerrain(x, y);
        }
        else Debug.LogError("ERROR: Could not get current contract's terrain because it is null");

        return result;
    }

    public Vector2 GetTargetPos(Vector2 sourcePos)
    {
        Vector2 result = Vector2.zero;

        if (_currentContract != null)
        {
            Vector2[] a = _currentContract.GetPOIArray();

            int iteration = 0;
            int val = 0;

            do
            {
                val = Random.Range(0, a.Length);

                if (Vector2.Distance(sourcePos, a[val]) > _targetingFracMin * _pointFindRadius) break;

                iteration++;
            }
            while (iteration < a.Length);
            

            result = a[val];
        }
        else Debug.LogError("Could not find an appropriate mail target.");


        return result;
    }

    public void SelectContract(int index)
    {
        if (index >= 0 && index < _possibleContracts.Length)
        {
            _currentContract = _possibleContracts[index];
        }
    }

    #endregion


    #region GENERATE

    //IN THE FUTURE, SUPPLY THE PLAYER STATS AND GENERATE CONTRACTS IN A SMARTER WAY
    public void Generate(int num)
    {
        _possibleContracts = new Contract[num];
        for(int i = 0; i < _possibleContracts.Length; i++)
        {
            _possibleContracts[i] = GenerateNewContract();
        }
    }

    private Contract GenerateNewContract()
    {
        Contract newContract = new Contract();

        TerrainSetting[] settings = Resources.LoadAll<TerrainSetting>("Terrain/");

        List<int> smalls = new List<int>();
        List<int> meds = new List<int>();
        List<int> bigs = new List<int>();

        for(int i = 0; i < settings.Length; i++)
        {
            switch(settings[i].GetDetailSize())
            {
                case TerrainSetting.DetailSize.Small:
                    smalls.Add(i);
                    break;
                case TerrainSetting.DetailSize.Medium:
                    meds.Add(i);
                    break;
                case TerrainSetting.DetailSize.Large:
                    bigs.Add(i);
                    break;
            }
        }

        int smallChoice = Random.Range(0, smalls.Count);
        int medChoice = Random.Range(0, meds.Count);
        int bigChoice = Random.Range(0, bigs.Count);

        Noise[] noises = new Noise[3];

        noises[0] = settings[smalls[smallChoice]].GenerateNoiseSetting();
        noises[1] = settings[meds[medChoice]].GenerateNoiseSetting();
        noises[2] = settings[bigs[bigChoice]].GenerateNoiseSetting();

        newContract.SetNoiseProfile(noises);

        GeneratePointsOfInterest(newContract, _pointFindRadius);

        return newContract;
    }

    private void GeneratePointsOfInterest(Contract c, float radius)
    {
        Vector2 playerPos = MPlayer().GetPosition();

        int iterations = 0;

        float testPoint;
        Vector2 point;
        bool tooClose;

        List<Vector2> validPoints = new List<Vector2>();
        float threshold = _pointVerticalThreshold;

        while(validPoints.Count < _maxPointsOfInterest && threshold > 0)
        {
            do
            {
                //get point
                point = Random.insideUnitCircle * radius + playerPos;
                testPoint = c.GetTerrain(point.x, point.y);

                tooClose = false;
                for (int i = 0; i < validPoints.Count && !tooClose; i++) tooClose = Vector2.Distance(validPoints[i], point) < _minimumPointSpacing;

                if (testPoint > threshold && !tooClose)
                {
                    //If we're just about ready to apply, check nearby points to see if we're not on an edge
                    bool isOnTop = false;

                    float p1 = c.GetTerrain(point.x + _minimumPointEdgeDistance, point.y + _minimumPointEdgeDistance);
                    float p2 = c.GetTerrain(point.x + _minimumPointEdgeDistance, point.y - _minimumPointEdgeDistance);
                    float p3 = c.GetTerrain(point.x - _minimumPointEdgeDistance, point.y - _minimumPointEdgeDistance);
                    float p4 = c.GetTerrain(point.x - _minimumPointEdgeDistance, point.y + _minimumPointEdgeDistance);

                    isOnTop = p1 > threshold && p2 > threshold && p3 > threshold && p4 > threshold;

                    if (isOnTop) validPoints.Add(point);
                }

                iterations++;
            }
            while (iterations < _maxPOIIterations && validPoints.Count < _maxPointsOfInterest);

            threshold -= _pointThresholdDropdown;
            iterations = 0;
        }



        //set it here
        c.SetPOIs(validPoints.ToArray());
    }

    #endregion


}

[Serializable]
public class Contract //A contract with data and a profile of noise generators ! :))
{
    //TERRAIN
    [SerializeField] private NoiseProfile _terrain; //The terrain of the contract
    [SerializeField] private Vector2[] _pointsOfInterest;

    //INTERNAL WORKINGS
    private int _currentIterator;


    public Contract()
    {
        _terrain = new NoiseProfile();
    }

    //THE SETTERS
    public void SetNoiseProfile(Noise[] noises) { _terrain.SetNoiseProfile(noises); }
    public void SetPOIs(Vector2[] POIs) { _pointsOfInterest = POIs; }

    //THE GETTERS
    public float GetTerrain(float x, float y) { return _terrain.GetNoise(x, y); }

    public int getPOILength() { return _pointsOfInterest.Length; }

    public Vector2 GetNextPOI()
    {
        Vector2 result = Vector2.zero;
        _currentIterator++;

        if (_currentIterator >= 0 && _currentIterator < _pointsOfInterest.Length)
        {
            result = _pointsOfInterest[_currentIterator];

        }

        return result;
    }
    public Vector2 GetFirstPOI()
    {
        Vector2 result = Vector2.zero;
        _currentIterator = 0;

        if (_pointsOfInterest.Length > 0) result = _pointsOfInterest[_currentIterator];

        return result;
    }

    public Vector2[] GetPOIArray()
    {
        Vector2[] copy = new Vector2[_pointsOfInterest.Length];

        for(int i = 0; i < copy.Length; i++)
        {
            copy[i] = _pointsOfInterest[i];
        }


        return copy;
    }
}

[Serializable]
public class NoiseProfile //A full collection of noise generators
{
    [SerializeField] private float _verticalOffset;

    [SerializeField] private Noise[] _noises;

    public void SetNoiseProfile(Noise[] n)
    {
        _verticalOffset = 0;
        _noises = n;
    }

    public float GetNoise(float x, float y)
    {
        float total = 0;
        for (int i = 0; i < _noises.Length; i++)
        {
            total += (_noises[i].GetNoise(x, y));
        }

        total += _verticalOffset;

        return total;
    }
}

[Serializable]
public class Noise //A single noise tex generator
{
    [SerializeField] private FastNoise _profile;

    [SerializeField] private FastNoise.NoiseType _type;
    [SerializeField] private FastNoise.FractalType _fracType;
    [SerializeField] private float _amplitude; //After multiply amplitude
    [SerializeField] private float _frequency; //built in frequency setting
    [SerializeField] private int seed; // :)

    public Noise(FastNoise.NoiseType type, FastNoise.FractalType fracType, float amp, float freq, int s)
    {
        _type = type;
        _fracType = fracType;
        _amplitude = amp;
        _frequency = freq;
        seed = s;

        _profile = new FastNoise(seed);
        _profile.SetFractalType(_fracType);
        _profile.SetNoiseType(_type);
        _profile.SetFrequency(freq);
    }

    public float GetNoise(float x, float y)
    {
        return (_profile.GetNoise(x, y) * _amplitude);
    }
}
