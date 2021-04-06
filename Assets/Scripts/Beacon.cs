using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : ManagedMonobehaviour
{
    [SerializeField] private GameObject _paracuteObj;
    [SerializeField] private GameObject _boxObj;
    private ParticleSystem _particles;
    private ParticleSystemRenderer _rend;
    private WorldObject _wo;
    private bool _landed;

    private Mail _mail;

    private void Awake()
    {
        GetAllRefs();
    }

    void Start()
    {
        Player.OnLeaveWorld += Reset;
        Player.OnPaletteUpdate += SetSmokeColor;

        _wo = GetComponent<WorldObject>();
        _rend = GetComponentInChildren<ParticleSystemRenderer>();
        _particles = GetComponentInChildren<ParticleSystem>();

        _landed = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!_landed) //If we're still gliding
        {
            Vector2 pos = _wo.GetPosition();
            float height = MPlayer().GetTerrainDisplayValue(pos.x, pos.y);

            if (height > transform.position.y) //The falling to landed transition -------
            {
                if (_mail != null)
                {
                    MPlayer().CheckRemoveMail(_mail, _wo.GetPosition(), height);

                    _mail = null;
                }
                else
                {
                    _particles.Play();
                }

                _landed = true;
                _wo.InitalizeVelocity(Vector2.zero, 0);
                _wo.SetIsMoving(false);
                _paracuteObj.SetActive(false);


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

    public void SetMail(Mail mail)
    {
        _mail = mail;
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
