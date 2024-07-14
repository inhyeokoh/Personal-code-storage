using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UI_ShopSell : UI_Entity
{
    GameObject shopSlots;
    GameObject goldPanel;
    UI_Shop shopUI;

    public ItemData[] shopItems;
    public int shopTotalCount; // 물품 담을 수 있는 칸 수

    long _totalSellGold;
    long _afterSellGold;

    enum Enum_UI_ShopPurchase
    {
        ShopSlots,
        GoldPanel,
        Sell,
        Reset
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_ShopPurchase);
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
        shopSlots = _entities[(int)Enum_UI_ShopPurchase.ShopSlots].gameObject;
        goldPanel = _entities[(int)Enum_UI_ShopPurchase.GoldPanel].gameObject;

        shopUI = transform.GetComponentInParent<UI_Shop>();
        shopTotalCount = 10;

        _DrawSlots();        
        UpdateGoldPanel();

        // 판매 (인벤토리에서 우클릭 -> 판매 버튼 누른 상황)
        _entities[(int)Enum_UI_ShopPurchase.Sell].ClickAction = (PointerEventData data) =>
        {
            _SaveTempForSold();
            _EmptyShopSlot();
            GameManager.Inven.Gold = _afterSellGold;
            UpdateGoldPanel();
        };

        // 장바구니 비우기
        _entities[(int)Enum_UI_ShopPurchase.Reset].ClickAction = (PointerEventData data) =>
        {
            _EmptyShopSlot();
            ReturnSellListToInven();
            UpdateGoldPanel();
        };
    }

    void _DrawSlots()
    {
        shopItems = new ItemData[shopTotalCount];

        for (int i = 0; i < shopTotalCount; i++)
        {
            GameObject _shopSlot = GameManager.Resources.Instantiate("Prefabs/UI/Scene/ShopSlot", shopSlots.transform);
            _shopSlot.GetComponent<UI_ShopSlot>().Index = i;
        }
    }

    // 목록 초기화
    void _EmptyShopSlot()
    {
        for (int i = 0; i < shopItems.Length; i++)
        {
            shopItems[i] = null;
            transform.GetChild(0).GetChild(i).GetComponent<UI_ShopSlot>().ItemRender(); // 이미지 갱신
        }
    }

    // 골드 계산 갱신
    public void UpdateGoldPanel()
    {
        _totalSellGold = 0;
        foreach (var item in shopItems)
        {
            if (item == null) continue;

            _totalSellGold += item.sellingprice * item.count;
        }
        goldPanel.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = _totalSellGold.ToString();
        _afterSellGold = GameManager.Inven.Gold + _totalSellGold;
        goldPanel.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = _afterSellGold.ToString();
    }

    public int GetEmptySlotIndex()
    {
        for (int i = 0; i < shopTotalCount; i++)
        {
            if (shopItems[i] == null || shopItems[i].count == 0)
            {
                return i;
            }
        }

        // 비어있는 슬롯 없음
        return -1;
    }

    void _SaveTempForSold()
    {
        int index = 0;
        foreach (var item in shopItems)
        {
            if (item != null)
            {
                shopUI.shopRepurchase.tempSoldItems[index] = new ItemData(item, item.count);
                index++;
            }
        }
    }

    public void ReturnSellListToInven()
    {
        for (int i = 0; i < shopItems.Length; i++)
        {
            if (shopItems[i] != null)
            {
                shopUI.ShopToInven(UI_ShopSlot.Enum_ShopSlotTypes.Sell, i);
            }
        }
    }


    void _UpdateSlotUIs()
    {
        for (int i = 0; i < shopTotalCount; i++)
        {
            shopSlots.transform.GetChild(i).GetComponent<UI_ShopSlot>().ItemRender();
        }
    }
}

