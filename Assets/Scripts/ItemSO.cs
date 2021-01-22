using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Create New Item")]
public class ItemSO : ScriptableObject
{
    private static float SELL_RATIO;

    [SerializeField] private GameObject _prefab;
    [SerializeField] private Items _type;
    [SerializeField] private int _cost;
    [Space]
    [SerializeField] private string _itemName;
    [TextArea]
    [SerializeField] private string _itemDescription;

    public GameObject GetPrefabRef() { return _prefab; }
    public Items GetType() { return _type; }

    public int GetCost() { return _cost; }
    public int GetSell() { return (int)(_cost * SELL_RATIO); }

    public string GetName() { return _itemName; }
    public string GetDescription() { return _itemDescription; }
}


