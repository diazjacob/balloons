using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : ManagedMonobehaviour
{
    [SerializeField] private GameObject _paracuteObj;
    [SerializeField] private GameObject _boxObj;
    private ParticleSystem _particles;
    private ParticleSystemRenderer _rend;
    private WorldObject obj;
    private bool _landed;

    private void Awake()
    {
        GetAllRefs();
    }

    void Start()
    {
        Player.OnLeaveWorld += Reset;
        Player.OnPaletteUpdate += SetSmokeColor;

        obj = GetComponent<WorldObject>();
        _rend = GetComponentInChildren<ParticleSystemRenderer>();
        _particles = GetComponentInChildren<ParticleSystem>();

        _landed = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!_landed) //If we're still gliding
        {
            Vector2 pos = obj.GetPosition();
            float height = MPlayer().GetTerrainDisplayValue(pos.x, pos.y);

            if (height > transform.position.y) //The falling to landed transition -------
            {
                _landed = true;
                obj.InitalizeVelocity(Vector2.zero, 0);
                obj.SetIsMoving(false);
                _paracuteObj.SetActive(false);
                _particles.Play();
                
            }
            else //As we're activley falling ------------------------------
            {
                
            }
        }
        else //If we've landed ---------------------------
        {
            //Update some sort of indicators here

            //Update the smoke dir
            _particles.transform.rotation = MWindController().GetRotation();

        }

    }

    private void Reset()
    {
        _landed = false;
        transform.position = Player.OBJ_HOLD_POINT;
        _paracuteObj.SetActive(true);
        _particles.Stop();
    }


    private void SetSmokeColor()
    {
        _rend.material.color = MPlayer().GetPalette().GetHighlightColor();
    }
}
