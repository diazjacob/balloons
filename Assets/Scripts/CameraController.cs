using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static float COMPARE_THRESHOLD = .1f;

    [SerializeField] private Vector2 _mainOrthoBounds;
    [SerializeField] private float _zoomedOrthoSize;
    [SerializeField] private float _orthoLerpSpeed;
    [SerializeField] private float _orthoZoomCoeff;
    [SerializeField] private float _otrhoZoomDeltaClamp = 1;
    [Space]
    [SerializeField] private GameObject _cullSphere;
    [SerializeField] private float _mainCullSize;
    [SerializeField] private float _zoomedCullSize;
    [SerializeField] private float _cullLerpSpeed;

    private float _previousTouchDist;

    private float _currentMainOrthoSize;

    private float _currentOrthoTarget;
    private float _currentCullObjTarget;

    private bool _isZoomed;

    private Camera _mainCamera;

    public delegate void InventoryOpened();
    public static event InventoryOpened OnInventoryOpened;

    public delegate void InventoryClosed();
    public static event InventoryClosed OnInventoryClosed;

    // Start is called before the first frame update

    void Start()
    {
        _currentMainOrthoSize = _mainOrthoBounds.y;
        _mainCamera = GetComponent<Camera>();
        SetCameraOrtho();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckCameraZoom();

        PerformLerps();
    }

    private void PerformLerps()
    {
        float difference = Mathf.Abs(_mainCamera.orthographicSize - _currentOrthoTarget);
        //Keep Lerping the camera
        if (difference > COMPARE_THRESHOLD)
        {
            _mainCamera.orthographicSize -= Mathf.Sign(_mainCamera.orthographicSize - _currentOrthoTarget) * _orthoLerpSpeed * Time.deltaTime * difference;
        }

        difference = Mathf.Abs(_cullSphere.transform.localScale.magnitude - _currentCullObjTarget);
        //Keep Lerping the camera
        if (difference > COMPARE_THRESHOLD)
        {
            _cullSphere.transform.localScale -= Vector3.one * Mathf.Sign(_cullSphere.transform.localScale.magnitude - _currentCullObjTarget) * _orthoLerpSpeed * Time.deltaTime * difference;
        }
    }

    private void CheckCameraZoom()
    {
        Touch[] touches = Input.touches;

        if(touches.Length == 2 && !_isZoomed)
        {
            if(touches[0].phase == TouchPhase.Began || touches[1].phase == TouchPhase.Began)
            {
                _previousTouchDist = Vector2.Distance(touches[0].position, touches[1].position);
            }
            else if (touches[0].phase == TouchPhase.Moved || touches[1].phase == TouchPhase.Moved)
            {
                float newDist = Vector2.Distance(touches[0].position, touches[1].position);
                float zoomDelta = Mathf.Clamp(_previousTouchDist - newDist, -_otrhoZoomDeltaClamp, _otrhoZoomDeltaClamp);

#if UNITY_EDITOR
                zoomDelta = Input.mouseScrollDelta.y * _orthoZoomCoeff;
#endif

                _previousTouchDist = newDist;

                _currentMainOrthoSize += (zoomDelta * _orthoZoomCoeff);
                _currentMainOrthoSize = Mathf.Clamp(_currentMainOrthoSize, _mainOrthoBounds.x, _mainOrthoBounds.y);

                _currentOrthoTarget = _currentMainOrthoSize;
            }
        }
    }



    public void SetCameraOrtho(bool zoomed = false)
    {
        _isZoomed = zoomed;

        if (_isZoomed)
        {
            _currentOrthoTarget = _zoomedOrthoSize;
            _currentCullObjTarget = _zoomedCullSize;

            OnInventoryOpened();
        }
        else
        {
            _currentOrthoTarget = _currentMainOrthoSize;
            _currentCullObjTarget = _mainCullSize;

            OnInventoryClosed();
        }
    }
    public void SetCameraOrthoSpecific(bool zoomed = false, bool items = false)
    {
        _isZoomed = zoomed;

        if (_isZoomed) _currentOrthoTarget = _zoomedOrthoSize;
        else  _currentOrthoTarget = _currentMainOrthoSize;
        
        if (items) _currentCullObjTarget = _zoomedCullSize;
        else  _currentCullObjTarget = _mainCullSize;
   
    }

    [ContextMenu("Toggle Camera Zoom")]
    public void ToggleCameraOrtho()
    {
        _isZoomed = !_isZoomed;

        if (_isZoomed)
        {
            _currentOrthoTarget = _zoomedOrthoSize;
            _currentCullObjTarget = _zoomedCullSize;

            OnInventoryOpened();
        }
        else
        {
            _currentOrthoTarget = _currentMainOrthoSize;
            _currentCullObjTarget = _mainCullSize;

            OnInventoryClosed();
        }
    }
}
