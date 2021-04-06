using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPaletteSetting", menuName = "Create New Palette Setting")]
public class PaletteSetting : ScriptableObject
{
    [Header("Palette Settings: ")]
    [SerializeField] private string _colorCSV;
    private string _confirmedColorCSV;

    [SerializeField] private Color[] _colors;
    private Color[] _confirmedColors;

    [Header("Generate Settings: ")]
    [SerializeField] private int _generateSize;
    [SerializeField] private float _generateMin;

    [Space]

    [SerializeField] private float _generateMonoDeltaCoeff;
    [SerializeField] private float _generateCompliDeltaCoeff;

    //UPDATING AND RETRIVEING COLOR
    private void OnValidate()
    {
        if (_colors == null || _confirmedColors == null || _colors.Length == 0)
        {
            _colors = new Color[1];
            _confirmedColors = new Color[1];
        }

        if(!ColorsEqual())
        {
            _confirmedColors = DeepCopy(_colors);
            _confirmedColorCSV = ColorArrayToString(_confirmedColors);
            _colorCSV = _confirmedColorCSV;
        }
        else if(!_colorCSV.Equals(_confirmedColorCSV))
        {
            _confirmedColors = ParseCSV(_colorCSV);
            _colors = DeepCopy(_confirmedColors);
            _confirmedColorCSV = _colorCSV;
        }

    }


    #region RETRIVING_COLOR

    public Color GetBaseColor()
    {
        if (_confirmedColors == null) OnValidate();

        Color result = Color.Lerp(_confirmedColors[0] / 2, Color.black, .5f);
        return result;
    }

    public Color GetHighlightColor()
    {
        if (_confirmedColors == null) OnValidate();

        Color result = Color.Lerp(_confirmedColors[_confirmedColors.Length - 1], Color.white, .5f);
        return result;
    }

    public Color GetBottomColor()
    {
        Color result = Color.black;
        if (_confirmedColors == null) OnValidate();

        if (_confirmedColors != null && _confirmedColors.Length > 0) result = _confirmedColors[0];
        else Debug.LogError("Could not grab palette bottom color from empty color array");

        return result;
    }

    public Color GetTopColor()
    {
        Color result = Color.black;
        if (_confirmedColors == null) OnValidate();

        if (_confirmedColors != null && _confirmedColors.Length > 0) result = _confirmedColors[_confirmedColors.Length - 1];
        else Debug.LogError("Could not grab palette bottom color from empty color array");

        return result;
    }

    public Color GetMidColor()
    {
        Color result = Color.black;
        if (_confirmedColors == null) OnValidate();

        if (_confirmedColors != null && _confirmedColors.Length > 0)
        {
            int mid = _confirmedColors.Length / 2;
            result = _confirmedColors[mid];
        }
        else Debug.LogError("Could not grab palette bottom color from empty color array");

        return result;
    }

    public Color GetColor(int i)
    {
        Color returnCol = Color.white;
        if (_confirmedColors == null || i > _confirmedColors.Length-1) OnValidate();
        
        if(i > -1 && i < _confirmedColors.Length) returnCol = _confirmedColors[i];

        return returnCol;
    }

    public Color GetRandomColor()
    {
        Color returnCol = Color.white;
        int rand = Random.Range(0, _confirmedColors.Length);

        return _confirmedColors[rand];
    }

    #endregion


    #region GENERATION

    //GENERATING COLORS
    [ContextMenu("Generate (Monochromatic)")]
    private void GenerateMonoChromatic()
    {
        Color c = Random.ColorHSV();

        float delta = Random.Range(_generateMin, _generateMonoDeltaCoeff / _generateSize);

        _confirmedColors = new Color[_generateSize];
        for(int i  = 0; i < _generateSize; i++)
        {
            _confirmedColors[i] = c;
            c = new Color(c.r + delta, c.g + delta, c.b + delta, c.a + delta);
        }

        _colors = DeepCopy(_confirmedColors);
        _confirmedColorCSV = ColorArrayToString(_confirmedColors);
        _colorCSV = _confirmedColorCSV;
    }

