using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShopSlot : UI_Entity
{
    UI_Shop shopUI;
    UI_ShopPurchase shopPurchase;
    UI_ShopSell shopSell;
    UI_ShopRepurchase shopRepurchase;
    public Enum_ShopSlotTypes currentType;

    // 현재 슬롯
    Image _iconImg;
    GameObject _amountText;
    public int Index { get; set; }

    public enum Enum_ShopSlotTypes
    {
        Purchase,
        Sell,
        Repurchase
    }


    enum Enum_UI_ShopSlot
    {
        SlotImg,
        IconImg
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_ShopSlot);
    }

    protected override void Init()
    {
        base.Init();
        _iconImg = _entities[(int)Enum_UI_ShopSlot.IconImg].GetComponent<Image>();
        shopUI = transform.GetComponentInParent<UI_Shop>();
        shopPurchase = transform.GetComponentInParent<UI_ShopPurchase>();
        shopSell = transform.GetComponentInParent<UI_ShopSell>();
        shopRepurchase = transform.GetComponentInParent<UI_ShopRepurchase>();
        _amountText = _iconImg.transform.GetChild(0).gameObject;

        if (shopPurchase != null)
        {
            currentType = Enum_ShopSlotTypes.Purchase;
        }
        else if (shopSell != null)
        {
            currentType = Enum_ShopSlotTypes.Sell;
        }
        else if (shopRepurchase != null)
        {
            currentType = Enum_ShopSlotTypes.Repurchase;
        }

        // 커서가 들어오면 아이템 설명 이미지 띄우기
        _entities[(int)Enum_UI_ShopSlot.IconImg].PointerEnterAction = (PointerEventData data) =>
        {
            if (!CheckItemNull())
            {
                shopUI.descrPanel.SetActive(true);
                ShowItemInfo();
                shopUI.RestrictItemDescrPos();
            }
        };

        // 커서가 나갔을때 아이템 설명 내리기
        _entities[(int)Enum_UI_ShopSlot.IconImg].PointerExitAction = (PointerEventData data) =>
        {
            shopUI.descrPanel.SetActive(false);
            shopUI.StopRestrictItemDescrPos(data);
        };

        // 아이템 우클릭
        _entities[(int)Enum_UI_ShopSlot.IconImg].ClickAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            if (data.button == PointerEventData.InputButton.Right)
            {
                switch (currentType)
                {
                    case Enum_ShopSlotTypes.Purchase: // 장바구니 담기
                        if (shopPurchase.AfterPurchaseGold <= 0)
                        {
                            GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmY);
                            GameManager.UI.InGameConfirmY.ChangeText(UI_InGameConfirmY.Enum_ConfirmTypes.NotEnoughMoney);
                            return;
                        }

                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            // 몇개 담을지 묻는 팝업
                            GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmYN);
                            GameManager.UI.InGameConfirmYN.ChangeText(UI_InGameConfirmYN.Enum_ConfirmTypes.PutInShopBasket, Index);
                        }
                        else
                        {
                            // 장바구니에 1개 담기
                            shopPurchase.AddItemInShopBasket(Index);
                            shopPurchase.UpdateGoldPanel();
                        }
                        break;
                    case Enum_ShopSlotTypes.Sell: // 인벤토리로 되돌리기
                        shopUI.ShopToInven(Enum_ShopSlotTypes.Sell, Index);
                        break;
                    case Enum_ShopSlotTypes.Repurchase:
                        shopUI.ShopToInven(Enum_ShopSlotTypes.Repurchase, Index);
                        break;
                    default:
                        break;
                }
            }
        };

        switch (currentType)
        {
            case Enum_ShopSlotTypes.Sell:
                if (Index == shopSell.shopTotalCount - 1)
                {
                    shopSell.gameObject.SetActive(false);
                }
                break;
            case Enum_ShopSlotTypes.Repurchase:
                if (Index == shopRepurchase.shopTotalCount - 1)
                {
                    shopRepurchase.gameObject.SetActive(false);
                    shopUI.gameObject.SetActive(false);
                }
                break;
            default:
                break;
        }
    }

    // 슬롯 번호에 맞게 아이템 그리기
    public void ItemRender()
    {
        switch (currentType)
        {
            case Enum_ShopSlotTypes.Purchase:
                _amountText.SetActive(false);
                if (Index < shopPurchase.shopItemList.Length)
                {
                    transform.GetChild(1).GetComponent<TMP_Text>().text = shopPurchase.shopItems[Index].name; // 이름
                    transform.GetChild(2).GetComponent<TMP_Text>().text = $"{shopPurchase.shopItemList[Index].itemPrice}"; // 구매 가격
                    _iconImg.color = new Color32(255, 255, 255, 255);
                    _iconImg.sprite = shopPurchase.shopItems[Index].icon;
                }
                else
                {
                    _iconImg.sprite = null;
                    _iconImg.color = new Color32(56, 58, 72, 230);
                    shopUI.descrPanel.SetActive(false);
                    // 표기 비활성화
                    for (int j = 1; j < 5; j++)
                    {
                        transform.GetChild(j).gameObject.SetActive(false);
                    }
                }
                break;
            case Enum_ShopSlotTypes.Sell:
                if (shopSell.shopItems[Index] != null)
                {
                    // 아이콘 외 요소 활성화
                    for (int j = 1; j < 5; j++)
                    {
                        transform.GetChild(j).gameObject.SetActive(true);
                    }
                    transform.GetChild(1).GetComponent<TMP_Text>().text = shopSell.shopItems[Index].name;
                    transform.GetChild(2).GetComponent<TMP_Text>().text = (shopSell.shopItems[Index].sellingprice * shopSell.shopItems[Index].count).ToString();
                    _iconImg.color = new Color32(255, 255, 255, 255);
                    _iconImg.sprite = shopSell.shopItems[Index].icon;
                    // 장비 타입은 수량 고정1 이라 수량 표기X
                    if (shopSell.shopItems[Index].itemType == Enum_ItemType.Equipment)
                    {
                        _amountText.SetActive(false);
                    }
                    else
                    {
                        _amountText.SetActive(true);
                        _amountText.GetComponent<TMP_Text>().text = $"{shopSell.shopItems[Index].count}";
                    }
                }
                else
                {
                    // 아이콘 외 요소 비활성화
                    for (int j = 1; j < 5; j++)
                    {
                        transform.GetChild(j).gameObject.SetActive(false);
                    }
                    _iconImg.sprite = null;
                    _iconImg.color = new Color32(56, 58, 72, 230);
                    shopUI.descrPanel.SetActive(false);
                }
                break;
            case Enum_ShopSlotTypes.Repurchase:
                if (shopRepurchase.tempSoldItems[Index] != null)
                {
                    // 아이콘 외 요소 활성화
                    for (int j = 1; j < 5; j++)
                    {
                        transform.GetChild(j).gameObject.SetActive(true);
                    }
                    transform.GetChild(1).GetComponent<TMP_Text>().text = shopRepurchase.tempSoldItems[Index].name;
                    transform.GetChild(2).GetComponent<TMP_Text>().text = shopRepurchase.tempSoldItems[Index].sellingprice.ToString();
                    _iconImg.color = new Color32(255, 255, 255, 255);
                    _iconImg.sprite = shopRepurchase.tempSoldItems[Index].icon;
                    // 장비 타입은 수량 고정1 이라 수량 표기X
                    if (shopRepurchase.tempSoldItems[Index].itemType == Enum_ItemType.Equipment)
                    {
                        _amountText.SetActive(false);
                    }
                    else
                    {
                        _amountText.SetActive(true);
                        _amountText.GetComponent<TMP_Text>().text = $"{shopRepurchase.tempSoldItems[Index].count}";
                    }
                }
                else
                {
                    // 아이콘 외 요소 비활성화
                    for (int j = 1; j < 5; j++)
                    {
                        transform.GetChild(j).gameObject.SetActive(false);
                    }
                    _iconImg.sprite = null;
                    _iconImg.color = new Color32(56, 58, 72, 230);
                    shopUI.descrPanel.SetActive(false);
                }
                break;
            default:
                break;
        }
    }

    bool CheckItemNull()
    {
        switch (currentType)
        {
            case Enum_ShopSlotTypes.Purchase:
                return shopPurchase.shopItems[Index] == null;
            case Enum_ShopSlotTypes.Sell:
                return shopSell.shopItems[Index] == null;
            case Enum_ShopSlotTypes.Repurchase:
                return shopRepurchase.tempSoldItems[Index] == null;
            default:
                return true;
        }
    }

    void ShowItemInfo()
    {
        switch (currentType)
        {
            case Enum_ShopSlotTypes.Purchase:
                shopUI.descrPanel.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = shopPurchase.shopItems[Index].name; // 아이템 이름
                shopUI.descrPanel.transform.GetChild(1).GetComponent<Image>().sprite = _iconImg.sprite; // 아이콘 이미지

                if (shopPurchase.shopItems[Index].itemType == Enum_ItemType.Equipment) // 장비아이템 설명
                {
                    StateItemData itemData = GameManager.Data.itemDatas[shopPurchase.shopItems[Index].id] as StateItemData;
                    int[] stats = { itemData.level, itemData.attack, itemData.defense, itemData.speed, itemData.attackSpeed, itemData.maxHp, itemData.maxMp };
                    string descLines = string.Format(shopPurchase.shopItems[Index].desc, $"{itemData.level}\n", $"{itemData.attack}\n", $"{itemData.defense}\n", $"{itemData.speed}\n", $"{itemData.attackSpeed}\n", $"{itemData.maxHp}\n", $"{itemData.maxMp}\n");
                    string[] lines = descLines.Split("\n");

                    string desc = $"{lines[0]} \n";
                    for (int i = 1; i < lines.Length - 1; i++)
                    {
                        if (stats[i] == 0)
                        {
                            continue;
                        }
                        desc += $"{lines[i]} \n";
                    }

                    shopUI.descrPanel.transform.GetChild(2).GetComponentInChildren<TMP_Text>().text = desc;
                }
                else
                {
                    shopUI.descrPanel.transform.GetChild(2).GetComponentInChildren<TMP_Text>().text =
                    shopPurchase.shopItems[Index].desc;
                }
                break;
            case Enum_ShopSlotTypes.Sell:
                shopUI.descrPanel.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = shopSell.shopItems[Index].name; // 아이템 이름
                shopUI.descrPanel.transform.GetChild(1).GetComponent<Image>().sprite = _iconImg.sprite; // 아이콘 이미지

                if (shopSell.shopItems[Index].itemType == Enum_ItemType.Equipment) // 장비아이템 설명
                {
                    StateItemData itemData = GameManager.Data.itemDatas[shopSell.shopItems[Index].id] as StateItemData;
                    int[] stats = { itemData.level, itemData.attack, itemData.defense, itemData.speed, itemData.attackSpeed, itemData.maxHp, itemData.maxMp };
                    string descLines = string.Format(shopSell.shopItems[Index].desc, $"{itemData.level}\n", $"{itemData.attack}\n", $"{itemData.defense}\n", $"{itemData.speed}\n", $"{itemData.attackSpeed}\n", $"{itemData.maxHp}\n", $"{itemData.maxMp}\n");
                    string[] lines = descLines.Split("\n");

                    string desc = $"{lines[0]} \n";
                    for (int i = 1; i < lines.Length - 1; i++)
                    {
                        if (stats[i] == 0)
                        {
                            continue;
                        }
                        desc += $"{lines[i]} \n";
                    }

                    shopUI.descrPanel.transform.GetChild(2).GetComponentInChildren<TMP_Text>().text = desc;
                }
                else
                {
                    shopUI.descrPanel.transform.GetChild(2).GetComponentInChildren<TMP_Text>().text =
                    shopSell.shopItems[Index].desc;
                }
                break;
            case Enum_ShopSlotTypes.Repurchase:
                shopUI.descrPanel.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = shopRepurchase.tempSoldItems[Index].name; // 아이템 이름
                shopUI.descrPanel.transform.GetChild(1).GetComponent<Image>().sprite = _iconImg.sprite; // 아이콘 이미지

                if (shopRepurchase.tempSoldItems[Index].itemType == Enum_ItemType.Equipment) // 장비아이템 설명
                {
                    StateItemData itemData = GameManager.Data.itemDatas[shopRepurchase.tempSoldItems[Index].id] as StateItemData;
                    int[] stats = { itemData.level, itemData.attack, itemData.defense, itemData.speed, itemData.attackSpeed, itemData.maxHp, itemData.maxMp };
                    string descLines = string.Format(shopRepurchase.tempSoldItems[Index].desc, $"{itemData.level}\n", $"{itemData.attack}\n", $"{itemData.defense}\n", $"{itemData.speed}\n", $"{itemData.attackSpeed}\n", $"{itemData.maxHp}\n", $"{itemData.maxMp}\n");
                    string[] lines = descLines.Split("\n");

                    string desc = $"{lines[0]} \n";
                    for (int i = 1; i < lines.Length - 1; i++)
                    {
                        if (stats[i] == 0)
                        {
                            continue;
                        }
                        desc += $"{lines[i]} \n";
                    }

                    shopUI.descrPanel.transform.GetChild(2).GetComponentInChildren<TMP_Text>().text = desc;
                }
                else
                {
                    shopUI.descrPanel.transform.GetChild(2).GetComponentInChildren<TMP_Text>().text =
                    shopRepurchase.tempSoldItems[Index].desc;
                }
                break;
            default:
                break;
        }
    }
}
