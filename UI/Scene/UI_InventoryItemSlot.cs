using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InventoryItemSlot : UI_ItemSlot
{
    List<ItemData> _invenItems;
    UI_Inventory _inven;

    protected override void Init()
    {
        base.Init();
        _inven = transform.GetComponentInParent<UI_Inventory>();

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
                _otherIndex = data.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<UI_InventoryItemSlot>().Index;
                GameManager.Inven.DragAndDropItems(Index, _otherIndex);
            }
            else if (_inven.CheckUIOutDrop()) // 인벤토리 밖에 드롭한 경우
            {
                if (CheckSlotDrop(data)) // 플레이어 정보창 장비 슬롯에 드롭한 경우
                {
                    Transform parentTr = data.pointerCurrentRaycast.gameObject.transform.parent;
                    if (parentTr.GetComponent<UI_EquipItemSlot>() != null)
                    {
                        _otherIndex = parentTr.GetComponent<UI_EquipItemSlot>().Index;
                        GameManager.Inven.InvenToEquipSlot(Index, _otherIndex);
                    }
                    else if (parentTr.GetComponent<UI_TradeItemSlot>() != null)
                    {
                        _otherIndex = parentTr.GetComponent<UI_TradeItemSlot>().Index;
                        GameManager.Inven.InvenToTradeSlot(Index, _otherIndex);
                    }
                }
                else // 필드에 버리는 경우
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

            highlightImg.color = new Color(highlightImg.color.r, highlightImg.color.g, highlightImg.color.b, 0.4f);
            _inven.highlightedSlot = this;
            GameManager.UI.itemToolTip.ShowItemInfo(item);
        };

        // 커서가 나갔을때 아이템 설명 내리기 + 하이라이트 효과 끄기
        _entities[(int)Enum_UI_ItemSlot.IconImg].PointerExitAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            highlightImg.color = new Color(highlightImg.color.r, highlightImg.color.g, highlightImg.color.b, 0f);
            _inven.highlightedSlot = null;
            GameManager.UI.itemToolTip.gameObject.SetActive(false);
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

        if (Index == GameManager.Inven.TotalSlotCount - 1)
        {
            _inven.AddListenerToItemTypeToggle();
        }
    }

    protected override ItemData GetItem()
    {
        _invenItems = GameManager.Inven.items;
        return _invenItems[Index];
    }
}
