using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelController : ManagedMonobehaviour
{
    [Header("Level Settings: ")]
    public static float PLANESIZE = 10;
    [SerializeField] private Material _levelMat;

    [SerializeField] private GameObject _levelPrefab;
    [SerializeField] private float _levelHeight;
    [SerializeField] private float _levelHeightCoefficent;

    [SerializeField] private int _startLevelCutoff = 0;

    [Space]
    [SerializeField] private int _levelNum = 9;
    [SerializeField] private int _resolution = 60;
    [Space]

    [Header("Noise Settings: ")]
    [SerializeField] private float _noiseStartPos;
    [SerializeField] private float _noiseHeightCoeff;
    [SerializeField] private float _whiteNoiseCoeff;
    [SerializeField] private float _whiteNoiseMaxPos;
    [SerializeField] private float _noiseSpeedCoeff;
    private FastNoise _whiteNoise = new FastNoise();


    private float[][] _heightMap;
    private Texture2D _heightMapTex;

    private GameObject[] _levelObjs;
    private Level[] _levels;

    private void Awake()
    {
        GetAllRefs();
    }

    private void Start()
    {
        //Set white noise
        _whiteNoise.SetFrequency(100);

        //Make the heightmap & settings
        _heightMap = new float[_resolution][];
        for (int i = 0; i < _resolution; i++) _heightMap[i] = new float[_resolution];

        _heightMapTex = new Texture2D(_heightMap.Length, _heightMap.Length);

        _heightMapTex.filterMode = FilterMode.Bilinear;
        _heightMapTex.wrapMode = TextureWrapMode.Clamp;
        _heightMapTex.anisoLevel = 3;

        //Create the objects
        _levelObjs = new GameObject[_levelNum];
        _levels = new Level[_levelNum];

        for (int i = _startLevelCutoff; i < _levelNum; i++)
        {
            _levelObjs[i] = Instantiate(_levelPrefab, transform.position + Vector3.up * i * _levelHeight, Quaternion.identity);
            _levelObjs[i].transform.SetParent(transform);

            _levels[i] = (_levelObjs[i].GetComponent<Level>());

            _levels[i].SetMat(new Material(_levelMat));

            _levels[i].SetHeight(i * _levelHeight);
        }
    }

    private void FixedUpdate()
    {
        if(MPlayer().PlayerInWorld())
        {
            ProcessMap();
            ApplyMap();
        }
    }

    private void ProcessMap()
    {
        //10 because the plane has dimentions 10x10
        float sect = 1f / _resolution * PLANESIZE;

        Vector2 position = MPlayer().GetPosition();

        for (int i = 0; i < _resolution; i++)
        {
            for (int j = 0; j < _resolution; j++)
            {
                float x = i * sect + position.x - PLANESIZE/2;
                float y = j * sect + position.y - PLANESIZE/2;

                float val = MPlayer().GetTerrainValue(x, y);

                val += WhiteNoiseAddOn(x, y);

                //TESTING

                //float localX = i;// x - position.x;
                //float localY = j;// y - position.y;

                //float buff = 5;

                //if (localX < buff) val -= (buff - localX) / 1.5f;
                //if (localY < buff) val -= (buff - localY) / 1.5f;
                //if (localX > _resolution - buff) val -= (localX - (_resolution - buff)) / 1.5f;
                //if (localY > _resolution - buff) val -= (localY - (_resolution - buff)) / 1.5f;

                //TESTING

                _heightMapTex.SetPixel(i, j, new Color(val, val, val, val));

                _heightMap[i][j] = val;
            }
        }

        _heightMapTex.Apply();
    }

    private void ApplyMap()
    {
        PaletteSetting p = MPlayer().GetPalette();

        Camera.main.backgroundColor = p.GetBaseColor();
        for (int i = _startLevelCutoff; i < _levelNum; i++)
        {
            _levels[i].SetColor(p.GetColor(i));
            _levels[i].SetHeightMap(_heightMapTex);
            _levels[i].SetHeight(i * _levelHeight * _levelHeightCoefficent);

            _levelObjs[i].transform.position = transform.position + Vector3.up * i * _levelHeight;
        }
    }

    private float WhiteNoiseAddOn(float x, float y)
    {
        float result = 0;

        float vertPos = MPlayer().GetVerticalPosition();

        if(vertPos > _noiseStartPos)
        {
            float delta = vertPos - _noiseStartPos;
            result += _whiteNoise.GetPerlin(x/ _noiseSpeedCoeff, y/ _noiseSpeedCoeff) * delta * _noiseHeightCoeff;

            float diff = _whiteNoiseMaxPos - _noiseStartPos;
            delta = Mathf.Clamp01(delta / diff);

            result += _whiteNoise.GetWhiteNoise(x + Time.deltaTime, y + Time.deltaTime) * _whiteNoiseCoeff * delta;
        }

        return result;
    }


}


