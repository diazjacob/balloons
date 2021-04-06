using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteTrackerMat : ManagedMonobehaviour
{

    [SerializeField] private bool _randomMat = true;


    private MeshRenderer _mesh;

    void Awake()
    {
        GetAllRefs();
        _mesh = GetComponent<MeshRenderer>();

        Player.OnPaletteUpdate += UpdateAll;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void UpdateAll()
    {
        if(_randomMat)
        {
            _mesh.material.color = MPlayer().GetPalette().GetRandomColor();
        }
        else
        {
            _mesh.material.color = MPlayer().GetPalette().GetHighlightColor();
        }
    }
}
