using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : UI_Entity
{
    public Image highlightImg;
    UI_Inventory _inven;
    List<ItemData> _invenItems;

    #region 현재 슬롯 관련 필드
    Image _iconImg;
    GameObject _amountText;
    public int Index { get; set; }
    #endregion

    int _otherIndex; // 드롭 시 위치한 슬롯

    enum Enum_UI_ItemSlot
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
        _invenItems = GameManager.Inven.items;
        _inven = transform.GetComponentInParent<UI_Inventory>();
        ItemRender();

        //드래그 시작
        _entities[(int)Enum_UI_ItemSlot.IconImg].BeginDragAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return; // 비어있는 칸 드래그 거부

            GameManager.UI.GetPopupForward(GameManager.UI.Inventory);
            _inven.dragImg.gameObject.SetActive(true);
            _inven.dragImg.sprite = _iconImg.sprite;

        };
        //드래그 중
        _entities[(int)Enum_UI_ItemSlot.IconImg].DragAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            _inven.dragImg.transform.position = data.position;
        };
        //드래그 끝
        _entities[(int)Enum_UI_ItemSlot.IconImg].EndDragAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;
            
            if (!_inven.CheckUIOutDrop() && CheckSlotDrop(data)) // 인벤토리 내 드롭 + 슬롯 안에 정확히 드롭한 경우
            {
                _otherIndex = data.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<UI_ItemSlot>().Index;
                GameManager.Inven.DragAndDropItems(Index, _otherIndex);
            }
            else if (_inven.CheckUIOutDrop()) // 인벤토리 밖에 드롭한 경우
            {
                if (CheckSlotDrop(data)) // 플레이어 정보창 장비 슬롯에 드롭한 경우
                {
                    _otherIndex = data.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<UI_EquipSlot>().Index;
                    GameManager.Inven.InvenToEquipSlot(Index, _otherIndex);
                }
                else
                {
                    if (_invenItems[Index].count == 1)
                    {
                        // 버릴지 되묻는 팝업
                        GameManager.UI.InGameConfirmYN.ChangeText(UI_InGameConfirmYN.Enum_ConfirmTypes.InvenSingleDrop, Index);
                        GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmYN);
                    }
                    else
                    {
                        // 버릴 아이템 이름 + 수량 적는 팝업
                        GameManager.UI.InGameConfirmYN.ChangeText(UI_InGameConfirmYN.Enum_ConfirmTypes.InvenPluralDrop, Index);
                        GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmYN);
                    }
                }
            }

            _inven.dragImg.gameObject.SetActive(false);
        };

        // 커서가 들어오면 아이템 설명 이미지 띄우기 + 하이라이트 효과
        _entities[(int)Enum_UI_ItemSlot.IconImg].PointerEnterAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            _inven.descrPanel.SetActive(true);
            highlightImg.color = new Color(highlightImg.color.r, highlightImg.color.g, highlightImg.color.b, 0.4f);
            _inven.highlightedSlotIndex = Index;
            ShowItemInfo();
            _inven.RestrictItemDescrPos();
        };

        // 커서가 나갔을때 아이템 설명 내리기 + 하이라이트 효과 끄기
        _entities[(int)Enum_UI_ItemSlot.IconImg].PointerExitAction = (PointerEventData data) =>
        {
            if(CheckItemNull()) return;

            highlightImg.color = new Color(highlightImg.color.r, highlightImg.color.g, highlightImg.color.b, 0f);
            _inven.highlightedSlotIndex = null;
            _inven.RemoveCursorOnEffectAtItemSlot();
        };

        // 아이템 우클릭 (장비 장착, 아이템 판매)
        _entities[(int)Enum_UI_ItemSlot.IconImg].ClickAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            if (data.button == PointerEventData.InputButton.Right)
            {
                if (GameManager.UI.Shop.gameObject.activeSelf) // 상점 진입한 상태에서 우클릭한 경우, 아이템을 상점 판매탭 물품으로 이동
                {
                    GameManager.UI.Shop.panel_U_Buttons[1].isOn = true; // 판매탭 활성화
                    GameManager.Inven.InvenToShop(Index);
                }
                else
                {
                    if (_invenItems[Index].itemType == Enum_ItemType.Equipment) // 장비에 우클릭 한 경우
                    {
                        GameManager.Inven.EquipItem(Index);
                    }
                    else if (_invenItems[Index].itemType == Enum_ItemType.Consumption)
                    {
                        GameManager.Inven.ConsumeItem(Index);
                    }
                }
            }
        };

        _inven.AddListenerToItemTypeToggle();
    }

    /// <summary>
    /// 슬롯 Index에 맞게 아이템 아이콘, 수량 텍스트, 하이라이트 효과 그리기
    /// </summary>
    public void ItemRender()
    {
        if (_invenItems[Index] != null)
        {
            _iconImg.color = new Color32(255, 255, 255, 255);
            _iconImg.sprite = _invenItems[Index].icon;
            // 장비 타입은 수량 고정1 이라 수량 표기X
            if (_invenItems[Index].itemType == Enum_ItemType.Equipment)
            {
                _amountText.SetActive(false);
            }
            else
            {
                _amountText.SetActive(true);
                _amountText.GetComponent<TMP_Text>().text = $"{_invenItems[Index].count}";
            }
        }
        else
        {
            _iconImg.sprite = null;
            _iconImg.color = new Color32(12, 15, 29, 0);
            highlightImg.color = new Color(highlightImg.color.r, highlightImg.color.g, highlightImg.color.b, 0f);
            _inven.descrPanel.SetActive(false);
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

    bool CheckItemNull()
    {
        return GameManager.Inven.items[Index] == null;
    }

    /// <summary>
    /// 드래그 이후 드롭 시, 슬롯에 벗어나지 않았는지 확인
    /// </summary>
    bool CheckSlotDrop(PointerEventData data)
    {
        if (data.pointerCurrentRaycast.gameObject == null)
        {
            return false;
        }

        return data.pointerCurrentRaycast.gameObject.name == "IconImg";
    }

    void ShowItemInfo()
    {
        _inven.descrPanelItemNameText.text = GameManager.Inven.items[Index].name;
        _inven.descrPanelItemImage.sprite = _iconImg.sprite;

        if (GameManager.Inven.items[Index].itemType == Enum_ItemType.Equipment) // 장비아이템 유효한 스탯만 표기
        {
            StateItemData itemData = GameManager.Data.itemDatas[GameManager.Inven.items[Index].id] as StateItemData;
            if (itemData != null)
            {
                int[] stats = {itemData.level, itemData.attack, itemData.defense, itemData.speed, itemData.attackSpeed, itemData.maxHp, itemData.maxMp};
                string descLines = string.Format(GameManager.Inven.items[Index].desc, $"{itemData.level}\n", $"{itemData.attack}\n", $"{itemData.defense}\n",
                    $"{itemData.speed}\n", $"{itemData.attackSpeed}\n", $"{itemData.maxHp}\n", $"{itemData.maxMp}\n");
                string[] lines = descLines.Split("\n");

                string desc = $"{lines[0]} \n";
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    if (stats[i] == 0) continue;
                    desc += $"{lines[i]} \n";
                }

                _inven.descrPanelDescrText.text = desc;
            }
        }
        else
        {
            _inven.descrPanelDescrText.text = GameManager.Inven.items[Index].desc;
        }
    }
}
