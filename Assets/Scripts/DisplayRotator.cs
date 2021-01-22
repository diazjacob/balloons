using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayRotator : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 1f;
    [SerializeField] private float _rotationSpeedVariance = 5f;

    private float _currentVariance;

    // Start is called before the first frame update
    void Start()
    {
        CameraController.OnInventoryOpened += RandomizeRotationDir;
        _currentVariance = Random.Range(-_rotationSpeedVariance, 0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation *= Quaternion.Euler(0, Time.deltaTime * (_rotationSpeed + _currentVariance), 0);
    }

    private void RandomizeRotationDir()
    {
        _rotationSpeed *= Mathf.Sign(Random.value - 0.5f);
        _currentVariance = Random.Range(-_rotationSpeedVariance, _rotationSpeedVariance);
    }
}
