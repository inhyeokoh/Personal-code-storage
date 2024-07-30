using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EquipItemSlot : UI_ItemSlot
{
    List<ItemData> _equipItems;
    UI_PlayerInfo _playerInfo;

    protected override void Init()
    {
        base.Init();
        _playerInfo = transform.GetComponentInParent<UI_PlayerInfo>();

        //드래그 시작
        _entities[(int)Enum_UI_ItemSlot.IconImg].BeginDragAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            GameManager.UI.GetPopupForward(GameManager.UI.PlayerInfo);
            _playerInfo.dragImg.gameObject.SetActive(true);
            _playerInfo.dragImg.GetComponent<Image>().sprite = _iconImg.sprite;

        };

        //드래그 중
        _entities[(int)Enum_UI_ItemSlot.IconImg].DragAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            _playerInfo.dragImg.transform.position = data.position;

        };

        //드래그 끝
        _entities[(int)Enum_UI_ItemSlot.IconImg].EndDragAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            // 플레이어 정보 UI 밖에 드롭할 경우
            if (_playerInfo.CheckUIOutDrop())
            {
                if (CheckSlotDrop(data))  // 드롭한 위치가 인벤 슬롯
                {
                    _otherIndex = data.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<UI_InventoryItemSlot>().Index;
                    GameManager.Inven.EquipSlotToInven(Index, _otherIndex);
                }
                else
                {
                    // 버릴지 되묻는 팝업
                    GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmYN);
                    GameManager.UI.InGameConfirmYN.ChangeText(UI_InGameConfirmYN.Enum_ConfirmTypes.EquipDrop, Index);
                }
            }

            _playerInfo.dragImg.gameObject.SetActive(false);
        };

        // 커서가 들어오면 아이템 설명 이미지 띄우기 + 하이라이트 효과
        _entities[(int)Enum_UI_ItemSlot.IconImg].PointerEnterAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            highlightImg.color = new Color(highlightImg.color.r, highlightImg.color.g, highlightImg.color.b, 0.4f);
            _playerInfo.highlightedSlot = this;
            GameManager.UI.itemToolTip.ShowItemInfo(item);
        };

        // 커서가 나갔을때 아이템 설명 내리기 + 하이라이트 효과 끄기
        _entities[(int)Enum_UI_ItemSlot.IconImg].PointerExitAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            highlightImg.color = new Color(highlightImg.color.r, highlightImg.color.g, highlightImg.color.b, 0f);
            _playerInfo.highlightedSlot = null;
            GameManager.UI.itemToolTip.gameObject.SetActive(false);
        };

        // 우클릭으로 아이템 장착해제
        _entities[(int)Enum_UI_ItemSlot.IconImg].ClickAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            if (data.button == PointerEventData.InputButton.Right && GameManager.Inven.equipItems[Index].itemType == Enum_ItemType.Equipment) // 장비에 우클릭 한 경우
            {
                GameManager.Inven.UnEquipItem(Index);
            }
        };
    }

    protected override ItemData GetItem()
    {
        _equipItems = GameManager.Inven.equipItems;
        return _equipItems[Index];
    }
}
