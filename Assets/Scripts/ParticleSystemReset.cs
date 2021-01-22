using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemReset : ManagedMonobehaviour
{
    [SerializeField] private bool _resetsPosOnStop;
    private ParticleSystem _ps;
    private ParticleSystemRenderer _psr;
    private GameObject _origParent;

    // Start is called before the first frame update

    private void Awake()
    {
        GetAllRefs();
    }

    void Start()
    {
        _origParent = transform.parent.gameObject;

        _ps = GetComponent<ParticleSystem>();
        _psr = GetComponent<ParticleSystemRenderer>();

        if(_resetsPosOnStop)
        {
            var main = _ps.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        Player.OnPaletteUpdate += UpdateParticleSystemColor;
    }

    //ONLY TO BE CALLED BY A PARTICLE SYSTEM STOP ACTION
    public void OnParticleSystemStopped()
    {
        _ps.transform.parent = _origParent.transform;
        _ps.transform.position = _origParent.transform.position;
    }

    private void UpdateParticleSystemColor()
    {
        _psr.material.color = MPlayer().GetPalette().GetHighlightColor();
    }
}
