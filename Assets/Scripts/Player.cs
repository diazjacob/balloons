using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : ManagedMonobehaviour
{
    public static Vector3 OBJ_HOLD_POINT = new Vector3(0, 40, 0); //The place we hold all objects we don't want on screen
    private static float COVERPLANE_THRESH = 0.01f; //The threshuold for lerping the transparency of the screen covering plane material
    private static float VELOCITY_THRESH = 0.0001f; //The velocity measurment threshold

    [Header("Exposed Values, Dont change.")]
    [SerializeField] private Vector2 _position;     //The position of the balloon in 2D game-space
    [SerializeField] private PaletteSetting _currentPalette;    //The current palette SO refrence from the REsources folder
    [SerializeField] private GameState _state;  //The current overarching state of the game

    [Space]

    [Header("Height Indicator Settings: ")]
    [SerializeField] private GameObject _heightIndicator;   //ref to the player's height indicator

    [Space]

    [Header("Game Settings: ")]
    [SerializeField] private float _maxLandingSpeed;    //The max lateral speed the player can safely land
    [SerializeField] private float _heightCalcCoefficent;   //The coefficent for calculating the display height based on y pos
    [SerializeField] private float _contractFinishHeight;   //The height in which a player can leave the region when flying above
    [SerializeField] private float _contractStartVerticalDropSpeed = -0.007f;   //The vertical velocity of the player when they enter a region

    private bool _gamePaused;   //Is the game paused?

    [Space]

    [Header("Transitions Settings:")]
    [SerializeField] private GameObject _coverPlane;    //The editor refrence to the plane that covers the screen
    private Material _coverPlaneMat;    //The material for the cover plane (for alpha fades)
    [SerializeField] private float _fadeOutSpeed;   //The speed in which the cover plane fades
    [SerializeField] private float _velocityKillSpeed;      //How fast we kill lateral velocity on region exit

    [Space]

    [Header("Manager Settings:")]
    //Other general data structures / controllers
    private ContractManager _contractManager;       //The manager of the contracts
    private bool _contractFinishedAndValid;     //If a contract is finished and valid while still in a region
    private bool _contractAbandoned;    //If a contract was abandoned


    private PlayerStats _playerStats;      //A seperate class for holding player stats

    //Flight related settings
    private bool _landed;       //Have we landed?

    private Vector2 _currentVelocity;   //The current lateral velocity
    private float _currentVerticalVelocity;     //The current vertical velocity


    [Space]

    [Header("Mail Settings:")]
    private List<Mail> _mailList = new List<Mail>();
    [SerializeField] private int _mailCapacity = 2;


    //The events
    public delegate void PaletteChangeAction();
    public static event PaletteChangeAction OnPaletteUpdate;    //The action to update all objects with palette colors

    public delegate void WorldFinishedAction();
    public static event WorldFinishedAction OnLeaveWorld;   //The action for when the player leaves the world

    public delegate void WorldBeginAction();
    public static event WorldBeginAction OnEnterWorld;      //The action for when the player enters the world


    private void Awake()
    {
        GetAllRefs();

        //Get attached components
        _contractManager = GetComponent<ContractManager>();
        _coverPlaneMat = _coverPlane.GetComponent<MeshRenderer>().material;

        _mailList = new List<Mail>();


        OnPaletteUpdate += UpdateIndicatorColor; //Do the initial update of the palette
    }

    private void Start()
    {
        _playerStats = new PlayerStats();

        //TESTING
        DebugRegnerateTerrain();

        //TESTING!!!
    }

    private void Update()
    {
        if (!_gamePaused) CheckState();
    }

    #region GAMESTATE

    private void CheckState()
    {
        //Indicator update
        UpdateIndicator();

        //Position Update
        _position += _currentVelocity * Time.deltaTime;
        transform.position += Vector3.up * _currentVerticalVelocity * Time.deltaTime;

        //Game State Update
        switch (_state) //Operate the system based on the gamestate
        {
            case GameState.ContractActive:

                if (_contractAbandoned && transform.position.y > _contractFinishHeight) _state = GameState.TransitionOut;

                break;

            case GameState.TransitionOut:

                float aout = _coverPlaneMat.color.a;
                if (aout < 1 - COVERPLANE_THRESH)
                {
                    aout += _fadeOutSpeed * Time.deltaTime;
                    _coverPlaneMat.color = new Color(_coverPlaneMat.color.r, _coverPlaneMat.color.g, _coverPlaneMat.color.b, aout);
                }
                else //The actual transition period
                {
                    OnLeaveWorld();

                    SetVerticalVelocity(0);
                    _position = Vector2.zero;

                    MCameraController().SetCameraOrthoSpecific(true, false);
                }

                break;
            case GameState.TransitionIn:

                float ain = _coverPlaneMat.color.a;
                if (ain > COVERPLANE_THRESH)
                {
                    ain -= _fadeOutSpeed * Time.deltaTime;
                    _coverPlaneMat.color = new Color(_coverPlaneMat.color.r, _coverPlaneMat.color.g, _coverPlaneMat.color.b, ain);
                }
                else //The actual transition period
                {
                    _state = GameState.ContractActive;

                    _coverPlaneMat.color = new Color(_coverPlaneMat.color.r, _coverPlaneMat.color.g, _coverPlaneMat.color.b, 0);
                }

                break;

        }
    }

    //For setting the game pause state
    public void SetGamePaused(bool paused)
    {
        _gamePaused = paused;

        if (paused) Time.timeScale = 0;
        else Time.timeScale = 1;
    }
    public bool GetGamePaused() { return _gamePaused; }

    //Abandoning the contract 
    public void SetContractAbandoned(bool c) { _contractAbandoned = c; }
    public bool GetContractAbandoned() { return _contractAbandoned; }

    //For passing the player stats refrence
    public PlayerStats GetPlayerStats() { return _playerStats; }
    public bool PlayerInWorld()
    {
        return (_state == GameState.ContractActive
            || _state == GameState.TransitionIn || _state == GameState.TransitionOut);
    }

    #endregion


    #region MOVEMENT

    //Velocity Handling
    public void AddVelocity(Vector2 v) { _currentVelocity += v; }
    public void SetVelocity(Vector2 v) { _currentVelocity = v; }
    public Vector2 GetVelocity() { return _currentVelocity; }
    public void AddVerticalVelocity(float v) { _currentVerticalVelocity += v; }
    public void SetVerticalVelocity(float v) { _currentVerticalVelocity = v; }
    public float GetVerticalVelocity() { return _currentVerticalVelocity; }

    //Position Handling
    public Vector2 GetPosition() { return _position; }
    public float GetVerticalPosition() { return transform.position.y;  }

    public Vector2 GetPlayerReletivePosition(Vector2 pos)
    {
        return (new Vector2(_position.x, _position.y) - pos);
    }

    #endregion


    #region PLAYER_PARAMS

    //Getting personal parameters
    public float GetMaxLandingSpeed() { return _maxLandingSpeed; }
    public float GetMaxHeight() { return _contractFinishHeight * _heightCalcCoefficent; }
    public bool GetLanded() { return _landed; }
    public bool CanPlayerMove()
    {
        return (_state == GameState.ContractActive);
    }

    public void SetLanded(bool l) { _landed = l; }
    public bool WasContractProperlyFinished() { return _contractFinishedAndValid; }
    public float ApplyHeightCoefficent(float h) { return h * _heightCalcCoefficent; }


    #endregion


    #region CONTRACT

    public void GenerateContracts()
    {
        _contractManager.Generate();
    }

    public void SelectContract(int index)
    {
        _contractFinishedAndValid = false;
        _contractAbandoned = false;
    }

    public void StartJourney(bool onContract, bool startWithMomentum = true)
    {
        if (!onContract)
        {
            GenerateContracts();
            SelectContract(0);
        }

        MPOIManager().InitalizeWorldObjects();

        _state = GameState.TransitionIn;
        MCameraController().SetCameraOrtho(false);
        if (startWithMomentum) _currentVerticalVelocity = _contractStartVerticalDropSpeed;
    }


    //Contract Information
    public Vector2 GetContractFirstPOI() { return _contractManager.GetFirstPOI(); }
    public Vector2 GetContractNextPOI() { return _contractManager.GetNextPOI(); }
    public Vector2 GetPOITarget(Vector2 sourcePos) { return _contractManager.GetTargetPos(sourcePos); }

    #endregion


    #region TERRAIN

    public float GetTerrainValue(float x, float y) { return _contractManager.GetCurrentTerrain(x, y); }
    public float GetTerrainDisplayValue(float x, float y) { return Mathf.Clamp01(_contractManager.GetCurrentTerrain(x, y)); }
    public float GetTerrainDisplayValue() { return Mathf.Clamp01(_contractManager.GetCurrentTerrain(_position.x, _position.y)); }

    //Terrain settings
    public float GetTrueAltitude()
    {
        return transform.position.y - GetTerrainDisplayValue(_position.x, _position.y);
    }

    #endregion


    #region COLLISION

    private void OnTriggerEnter(Collider collider)
    {
        print("Player collided with something");
        GameObject obj = collider.gameObject;

        if (obj.tag == "Obstacle") //Then the player has collided with us
        {
            WorldObject worldObj = obj.GetComponent<WorldObject>();

            worldObj.PlayerCollision();

            //DAMAGE THE PLAYER HERE OR SOMETHING
        }
    }

    #endregion

    #region MAIL

    public bool AddMail(Mail mail)
    {
        bool result = false;

        if(mail != null)// && _mailList.Count < _mailCapacity)
        {
            if(!MInventoryManager().IsFull())
            {
                int val = MInventoryManager().AddItem(mail.GetType());
                if (val != -1)
                {
                    //print("Mail Location index:" + val);

                    mail.LinkToInventory(val);
                    _mailList.Add(mail);
                    result = true;
                    mail = null;
                }
                else Debug.LogError("Inventory Marked as 'Not Full' but no slot was found.");

            }
        }

        return result;
    }

    public Mail[] GetAllMail()
    {
        Mail[] mailList = new Mail[6];
        _mailList.CopyTo(mailList);

        return mailList;
    }

    public Mail GetMailFromRef(int index)
    {
        Mail result = null;

        for(int i = 0; i < _mailList.Count && result == null; i++)
        {
            if(_mailList[i].GetInventoryLink() == index)
            {
                result = _mailList[i];
            }
        }

        if (result == null) Debug.LogError("Could not find mail in _mailList from refrence with index: " + index);

        return result;
    }

    public void CheckRemoveMail(Mail mail, Vector2 position, float height)
    {
        bool valid = _mailList.Remove(mail);

        if (valid)
        {
            Vector2 targ = mail.GetTarget();

            float dist = Vector2.Distance(targ, position);

            float result = mail.GetValue() - dist;


            string message = "Package Drop Delivered";

            if (height < 0.05)
            {
                result /= 2; //Dividing the result by 2 if the package ends up in the water. Lol
                message += "(Water Damaged)";
            }

            _playerStats.IncrementGold((int)result);
            MUIManager().NewIconFade(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f), message, GetPalette().GetBaseColor());
        }
        else Debug.LogError("Attempted to deliver mail that was not present in the player's inverotry");



    }

    public void CheckRemoveMail(Mail mail)
    {
        bool valid = _mailList.Remove(mail);

        if (valid)
        {
            float result = mail.GetValue();

            _playerStats.IncrementGold((int)result);

            string message = "Package Hand Delivered!";
            MUIManager().NewIconFade(new Vector2(Screen.width / 2.0f, Screen.height / 2.0f), message, GetPalette().GetBaseColor());
        }
        else Debug.LogError("Attempted to deliver mail that was not present in the player's inverotry");
    }



    #endregion



    #region LOOKS

    //Looks & Asthetic
    public void SetPalette(PaletteSetting p)
    {
        _currentPalette = p;
        Color newColor = p.GetBaseColor();
        _coverPlaneMat.color = new Color(newColor.r, newColor.g, newColor.b, _coverPlaneMat.color.a);

        OnPaletteUpdate();
    }
    public PaletteSetting GetPalette() { return _currentPalette; }

    //WORLD SETTINGS (Shops/Indicators/...)
    private void UpdateIndicator()
    {
        Vector2 p = GetPosition();
        _heightIndicator.transform.position = new Vector3(0, GetTerrainDisplayValue(p.x, p.y), 0);
    }
    private void UpdateIndicatorColor() { _heightIndicator.GetComponent<MeshRenderer>().material.color = _currentPalette.GetBaseColor(); }

    #endregion



    //DEBUG --------------------------------------
    [ContextMenu("Regenerate Contract Terrain")]
    public void DebugRegnerateTerrain()
    {
        if (_contractManager != null)
        {
            _contractManager.Generate();
        }
        //regen palette
        PaletteSetting[] p = Resources.LoadAll<PaletteSetting>("Palettes/");
        SetPalette(p[Random.Range(0, p.Length)]);
        StartJourney(true, false);
    }
    [ContextMenu("Refresh Palette Setting")]
    public void DebugUpdatePalette() { OnPaletteUpdate(); }


    private void OnDrawGizmos()
    {
        for(int i = 0; i < _mailList.Count; i++)
        {
            if(_mailList[i] != null)
            {
                Vector2 targ = GetPlayerReletivePosition(_mailList[i].GetTarget());
                Vector3 newTarg = new Vector3(targ.x, 0, targ.y);

                Debug.DrawLine(transform.position, newTarg, Color.magenta);
            }
        }
    }
    //DEBUG -----------------------------------------

}


[System.Serializable]
public class PlayerStats
{
    private int _gold = 0;      //How much currency does the player have?
    private int _numContracts = 0; //How many contracts were turned in properly


    public int GetGold() { return _gold; }
    public void IncrementGold(int g) { _gold += g; }

    public int GetNumContracts() { return _numContracts; }
    public void IncrementNumContracts() { _numContracts++; }
}

public enum GameState
{
    TransitionOut,
    TransitionIn,
    ContractActive,
}
