using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepRotation : MonoBehaviour
{

    private Quaternion _rot;
    [SerializeField] private GameObject _player;
    private Vector3 _offset;

    private void Start()
    {
        _rot = transform.rotation;
        _offset = transform.position - _player.transform.position;

    }

    void Update()
    {
        transform.rotation = _rot;
        transform.position = _offset + _player.transform.position;
    }
}
