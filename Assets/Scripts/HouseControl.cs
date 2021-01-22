using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseControl : ManagedMonobehaviour
{
    private static float PICKUP_DIST = 3f;

    private bool _hasMail;
    private Vector2 _target;

    private string[] _hasMailStrings = {"Needs Delivery", "Has Mail", "Has Package", "Has Letter"};
    private string[] _noMailStrings = { "No Mail", "Nothing to Deliver", "Nothing to Pickup" };

    private WorldObject _wo;

    private void Awake()
    {
        GetAllRefs();
        _wo = GetComponent<WorldObject>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if(_wo.IsInitalized())
        {
            Vector2 pos = MPlayer().GetPlayerReletivePosition(_target);
            Vector3 newTarg = new Vector3(pos.x, 0, pos.y);
            Vector3 newSrs = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 halfwayPoint = ((newTarg - newSrs) / 2) + newSrs;

            Debug.DrawLine(newSrs, halfwayPoint, Color.green);
            Debug.DrawLine(halfwayPoint, newTarg, Color.cyan);
        }

#endif
    }

    public void Initalize()
    {
        _hasMail = true;

        _target = MPlayer().GetPOITarget(MPlayer().GetPlayerReletivePosition(_wo.GetPosition()));
    }

    public void Reset()
    {
        _hasMail = false;
    }

    public void Tapped()
    {
        string output = "";

        if(_hasMail)
        {
            if (MPlayer().GetLanded())
            {
                if (Vector2.Distance(_wo.GetPosition(), MPlayer().GetPosition()) < PICKUP_DIST)
                {
                    output = "Mail Loaded";
                    _hasMail = false;
                }
                else output = "Too Far";
            }
            else output = "Needs Delivery";
        }
        else
        {
            output = "No Mail";
        }

        MUIManager().NewIconFade(transform.position, output, MPlayer().GetPalette().GetHighlightColor());
    }

}

public class Mail
{



    public Mail()
    {

    }
}









