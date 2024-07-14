using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ShopBasketSlot : UI_Entity
{
    UI_Shop shopUI;
    UI_ShopPurchase shopPurchase;
    Image _highlightImg;

    // 현재 슬롯
    Image _iconImg;
    GameObject _amountText;
    public int index;

    enum Enum_UI_ShopBasketSlot
    {
        SlotImg,
        HighlightImg,
        IconImg
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_ShopBasketSlot);
    }

    protected override void Init()
    {
        base.Init();
        _iconImg = _entities[(int)Enum_UI_ShopBasketSlot.IconImg].GetComponent<Image>();
        _highlightImg = _entities[(int)Enum_UI_ShopBasketSlot.HighlightImg].GetComponent<Image>();
        _amountText = _iconImg.transform.GetChild(0).gameObject;
        shopPurchase = transform.GetComponentInParent<UI_ShopPurchase>();
        shopUI = transform.GetComponentInParent<UI_Shop>();
        ItemRender();

        // 커서가 들어오면 아이템 설명 이미지 띄우기 + 하이라이트 효과
        _entities[(int)Enum_UI_ShopBasketSlot.IconImg].PointerEnterAction = (PointerEventData data) =>
        {
            if (!CheckItemNull())
            {
                shopUI.descrPanel.SetActive(true);
                _highlightImg.color = new Color(_highlightImg.color.r, _highlightImg.color.g, _highlightImg.color.b, 0.4f);
                ShowItemInfo();
                shopUI.RestrictItemDescrPos();
            }
        };

        // 커서가 나갔을때 아이템 설명 내리기 + 하이라이트 효과 끄기
        _entities[(int)Enum_UI_ShopBasketSlot.IconImg].PointerExitAction = (PointerEventData data) =>
        {
            _highlightImg.color = new Color(_highlightImg.color.r, _highlightImg.color.g, _highlightImg.color.b, 0f);
            shopUI.descrPanel.SetActive(false);
            shopUI.StopRestrictItemDescrPos(data);
        };

        // 우클릭으로 해당 칸 초기화
        _entities[(int)Enum_UI_ShopBasketSlot.IconImg].ClickAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;            

            if (data.button == PointerEventData.InputButton.Right)
            {
                // 칸 비우기
                shopPurchase.EmptyBasketSlot(index);
                shopPurchase.UpdateGoldPanel();
            }
        };

/*        // 주의
        if (index == shopPurchase.shopBasketCount - 1)
        {
            shopUI.gameObject.SetActive(false);
        }*/
    }

    // 슬롯 번호에 맞게 아이템 그리기
    public void ItemRender()
    {
        if (shopPurchase.shopBasketItems[index] != null)
        {
            _iconImg.sprite = shopPurchase.shopBasketItems[index].icon;
            _iconImg.color = new Color32(255, 255, 255, 255);
            // 장비 타입은 수량 고정1 이라 수량 표기X
            if (shopPurchase.shopBasketItems[index].itemType == Enum_ItemType.Equipment)
            {
                _amountText.SetActive(false);
            }
            else
            {
                _amountText.SetActive(true);
                _amountText.GetComponent<TMP_Text>().text = $"{shopPurchase.shopBasketItems[index].count}";
            }
        }
        else
        {
            _iconImg.sprite = null;
            _iconImg.color = new Color32(12, 15, 29, 0);
            _highlightImg.color = new Color(_highlightImg.color.r, _highlightImg.color.g, _highlightImg.color.b, 0f);
            shopUI.descrPanel.SetActive(false);
            _amountText.gameObject.SetActive(false);
        }
    }

    bool CheckItemNull()
    {
        return shopPurchase.shopBasketItems[index] == null;
    }

    void ShowItemInfo()
    {
        shopUI.descrPanel.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = shopPurchase.shopBasketItems[index].name; // 아이템 이름
        shopUI.descrPanel.transform.GetChild(1).GetComponent<Image>().sprite = _iconImg.sprite; // 아이콘 이미지

        if (shopPurchase.shopBasketItems[index].itemType == Enum_ItemType.Equipment) // 장비아이템 설명
        {
            StateItemData itemData = GameManager.Data.itemDatas[shopPurchase.shopBasketItems[index].id] as StateItemData;
            if (itemData != null)
            {
                int[] stats = { itemData.level, itemData.attack, itemData.defense, itemData.speed, itemData.attackSpeed, itemData.maxHp, itemData.maxMp };
                string descLines = string.Format(shopPurchase.shopBasketItems[index].desc, $"{itemData.level}\n", $"{itemData.attack}\n", $"{itemData.defense}\n", $"{itemData.speed}\n", $"{itemData.attackSpeed}\n", $"{itemData.maxHp}\n", $"{itemData.maxMp}\n");
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
        }
        else  // 장비 외 아이템 설명
        {
            shopUI.descrPanel.transform.GetChild(2).GetComponentInChildren<TMP_Text>().text =
            shopPurchase.shopBasketItems[index].desc;
        }
    }
}

