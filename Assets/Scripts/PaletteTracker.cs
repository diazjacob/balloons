using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaletteTracker : ManagedMonobehaviour
{
    [SerializeField] private bool _imagesDarkened;

    private List<Image> _imagesToUpdate;
    private List<Text> _textToUpdate;

    void Awake()
    {
        GetAllRefs();

        //Player.OnPaletteUpdate += UpdateColors;

        _imagesToUpdate = new List<Image>();
        _textToUpdate = new List<Text>();

        RetriveAllComponents(transform);
        Player.OnPaletteUpdate += UpdateAll;
    }

    private void UpdateAll()
    {
        for (int i = 0; i < _imagesToUpdate.Count; i++)
        {
            var control = _imagesToUpdate[i];
            float a = control.color.a;

            Color newColor = MPlayer().GetPalette().GetMidColor();

            if (_imagesDarkened) newColor = MPlayer().GetPalette().GetBaseColor();
            
            control.color = new Color(newColor.r, newColor.g, newColor.b, a);
        }
        for (int i = 0; i < _textToUpdate.Count; i++)
        {
            var control = _textToUpdate[i];
            float a = control.color.a;
            var newColor = MPlayer().GetPalette().GetMidColor();
            control.color = new Color(newColor.r, newColor.g, newColor.b, a);
        }
    }

    //Grab components based on the transform heirarchy 
    private void RetriveAllComponents(Transform t)
    {
        Image im = t.gameObject.GetComponent<Image>();
        if (im != null) _imagesToUpdate.Add(im);
        Text tex = t.gameObject.GetComponent<Text>();
        if (tex != null) _textToUpdate.Add(tex);

        if(t.childCount > 0)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                RetriveAllComponents(t.GetChild(i));
            }

        }
    }

}
