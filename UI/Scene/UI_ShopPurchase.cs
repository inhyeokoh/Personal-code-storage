using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public struct ShopItem
{
    public int itemID;
    public int itemPrice;
    public Sprite iconSprite;
}

public class UI_ShopPurchase : UI_Entity
{
    GameObject shopSlots;
    GameObject goldPanel;
    GameObject basket;

    public ItemData[] shopItems;
    public ShopItem[] shopItemList;
    public int shopTotalCount = 8; // 물품 담을 수 있는 칸 수
    public ItemData[] shopBasketItems;
    public int shopBasketCount = 6;

    long _totalPurchaseGold;
    public long AfterPurchaseGold { get; private set; }
    enum Enum_UI_ShopPurchase
    {
        ShopSlots,
        GoldPanel,
        Basket,
        Purchase,
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
        basket = _entities[(int)Enum_UI_ShopPurchase.Basket].gameObject;
        shopTotalCount = 8;
        shopBasketCount = 6;
        shopBasketItems = new ItemData[shopBasketCount];

        _DrawSlots();
        UpdateGoldPanel();

        // 구매 (장바구니 품목 -> 인벤토리)
        _entities[(int)Enum_UI_ShopPurchase.Purchase].ClickAction = (PointerEventData data) =>
        {
            for (int i = 0; i < shopBasketCount; i++)
            {
                GameManager.Inven.GetItem(shopBasketItems[i]);
                EmptyBasketSlot(i);
            }

            GameManager.Inven.Gold = AfterPurchaseGold;
            UpdateGoldPanel();
        };

        // 장바구니 비우기
        _entities[(int)Enum_UI_ShopPurchase.Reset].ClickAction = (PointerEventData data) =>
        {
            for (int i = 0; i < shopBasketCount; i++)
            {
                EmptyBasketSlot(i);
            }
            UpdateGoldPanel();
        };
    }

    void _DrawSlots()
    {
        for (int i = 0; i < shopTotalCount; i++)
        {
            GameObject _shopSlot = GameManager.Resources.Instantiate("Prefabs/UI/Scene/ShopSlot", shopSlots.transform);
            _shopSlot.GetComponent<UI_ShopSlot>().Index = i;
        }

        for (int i = 0; i < shopBasketCount; i++)
        {
            GameObject shopBasketSlot = GameManager.Resources.Instantiate("Prefabs/UI/Scene/ShopBasketSlot", basket.transform);
            shopBasketSlot.GetComponent<UI_ShopBasketSlot>().index = i;
        }
    }


    public void DrawSellingItems(int npcID)
    {
        shopItemList = GameManager.Data.shopDict[npcID];
        shopItems = new ItemData[shopItemList.Length];

        for (int i = 0; i < shopItemList.Length; i++)
        {
            // id에 해당하는 아이템 참조
            int id = shopItemList[i].itemID;
            shopItems[i] = GameManager.Data.itemDatas[id];
        }
    }

    // 골드 계산 갱신
    public void UpdateGoldPanel()
    {
        _totalPurchaseGold = 0;
        for (int i = 0; i < shopBasketItems.Length; i++)
        {
            if (shopBasketItems[i] == null) continue;

            _totalPurchaseGold += shopItemList[i].itemPrice * shopBasketItems[i].count;
        }

        goldPanel.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().text = _totalPurchaseGold.ToString();
        AfterPurchaseGold = GameManager.Inven.Gold - _totalPurchaseGold;
        goldPanel.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = AfterPurchaseGold.ToString();
    }

    // 장바구니 UI 갱신
    public void UpdateShopBasketUI(int slotIndex)
    {
        UI_ShopBasketSlot shopBasketSlot = basket.transform.GetChild(slotIndex).GetComponent<UI_ShopBasketSlot>();
        shopBasketSlot.ItemRender();
    }

