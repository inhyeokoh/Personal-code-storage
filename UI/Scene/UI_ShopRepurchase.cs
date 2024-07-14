using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UI_ShopRepurchase : UI_Entity
{
    GameObject shopSlots;
    UI_Shop shopUI;

    public ItemData[] tempSoldItems;
    public int shopTotalCount; // 물품 담을 수 있는 칸 수

    enum Enum_UI_ShopRePurchase
    {
        ShopSlots
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_ShopRePurchase);
    }

    private void OnEnable()
    {
        _UpdateSlotUIs();        
    }

/*    private void OnDisable()
    {
        GameManager.UI.PointerOnUI(false);
    }*/

    protected override void Init()
    {
        base.Init();
        shopSlots = _entities[(int)Enum_UI_ShopRePurchase.ShopSlots].gameObject;
        shopUI = transform.GetComponentInParent<UI_Shop>();
        shopTotalCount = 10;

        _DrawSlots();
    }

    void _DrawSlots()
    {
        tempSoldItems = new ItemData[shopTotalCount];

        for (int i = 0; i < shopTotalCount; i++)
        {
            GameObject _shopSlot = GameManager.Resources.Instantiate("Prefabs/UI/Scene/ShopSlot", shopSlots.transform);
            _shopSlot.GetComponent<UI_ShopSlot>().Index = i;
        }
    }

    void _UpdateSlotUIs()
    {
        for (int i = 0; i < shopTotalCount; i++)
        {
            shopSlots.transform.GetChild(i).GetComponent<UI_ShopSlot>().ItemRender();
        }
    }


    public void EmptyTempForSold()
    {
        tempSoldItems.Initialize();
    }
}

