using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagedMonobehaviour : MonoBehaviour
{
    private Player player;

    private InputController inC;
    private LevelController levC;
    private CameraController camC;
    private WindController windC;

    private UIManager uiM;
    private ObstacleManager obsM;
    private POIManager poiM;
    private InventoryManager invM;

    protected Player MPlayer() { return player; }
    protected InputController MInputController() { return inC; }
    protected LevelController MLevelController() { return levC; }
    protected CameraController MCameraController() { return camC; }
    protected WindController MWindController() { return windC; }
    protected UIManager MUIManager() { return uiM; }
    protected ObstacleManager MObstacleManager() { return obsM; }
    protected POIManager MPOIManager() { return poiM; }
    protected InventoryManager MInventoryManager() { return invM; }


    protected void GetAllRefs()
    {
        player = (Player)GetRef<Player>();
        inC = (InputController)GetRef<InputController>();
        levC = (LevelController)GetRef<LevelController>();
        camC = (CameraController)GetRef<CameraController>();
        windC = (WindController)GetRef<WindController>();

        uiM = (UIManager)GetRef<UIManager>();
        obsM = (ObstacleManager)GetRef<ObstacleManager>();
        poiM = (POIManager)GetRef<POIManager>();
        invM = (InventoryManager)GetRef<InventoryManager>();

    }

    private static UnityEngine.Object GetRef<T>()
    {
        UnityEngine.Object[] allRefs = FindObjectsOfType(typeof(T));

        if (allRefs.Length > 1) Debug.LogError("STARTUP ERROR: There is more than one " + typeof(T).ToString() + " monobehaviour in the scene.");
        if (allRefs.Length <= 0) Debug.LogError("STARTUP ERROR: There is no " + typeof(T).ToString() + ".");

        return allRefs[0];
    }
}
