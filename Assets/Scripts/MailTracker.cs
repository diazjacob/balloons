using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailTracker : ManagedMonobehaviour
{
    [SerializeField] private GameObject[] _indicatorObjs;

    private RectTransform[] _indicators;
    private Image[] _indicatorImages;

    private void Awake()
    {
        GetAllRefs();
    }

    void Start()
    {
        _indicators = new RectTransform[_indicatorObjs.Length];
        _indicatorImages = new Image[_indicatorObjs.Length];

        for (int i = 0; i < _indicatorObjs.Length; i++)
        {
            _indicators[i] = _indicatorObjs[i].GetComponent<RectTransform>();
            _indicatorImages[i] = _indicatorObjs[i].GetComponent<Image>();
        }
    }


    void Update()
    {
        for (int i = 0; i < _indicatorObjs.Length; i++) _indicatorObjs[i].SetActive(false);

        Mail[] mailList = MPlayer().GetAllMail();

        for(int i = 0; i < mailList.Length; i++)
        {
            if(mailList[i] != null)
            { 
                Mail m = mailList[i];
                int index = m.GetInventoryLink();

                _indicatorObjs[index].SetActive(true);

                Vector2 targ = (m.GetTarget() - MPlayer().GetPosition()).normalized;

                float offset = _indicatorImages[index].fillAmount * 180; // This is really just multiplied by 360/2, but simplified.

                float angle = Vector2.SignedAngle(-Vector2.right, targ);

                _indicators[index].rotation = Quaternion.Euler(-90,0, (-angle + offset) + 90);
            }
        }
    }
}
