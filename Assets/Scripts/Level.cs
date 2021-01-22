using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Level : MonoBehaviour
{
    [SerializeField] private Material _mat;
    [SerializeField] private MeshRenderer _meshRend;

    public void SetMat(Material m)
    { 
        _mat = m;
        _meshRend.material = _mat;
    }
    public void SetColor(Color c) 
    {
        _meshRend.material.SetColor("_MainColor", c); 
    }
    public void SetHeight(float i)
    {
        _meshRend.material.SetFloat("_Height", i);
    }

    public void SetHeightMap(Texture2D heightMap)
    {
        //_mat.mainTexture = heightMap;

        _meshRend.material.SetTexture("_MainTex", heightMap);

    }

    private void Awake()
    {
        _meshRend = GetComponent<MeshRenderer>();
    }
}
