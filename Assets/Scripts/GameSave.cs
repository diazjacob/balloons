using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSave 
{
    public PlayerStatSave stats;


    public GameSave()
    {
        stats = new PlayerStatSave();
    }
}

[Serializable]
public class PlayerStatSave
{
    private int _gold;
    private int _numContracts;

    public PlayerStatSave()
    {
        _gold = 0;
        _numContracts = 0;
    }

    public void SetGold(int g) { _gold = g; }
    public int GetGold() { return _gold; }

    public void SetNumContracts(int c) { _numContracts = c; }
    public int GetContracts() { return _numContracts; }
}