    [ContextMenu("Generate (Monochromatic Preserve Base Color)")]
    private void GenerateMonoChromaticPreserved()
    {
        Color c = _confirmedColors[0];

        float delta = Random.Range(_generateMin, _generateMonoDeltaCoeff / _generateSize);

        _confirmedColors = new Color[_generateSize];
        for (int i = 0; i < _generateSize; i++)
        {
            c.a = 1;

            _confirmedColors[i] = c;
            c = new Color(c.r + delta, c.g + delta, c.b + delta, c.a + delta);
        }

        _colors = DeepCopy(_confirmedColors);
        _confirmedColorCSV = ColorArrayToString(_confirmedColors);
        _colorCSV = _confirmedColorCSV;
    }

    [ContextMenu("Generate (Complementary)")]
    private void GenerateComplementary()
    {
        Color c1 = Random.ColorHSV();
        Color c2 = Color.white - c1;

        Color c = c1;

        float delta = Random.Range(_generateMin, _generateCompliDeltaCoeff / _generateSize);

        _confirmedColors = new Color[_generateSize];
        for (int i = 0; i < _generateSize; i++)
        {
            c.a = 1;

            _confirmedColors[i] = c;
            c = Color.LerpUnclamped(c1, c2, i * delta);
        }

        _colors = DeepCopy(_confirmedColors);
        _confirmedColorCSV = ColorArrayToString(_confirmedColors);
        _colorCSV = _confirmedColorCSV;
    }

    [ContextMenu("Generate (Complementary With Decrease)")]
    private void GenerateComplementaryDecrease()
    {
        Color c1 = Random.ColorHSV();
        Color c2 = Color.white - c1;

        Color c = c1;

        float delta = Random.Range(_generateMin, _generateCompliDeltaCoeff / _generateSize);
        float colorDelta = Random.Range(_generateMin, _generateMonoDeltaCoeff / _generateSize);

        _confirmedColors = new Color[_generateSize];
        for (int i = 0; i < _generateSize; i++)
        {
            c.a = 1;

            _confirmedColors[i] = c;
            c = Color.LerpUnclamped(c1, c2, i * delta);
            c = new Color(c.r - colorDelta, c.g - colorDelta, c.b - colorDelta, c.a - colorDelta);
        }

        _colors = DeepCopy(_confirmedColors);
        _confirmedColorCSV = ColorArrayToString(_confirmedColors);
        _colorCSV = _confirmedColorCSV;
    }

    #endregion


    #region EDITOR

    //COLOR LIST OPERATIONS
    [ContextMenu("Invert Palette Order")]
    private void Invert()
    {
        Color[] newC = new Color[_confirmedColors.Length];

        for(int i = 0; i < newC.Length; i++)
        {
            newC[i] = _confirmedColors[newC.Length - i - 1];
        }

        _confirmedColors = newC;

        _colors = DeepCopy(_confirmedColors);
        _confirmedColorCSV = ColorArrayToString(_confirmedColors);
        _colorCSV = _confirmedColorCSV;
    }

    [ContextMenu("Invert Palette Colors")]
    private void InvertColor()
    {
        for (int i = 0; i < _confirmedColors.Length; i++)
        {
            _confirmedColors[i] = Color.white - _confirmedColors[i];
        }
        _colors = DeepCopy(_confirmedColors);
        _confirmedColorCSV = ColorArrayToString(_confirmedColors);
        _colorCSV = _confirmedColorCSV;
    }

    #endregion


    #region HELPER_FUNCTIONS

    //INTERNAL CHECKS
    private bool ColorsEqual()
    {
        bool equal = true;

        if (_colors.Length == _confirmedColors.Length)
        {
            for(int i = 0; i < _colors.Length; i++)
            {
                if (_colors[i] != _confirmedColors[i]) equal = false;
            }
        }
        else equal = false;

        return equal;
    }

    private Color[] DeepCopy(Color[] c)
    {
        Color[] newC = new Color[c.Length];
        for (int i = 0; i < c.Length; i++) newC[i] = c[i];

        return newC;
    }

    private Color[] ParseCSV(string s)
    {
        string[] split = _colorCSV.Trim().Split(","[0]);

        Color[] newC = new Color[split.Length];

        for(int i = 0; i < newC.Length; i++)
        {
            ColorUtility.TryParseHtmlString("#" + split[i], out newC[i]);
        }

        return newC;

    }

    private string ColorArrayToString(Color[] c)
    {
        string newString = "";
        for(int i = 0; i < c.Length; i++)
        {
            newString += ColorUtility.ToHtmlStringRGB(c[i]);
            if (i < c.Length - 1) newString += ",";
        }

        return newString;
    }

    #endregion


}
