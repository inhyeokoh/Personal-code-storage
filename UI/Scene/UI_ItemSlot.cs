using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UI_ItemSlot : UI_Entity
{
    protected ItemData item;
    public Image highlightImg;

    #region 현재 슬롯 관련 필드
    protected Image _iconImg;
    GameObject _amountText;
    public int Index { get; set; }
    #endregion

    protected int _otherIndex; // 드롭 시 위치한 슬롯

    protected enum Enum_UI_ItemSlot
    {
        SlotImg,
        HighlightImg,
        IconImg
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_ItemSlot);
    }

    protected override void Init()
    {
        base.Init();
        _iconImg = _entities[(int)Enum_UI_ItemSlot.IconImg].GetComponent<Image>();
        highlightImg = _entities[(int)Enum_UI_ItemSlot.HighlightImg].GetComponent<Image>();
        _amountText = _iconImg.transform.GetChild(0).gameObject;
        ItemRender();               
    }

    protected abstract ItemData GetItem();

    /// <summary>
    /// 슬롯 Index에 맞게 아이템 아이콘, 수량 텍스트, 하이라이트 효과 그리기
    /// </summary>
    public void ItemRender()
    {
        item = GetItem();
        if (item != null)
        {
            _iconImg.color = new Color32(255, 255, 255, 255);
            _iconImg.sprite = item.icon;
            // 장비 타입은 수량 고정1 이라 수량 표기X
            if (item.itemType == Enum_ItemType.Equipment)
            {
                _amountText.SetActive(false);
            }
            else
            {
                _amountText.SetActive(true);
                _amountText.GetComponent<TMP_Text>().text = $"{item.count}";
            }
        }
        else
        {
            _iconImg.sprite = null;
            _iconImg.color = new Color32(12, 15, 29, 0);
            highlightImg.color = new Color(highlightImg.color.r, highlightImg.color.g, highlightImg.color.b, 0f);
            _amountText.gameObject.SetActive(false);
        }
    }

    public void RenderBright()
    {
        _iconImg.color = new Color32(255, 255, 255, 255);
    }

    public void RenderDark()
    {
        _iconImg.color = new Color32(50, 50, 50, 255);
    }

    protected bool CheckItemNull()
    {
        return item == null;
    }

    /// <summary>
    /// 드래그 이후 드롭 시, 슬롯에 벗어나지 않았는지 확인
    /// </summary>
    protected bool CheckSlotDrop(PointerEventData data)
    {
        if (data.pointerCurrentRaycast.gameObject == null)
        {
            return false;
        }

        return data.pointerCurrentRaycast.gameObject.name == "IconImg";
    }
}
