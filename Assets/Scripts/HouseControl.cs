using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseControl : ManagedMonobehaviour
{
    private static float PICKUP_DIST = 3f;

    private Mail _mail;

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
        if(_wo.IsInitalized() && _mail != null)
        {
            Vector2 pos = MPlayer().GetPlayerReletivePosition(_mail.GetTarget());
            Vector2 src = MPlayer().GetPlayerReletivePosition(_mail.GetSource());

            Vector3 newTarg = new Vector3(pos.x, 0, pos.y);
            Vector3 newSrs = new Vector3(src.x, 0, src.y);
            Vector3 halfwayPoint = ((newTarg - newSrs) / 2) + newSrs;

            Debug.DrawLine(newSrs, halfwayPoint, Color.green);
            Debug.DrawLine(halfwayPoint, newTarg, Color.cyan);
        }

#endif
    }

    public void Initalize()
    {
        Vector2 targ = MPlayer().GetPOITarget(_wo.GetPosition());

        print("Initalizing Mail with:" + _wo.GetPosition() + "SRC and :" + targ + "TARG");
        _mail = new Mail(_wo.GetPosition(), targ);
    }

    public void Reset()
    {
        _mail = null;
    }

    public void Tapped()
    {
        string output = "";

        if(_mail != null)
        {
            if (MPlayer().GetLanded())
            {
                if (Vector2.Distance(_wo.GetPosition(), MPlayer().GetPosition()) < PICKUP_DIST)
                {
                    if(MPlayer().AddMail(_mail))
                    {
                        output = "Mail Loaded";
                        _mail = null;
                    }
                    else
                    {
                        output = "No Room";
                    }

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

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (_wo.IsInitalized() && _mail != null)
        {
            Vector2 pos = MPlayer().GetPlayerReletivePosition(_mail.GetTarget());
            Vector3 newTarg = new Vector3(pos.x, 0, pos.y);
            Vector3 newSrs = new Vector3(transform.position.x, 0, transform.position.z);

            Debug.DrawLine(newSrs, newTarg, Color.red);

        }
    }


#endif

}

[System.Serializable]
public class Mail
{
    private const float frac = 1 / (float)16.0f;
    public static int VALUE_COEFF = 2;

    private Vector2 _srcPos;
    private Vector2 _targetPos;

    private PackageType _type = PackageType.Letter;

    private int _value;
    private Direction _dir;

    private int _inventoryLinkVal;

    public Mail(Vector2 srcPos, Vector2 targ)
    {
        _srcPos = srcPos;
        _targetPos = targ;

        //TESTING!!
        _type = (PackageType)Random.Range(0, 4);
        if (_type == 0) _type = PackageType.Letter;
        //TESTING!!!

        _value = CalculateValue();
        _dir = CalculateDirection();

        _inventoryLinkVal = -1;
    }

    private int CalculateValue()
    {
        Vector2 dir = _targetPos - _srcPos;

        return((int)dir.magnitude) * VALUE_COEFF * (((int)_type)/10 + 1);
    }

    private Direction CalculateDirection()
    {
        Vector2 dir = _targetPos - _srcPos;

        float upDot = Vector2.Angle(dir, InputController.UP_DIR) / 180.0f;
        float rightDot = Vector2.Angle(dir, InputController.RIGHT_DIR) / 180.0f;

        Direction result = Direction.North;

        if (upDot > 1 - frac) result = Direction.North;
        if (upDot < frac) result = Direction.South;
        if (rightDot > 1 - frac) result = Direction.East;
        if (rightDot < frac) result = Direction.West;

        if (upDot > frac && upDot < 3 * frac && rightDot > frac && rightDot < 3 * frac) result = Direction.NorthEast;
        if (upDot < -frac && upDot > -3 * frac && rightDot > frac && rightDot < 3 * frac) result = Direction.SouthEast;
        if (upDot > frac && upDot < 3 * frac && rightDot < -frac && rightDot > -3 * frac) result = Direction.NorthWest;
        if (upDot < -frac && upDot > -3 * frac && rightDot > frac && rightDot < 3 * frac) result = Direction.NorthEast;

        return result;
    }

    public Vector2 GetTarget() { return _targetPos; }
    public Vector2 GetSource() { return _srcPos; }
    public int GetValue() { return _value; }
    public Direction GetDirection() { return _dir; }
    public new PackageType GetType() { return _type; }

    public void LinkToInventory(int linkVal)
    {
        _inventoryLinkVal = linkVal;
        //if (_inventoryLinkVal != -1) Debug.LogError("Mail Item has been linked to inventory slot more than once.");
    }


    public int GetInventoryLink() { return _inventoryLinkVal; }
    public string GetDirectionString()
    {
        string result = "";

        switch(_dir)
        {
            case Direction.North:
                result = "North";
                break;
            case Direction.South:
                result = "South";
                break;
            case Direction.East:
                result = "East";
                break;
            case Direction.West:
                result = "West";
                break;
            case Direction.NorthWest:
                result = "North-West";
                break;
            case Direction.NorthEast:
                result = "North-East";
                break;
            case Direction.SouthEast:
                result = "South-East";
                break;
            case Direction.SouthWest:
                result = "South-West";
                break;
        }

        return result;
    }
}

public enum PackageType
{
    Letter = 1,
    Parcel = 2,
    Package = 3,
    Crate = 4
}

public enum Direction
{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest
}







