using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedIndicatorController : ManagedMonobehaviour
{
    //NOTE:

    //BL = posX
    //BR = posY
    //UR = negX
    //UL = negY

    private enum Dir
    {
        UL = 0,
        UR = 1,
        BR = 2,
        BL = 3
    }

    [Header("Speed Indicators: Order = (UL, UR, BR, BL)")]
    [SerializeField] private GameObject[] _speed0;
    [SerializeField] private GameObject[] _speed1;
    [SerializeField] private GameObject[] _speed2;
    [SerializeField] private GameObject[] _speed3;

    [SerializeField] private float _speedDisplayThreshold;

    private void Awake()
    {
        GetAllRefs();
    }

    private void Start()
    {
        for (int i = 0; i < _speed0.Length; i++) _speed0[i].SetActive(false);
        for (int i = 0; i < _speed1.Length; i++)  _speed1[i].SetActive(false);
        for (int i = 0; i < _speed2.Length; i++) _speed2[i].SetActive(false);
        for (int i = 0; i < _speed3.Length; i++) _speed3[i].SetActive(false);
    }

    private void Update()
    {
        UpdateSpeedDisplay();
    }

    public void UpdateSpeedDisplay()
    {
        Vector2 velocity = MPlayer().GetVelocity();

        float x = Mathf.Abs(velocity.x);
        float y = Mathf.Abs(velocity.y);

        bool xPos = false;
        bool yPos = false;

        //X INDICATORS
        if (x > _speedDisplayThreshold * 3)
        {
            if(velocity.x > 0)  //SPEED 3 BL
            {
                _speed0[(int)Dir.BL].SetActive(false);
                _speed1[(int)Dir.BL].SetActive(false);
                _speed2[(int)Dir.BL].SetActive(false);
                _speed3[(int)Dir.BL].SetActive(true); // 3 is set

                xPos = true;
            }
            else //SPEED 3 UR
            {
                _speed0[(int)Dir.UR].SetActive(false);
                _speed1[(int)Dir.UR].SetActive(false);
                _speed2[(int)Dir.UR].SetActive(false);
                _speed3[(int)Dir.UR].SetActive(true); // 3 is set
            }
        }
        else if (x > _speedDisplayThreshold * 2)
        {
            if (velocity.x > 0) //SPEED 2 BL
            {
                _speed0[(int)Dir.BL].SetActive(false);
                _speed1[(int)Dir.BL].SetActive(false);
                _speed2[(int)Dir.BL].SetActive(true);
                _speed3[(int)Dir.BL].SetActive(false);

                xPos = true;
            }
            else //SPEED 2 UR
            {
                _speed0[(int)Dir.UR].SetActive(false);
                _speed1[(int)Dir.UR].SetActive(false);
                _speed2[(int)Dir.UR].SetActive(true);
                _speed3[(int)Dir.UR].SetActive(false);
            }
        }
        else if (x > _speedDisplayThreshold)
        {
            if (velocity.x > 0) //SPEED 1 BL
            {
                _speed0[(int)Dir.BL].SetActive(false);
                _speed1[(int)Dir.BL].SetActive(true);
                _speed2[(int)Dir.BL].SetActive(false);
                _speed3[(int)Dir.BL].SetActive(false);

                xPos = true;
            }
            else //SPEED 1 UR
            {
                _speed0[(int)Dir.UR].SetActive(false);
                _speed1[(int)Dir.UR].SetActive(true);
                _speed2[(int)Dir.UR].SetActive(false);
                _speed3[(int)Dir.UR].SetActive(false);
            }
        }
        else if (x > MPlayer().GetMaxLandingSpeed())
        {
            if (velocity.x > 0) //SPEED 1 BR
            {
                _speed0[(int)Dir.BL].SetActive(true);
                _speed1[(int)Dir.BL].SetActive(false);
                _speed2[(int)Dir.BL].SetActive(false);
                _speed3[(int)Dir.BL].SetActive(false);

                xPos = true;
            }
            else //SPEED 1 UL
            {
                _speed0[(int)Dir.UR].SetActive(true);
                _speed1[(int)Dir.UR].SetActive(false);
                _speed2[(int)Dir.UR].SetActive(false);
                _speed3[(int)Dir.UR].SetActive(false);
            }
        }
        else //clear everything
        {
            xPos = true;
            _speed0[(int)Dir.BL].SetActive(false);
            _speed1[(int)Dir.BL].SetActive(false);
            _speed2[(int)Dir.BL].SetActive(false);
            _speed3[(int)Dir.BL].SetActive(false);
        }


        //Y INDICATORS
        if (y > _speedDisplayThreshold * 3)
        {
            if (velocity.y > 0) //SPEED 3 BR
            {
                _speed0[(int)Dir.BR].SetActive(false);
                _speed1[(int)Dir.BR].SetActive(false);
                _speed2[(int)Dir.BR].SetActive(false);
                _speed3[(int)Dir.BR].SetActive(true); // 3 is set

                yPos = true;
            }
            else //SPEED 3 UL
            {
                _speed0[(int)Dir.UL].SetActive(false);
                _speed1[(int)Dir.UL].SetActive(false);
                _speed2[(int)Dir.UL].SetActive(false);
                _speed3[(int)Dir.UL].SetActive(true);
            }
        }
        else if (y > _speedDisplayThreshold * 2)
        {
            if (velocity.y > 0) //SPEED 2 BR
            {
                _speed0[(int)Dir.BR].SetActive(false);
                _speed1[(int)Dir.BR].SetActive(false);
                _speed2[(int)Dir.BR].SetActive(true);
                _speed3[(int)Dir.BR].SetActive(false);

                yPos = true;
            }
            else //SPEED 2 UL
            {
                _speed0[(int)Dir.UL].SetActive(false);
                _speed1[(int)Dir.UL].SetActive(false);
                _speed2[(int)Dir.UL].SetActive(true);
                _speed3[(int)Dir.UL].SetActive(false);
            }
        }
        else if (y > _speedDisplayThreshold)
        {
            if (velocity.y > 0) //SPEED 1 BR
            {
                _speed0[(int)Dir.BR].SetActive(false);
                _speed1[(int)Dir.BR].SetActive(true);
                _speed2[(int)Dir.BR].SetActive(false);
                _speed3[(int)Dir.BR].SetActive(false);

                yPos = true;
            }
            else //SPEED 1 UL
            {
                _speed0[(int)Dir.UL].SetActive(false);
                _speed1[(int)Dir.UL].SetActive(true);
                _speed2[(int)Dir.UL].SetActive(false);
                _speed3[(int)Dir.UL].SetActive(false);
            }
        }
        else if (y > MPlayer().GetMaxLandingSpeed())
        {
            if (velocity.y > 0) //SPEED 1 BR
            {
                _speed0[(int)Dir.BR].SetActive(true);
                _speed1[(int)Dir.BR].SetActive(false);
                _speed2[(int)Dir.BR].SetActive(false);
                _speed3[(int)Dir.BR].SetActive(false);

                yPos = true;
            }
            else //SPEED 1 UL
            {
                _speed0[(int)Dir.UL].SetActive(true);
                _speed1[(int)Dir.UL].SetActive(false);
                _speed2[(int)Dir.UL].SetActive(false);
                _speed3[(int)Dir.UL].SetActive(false);
            }
        }
        else //clear everything
        {
            yPos = true;
            _speed0[(int)Dir.BR].SetActive(false);
            _speed1[(int)Dir.BR].SetActive(false);
            _speed2[(int)Dir.BR].SetActive(false);
            _speed3[(int)Dir.BR].SetActive(false);
        }

        if(xPos)
        {
            _speed0[(int)Dir.UR].SetActive(false);
            _speed1[(int)Dir.UR].SetActive(false);
            _speed2[(int)Dir.UR].SetActive(false);
            _speed3[(int)Dir.UR].SetActive(false);
        }
        else
        {
            _speed0[(int)Dir.BL].SetActive(false);
            _speed1[(int)Dir.BL].SetActive(false);
            _speed2[(int)Dir.BL].SetActive(false);
            _speed3[(int)Dir.BL].SetActive(false);
        }

        if(yPos)
        {
            _speed0[(int)Dir.UL].SetActive(false);
            _speed1[(int)Dir.UL].SetActive(false);
            _speed2[(int)Dir.UL].SetActive(false);
            _speed3[(int)Dir.UL].SetActive(false);
        }
        else
        {
            _speed0[(int)Dir.BR].SetActive(false);
            _speed1[(int)Dir.BR].SetActive(false);
            _speed2[(int)Dir.BR].SetActive(false);
            _speed3[(int)Dir.BR].SetActive(false);
        }
    }
}
