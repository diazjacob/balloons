﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : ManagedMonobehaviour
{
    [SerializeField] private ItemSO[] _allItems; //holding all possible items
    [Space]
    [Space]
    [SerializeField] private GameObject[] _itemContainers; //All the parent objects
    
    private InventorySlot[] _inventorySlots; //All the runtime inventory slot data structures

    private void Awake()
    {
        GetAllRefs();
    }

    private void Start()
    {
        InitalizeInventory();

        //DEBUG

        _inventorySlots[1].SetItem(Items.PersonalBeacon);

        _inventorySlots[4].SetItem(Items.PersonalBeacon);

        //DEBUG
    }

    private void InitalizeInventory()
    {
        _inventorySlots = new InventorySlot[_itemContainers.Length];

        GameObject[] _currentItems;

        InventorySlot _currentSlot;
        for (int i = 0; i < _inventorySlots.Length; i++) //for each slot
        {
            _currentItems = new GameObject[_allItems.Length];
            for (int j = 0; j < _allItems.Length; j++)  //Create the items and hide them, so we can easily toggle what we want
            {
                GameObject obj = Instantiate(_allItems[j].GetPrefabRef(), _itemContainers[i].transform);
                obj.transform.parent = _itemContainers[i].transform;

                _currentItems[j] = obj;
            }


            _currentSlot = new InventorySlot(_currentItems);

            _inventorySlots[i] = _currentSlot;

            _inventorySlots[i].Reset();
        }
    }
    public bool IsFull() //If the player's inventory is full
    {
        bool isFull = true;

        for(int i = 0; i < _inventorySlots.Length && isFull; i++)
        {
            //print(_inventorySlots[i].GetItem());
            if (_inventorySlots[i].GetItem() == Items.None) isFull = false;
        }

        return isFull;
    }

    public bool AddItem(Items type)
    {
        bool result = false;
        int location = -1;

        for(int i = 0; i < _allItems.Length && !result; i++)
        {
            result = _inventorySlots[i].GetItem() == Items.None;
            location = i;
        }

        print("Adding Item to slot: " +location);

        if(result)
        {
            _inventorySlots[location].SetItem(type);
        }

        return result;
    }

    public int AddItem(PackageType mailType)
    {
        int location = -1;

        for (int i = 0; i < _inventorySlots.Length && location == -1; i++)
        {
            if(_inventorySlots[i].GetItem() == Items.None) location = i;
        }

        if (location != -1)
        {
            Items realType = Items.None;

            switch (mailType)
            {
                case PackageType.Letter:
                    realType = Items.MailLetter;
                    break;
                case PackageType.Parcel:
                    realType = Items.MailParcel;
                    break;
                case PackageType.Package:
                    realType = Items.MailPackage;
                    break;
                case PackageType.Crate:
                    realType = Items.MailCrate;
                    break;
            }

            _inventorySlots[location].SetItem(realType);
        }

        return location;
    }

    private int RefToInt(GameObject container)
    {
        int index = -1;
        for (int i = 0; i < _itemContainers.Length && index == -1; i++)
        {
            if (_itemContainers[i] == container) index = i;
        }

        if (index == -1) //Can't have an empty slot
        {
            Debug.LogError("ERROR: Could not find inventory slot by object refrence.");
        }

        return index;
    }

    //private void AddItem(GameObject container) //Buy or sell and item based on the slot obj ref
    //{
    //    int index = RefToInt(container);

    //    if (index != -1)
    //    {
    //        Items item = _inventorySlots[index].GetItem();

    //        if (item != Items.None)
    //        {
    //            ItemSO itemRef = null;

    //            for (int i = 0; i < _allItems.Length && itemRef == null; i++)
    //            {
    //                if (_allItems[i].GetType() == item) itemRef = _allItems[i];
    //            }

    //            int cost = 0;

    //            if (buying) cost = itemRef.GetCost();
    //            else cost = -itemRef.GetSell();
    //            //SELL ITEM HERE

    //            MPlayer().GetPlayerStats().IncrementGold(cost);

    //            _inventorySlots[index].SetItem(Items.None);
    //        }
    //    }
    //    else Debug.Log("ERROR: Copuld Not Process BuySellItem.");
    //}

    //Use an item, returns if the inventory should be closed
    public bool UseItem(GameObject container)
    {
        bool invClosed = false;


        int index = RefToInt(container);
        if(index != -1)
        {
            Items itemType = _inventorySlots[index].GetItem();
            if (itemType != Items.None)
            {

                Debug.Log("Using item: " + itemType.ToString());

                if (itemType == Items.PersonalBeacon)  //Handle each item usage acordingly
                {
                    MObstacleManager().SendBeacon();
                }
                else if (itemType == Items.PatchKit)
                {

                }
                else if(itemType == Items.MailPackage || itemType == Items.MailLetter 
                        || itemType == Items.MailCrate || itemType == Items.MailParcel)
                {
                    if (!MPlayer().GetLanded()) MObstacleManager().SendMailBeacon(MPlayer().GetMailFromRef(index));
                    else MPlayer().CheckRemoveMail(MPlayer().GetMailFromRef(index));
                }
                
            }

            _inventorySlots[index].SetItem(Items.None);
            invClosed = true;
        }
        else Debug.Log("ERROR: Copuld Not Process UseItem.");


        return invClosed;
    }

    //We will also need a function to just remove an item
}

[System.Serializable]
public class InventorySlot
{
    [SerializeField] private GameObject[] _inventoryItems;

    private int _currentItemIndex; //equal to the 'Items' enum order subtract 1 (so -1 is empty)   :D

    public InventorySlot (GameObject[] items) 
    {
        _currentItemIndex = -1;
        _inventoryItems = items; 
    }


    public Items GetItem() { return (Items)(_currentItemIndex + 1); }

    public void SetItem(Items selectedItem)
    {
        Reset();

        int index = ((int)selectedItem) - 1;
        if (index >= 0) _inventoryItems[index].SetActive(true);

        _currentItemIndex = index;
    }
    
    public void Reset() {for (int i = 0; i < _inventoryItems.Length; i++) _inventoryItems[i].SetActive(false);}

}

public enum Items //All our items, DO NOT change the order of this enum. And keep 'None' first always.
{
    None,
    PersonalBeacon,
    PatchKit,
    MailLetter,
    MailParcel,
    MailPackage,
    MailCrate
}