    int GetEmptyBasketSlotIndex()
    {
        for (int i = 0; i < shopBasketCount; i++)
        {
            if (shopBasketItems[i] == null || shopBasketItems[i].count == 0)
            {
                return i;
            }
        }

        // 비어있는 슬롯 없음
        return -1;
    }

    public void AddItemInShopBasket(int clickedIndex, int count = 1)
    {
        var shopItem = shopItems[clickedIndex];
        // 획득한 아이템이 없는 경우
        if (shopItem == null) return;

        // 수량이 합산되지 않는 장비 아이템
        if (shopItem.itemType == Enum_ItemType.Equipment)
        {
            int emptySlotIndex = GetEmptyBasketSlotIndex();
            if (emptySlotIndex != -1)
            {
                shopBasketItems[emptySlotIndex] = new ItemData(shopItem, 1);
            }
            else  // 장바구니가 가득 찬 경우
            {
                // GameManager.UI.OpenPopup(GameManager.UI);
                // shopUI.notifyFull.transform.GetChild(0).GetComponent<UI_InGameNotify>().ChangeText(UI_InGameNotify.Enum_NotifyParent.Shop);
                return;
            }
        }
        else
        {
            // 획득 수량 전부 처리할때까지 반복
            while (count > 0)
            {
                bool itemAdded = false;
                for (int i = 0; i < shopBasketCount; i++)
                {
                    if (shopBasketItems[i] == null) continue;

                    // 동일 아이템 확인
                    if (shopBasketItems[i].name != null && shopBasketItems[i].id == shopItem.id)
                    {
                        // 칸에 최대 수량이 아닌 경우
                        if (shopBasketItems[i].count < shopBasketItems[i].maxCount)
                        {
                            int remainSpace = shopBasketItems[i].maxCount - shopBasketItems[i].count; // 잔여공간 크기
                            // 획득한 수량이 잔여 공간에 들어갈 수 있는 경우                                                                           
                            if (count <= remainSpace)
                            {
                                shopBasketItems[i].count += count;
                                count = 0;
                            }
                            else
                            {
                                shopBasketItems[i].count = shopBasketItems[i].maxCount;
                                count -= remainSpace;
                            }
                            itemAdded = true;
                            break;
                        }
                    }
                }
                // 동일 아이템이 없어 새 슬롯에 추가해야 하는 경우
                if (!itemAdded)
                {
                    int emptySlotIndex = GetEmptyBasketSlotIndex();
                    // 빈 슬롯을 찾은 경우
                    if (emptySlotIndex != -1)
                    {
                        shopBasketItems[emptySlotIndex] = new ItemData(shopItem, count);
                        // 획득 수량이 최대 수량 이하인 경우                                                           
                        if (count <= shopItem.maxCount)
                        {
                            shopBasketItems[emptySlotIndex].count = count;
                            count = 0;
                        }
                        else
                        {
                            shopBasketItems[emptySlotIndex].count = shopItem.maxCount;
                            count -= shopItem.maxCount;
                        }
                    }
                    else
                    {
                        // 더 이상 추가할 공간이 없으므로 사용자에게 알림
                        // shopUI.notifyFull.SetActive(true);
                        // shopUI.notifyFull.transform.GetChild(0).GetComponent<UI_InGameNotify>().ChangeText(UI_InGameNotify.Enum_NotifyParent.Shop);
                        break;
                    }
                }
            }
        }

        // UI 갱신
        for (int i = 0; i < shopBasketCount; i++)
        {
            UpdateShopBasketUI(i);
        }
    }

    // 장바구니 슬롯 비우기
    public void EmptyBasketSlot(int index)
    {
        shopBasketItems[index] = null;
        UpdateShopBasketUI(index);
    }

    void _UpdateSlotUIs()
    {
        for (int i = 0; i < shopTotalCount; i++)
        {
            shopSlots.transform.GetChild(i).GetComponent<UI_ShopSlot>().ItemRender();
        }
    }
}

