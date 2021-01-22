using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrainSetting", menuName = "Create New Terrain Setting")]
public class TerrainSetting : ScriptableObject
{
    public enum DetailSize
    {
        Small,
        Medium,
        Large
    }

    [SerializeField] private DetailSize _detailSize;
    [SerializeField] private FastNoise.NoiseType _type;
    [SerializeField] private FastNoise.FractalType _fracType;
    [SerializeField] private Vector2 _amplitudeRange = new Vector2(1,2); //After multiply amplitude
    [SerializeField] private Vector2 _frequencyRange = new Vector2(1, 2); //built in frequency setting
    [SerializeField] private Vector2Int _seedRange = new Vector2Int(1, 10000);

    public DetailSize GetDetailSize() { return _detailSize;  }

    public Noise GenerateNoiseSetting()
    {
        FastNoise.NoiseType t = _type;
        FastNoise.FractalType ft = _fracType;
        float a = Random.Range(_amplitudeRange.x, _amplitudeRange.y);
        float f = Random.Range(_frequencyRange.x, _frequencyRange.y);
        int s = Random.Range(_seedRange.x, _seedRange.y);

        return new Noise(t, ft, a/3, f, s);
    }
}
