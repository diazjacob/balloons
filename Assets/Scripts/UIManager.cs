using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : ManagedMonobehaviour
{

    [Space]

    [Space]
    [Header("Pause Menu Settings: ")]
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _pauseMenuAbortButton;
    [SerializeField] private GameObject _pauseButton;
    [SerializeField] private GameObject _soundButton;
    [Space]

    [SerializeField] private float _verticalOffset;
    [SerializeField] private GameObject _fadeTextObject;
    [SerializeField] private int _fontSize;
    [SerializeField] private float _fontSpeed;
    [SerializeField] private float _fadeSpeed;
    [SerializeField] private GameObject _instantiateParent;

    private bool _gamePaused;

    private void Awake()
    {
        GetAllRefs();

        CameraController.OnInventoryOpened += InventoryOpen;
        CameraController.OnInventoryClosed += InventoryClose;
    }

    #region WorldUI

    //INVENTORY OPENING SPECIFICS
    public void InventoryOpen() { SetMenuButtons(false); }
    public void InventoryClose() { SetMenuButtons(true); }
    public void SetMenuButtons(bool val)
    {
        _pauseButton.SetActive(val);
        _soundButton.SetActive(val);
    }

    #endregion


    #region PauseMenu

    public void AbandonContract()
    {
        MPlayer().SetContractAbandoned(true);

        PauseMenu();
    }

    //THE PAUSE MENU
    public void PauseMenu()
    {
        _gamePaused = !_gamePaused;
        MPlayer().SetGamePaused(_gamePaused);

        _pauseMenu.SetActive(_gamePaused);
        _pauseMenuAbortButton.SetActive(false);


        //_pauseMenuAbortButton.SetActive((!MPlayer().GetContractAbandoned()) && _gamePaused);
    }

    #endregion

    #region IconFade

    //General fading icon methods
    public void NewIconFade(Vector3 objPos, string message, Color color, float fadeSpeedModifier = 1)
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(new Vector3(objPos.x, objPos.y + _verticalOffset, objPos.z));
        NewIconFade(screenPosition, message, color, fadeSpeedModifier);
    }
    public void NewIconFade(Vector2 screenPosition, string message, Color color, float fadeSpeedModifier = 1)
    {
        GameObject newText = Instantiate(_fadeTextObject, Vector3.zero, Quaternion.identity);
        newText.transform.SetParent(_instantiateParent.transform);
        newText.transform.SetAsFirstSibling();

        Text text = newText.GetComponent<Text>();
        text.fontSize = _fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.resizeTextForBestFit = true;
        text.rectTransform.position = screenPosition;

        text.color = color;
        text.text = message;

        StartCoroutine(UiIconFade(text, _fadeSpeed / fadeSpeedModifier));
    }

    private IEnumerator UiIconFade(Text text, float fadeSpeedDelta = 0f)
    {
        //float fadeTime = fadeSpeedDelta;
        int bound = (int)(255f / fadeSpeedDelta);
        for (int i = 0; i < bound; i++)
        {
            text.gameObject.transform.position += new Vector3(0, _fontSpeed, 0);
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - fadeSpeedDelta / 255f);
            yield return null;
        }
        Destroy(text.gameObject);
    }

    #endregion

}

