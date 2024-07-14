using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EquipSlot : UI_Entity
{
    UI_PlayerInfo _playerInfoUI;
    Image _highlightImg;

    // 현재 슬롯
    Image _iconImg;
    public int Index { get; set; }

    // 드롭 시 위치한 슬롯
    int _otherIndex;

    enum Enum_UI_EquipSlot
    {
        SlotImg,
        HighlightImg,
        IconImg
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_EquipSlot);
    }

    protected override void Init()
    {
        base.Init();
        _iconImg = _entities[(int)Enum_UI_EquipSlot.IconImg].GetComponent<Image>();
        _highlightImg = _entities[(int)Enum_UI_EquipSlot.HighlightImg].GetComponent<Image>();
        _playerInfoUI = transform.GetComponentInParent<UI_PlayerInfo>();
        
        //드래그 시작
        _entities[(int)Enum_UI_EquipSlot.IconImg].BeginDragAction = (PointerEventData data) =>
        {
            if (!CheckItemNull())
            {
                GameManager.UI.GetPopupForward(GameManager.UI.PlayerInfo);
                _playerInfoUI.dragImg.SetActive(true);
                _playerInfoUI.dragImg.GetComponent<Image>().sprite = _iconImg.sprite;  // 드래그 이미지를 현재 이미지로
            }
        };

        //드래그 중
        _entities[(int)Enum_UI_EquipSlot.IconImg].DragAction = (PointerEventData data) =>
        {
            if (!CheckItemNull())
            {
                _playerInfoUI.dragImg.transform.position = data.position;
            }
        };

        //드래그 끝
        _entities[(int)Enum_UI_EquipSlot.IconImg].EndDragAction = (PointerEventData data) =>
        {
            if (CheckItemNull())
            {
                return;
            }

            // 플레이어 정보 UI 밖에 드롭할 경우
            if (_playerInfoUI.CheckUIOutDrop())
            {
                if (CheckSlotDrop(data))  // 드롭한 위치가 인벤 슬롯
                {
                    _otherIndex = data.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<UI_ItemSlot>().Index;
                    GameManager.Inven.EquipSlotToInven(Index, _otherIndex);
                }
                else
                {
                    // 버릴지 되묻는 팝업
                    GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmYN);
                    GameManager.UI.InGameConfirmYN.ChangeText(UI_InGameConfirmYN.Enum_ConfirmTypes.EquipDrop, Index);
                }
            }

            _playerInfoUI.dragImg.SetActive(false);
        };

        // 커서가 들어오면 아이템 설명 이미지 띄우기 + 하이라이트 효과
        _entities[(int)Enum_UI_EquipSlot.IconImg].PointerEnterAction = (PointerEventData data) =>
        {
            if (!CheckItemNull())
            {
                _playerInfoUI.descrPanel.SetActive(true);
                _highlightImg.color = new Color(_highlightImg.color.r, _highlightImg.color.g, _highlightImg.color.b, 0.4f);
                ShowItemInfo();
                _playerInfoUI.RestrictItemDescrPos();
            }
        };

        // 커서가 나갔을때 아이템 설명 내리기 + 하이라이트 효과 끄기
        _entities[(int)Enum_UI_EquipSlot.IconImg].PointerExitAction = (PointerEventData data) =>
        {
            if (!CheckItemNull())
            {
                _highlightImg.color = new Color(_highlightImg.color.r, _highlightImg.color.g, _highlightImg.color.b, 0f);
                _playerInfoUI.descrPanel.SetActive(false);
                _playerInfoUI.StopRestrictItemDescrPos(data);
            }
        };

        // 우클릭으로 아이템 장착
        _entities[(int)Enum_UI_EquipSlot.IconImg].ClickAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            if (data.button == PointerEventData.InputButton.Right && GameManager.Inven.equips[Index].itemType == Enum_ItemType.Equipment) // 장비에 우클릭 한 경우
            {
                // TODO 장착 불가 경우

                GameManager.Inven.UnEquipItem(Index);
            }
        };

        if (Index == GameManager.Inven.EquipSlotCount - 1)
        {
            _playerInfoUI.gameObject.SetActive(false);
        }
    }

    // 슬롯 번호에 맞게 아이템 그리기
    public void ItemRender()
    {
        if (GameManager.Inven.equips[Index] != null)
        {
            _iconImg.color = new Color32(255, 255, 255, 255);
            _iconImg.sprite = GameManager.Inven.equips[Index].icon;
        }
        else
        {
            _iconImg.sprite = null;
            _iconImg.color = new Color32(12, 15, 29, 0);
            _highlightImg.color = new Color(_highlightImg.color.r, _highlightImg.color.g, _highlightImg.color.b, 0f);
            _playerInfoUI.descrPanel.SetActive(false);
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
        return GameManager.Inven.equips[Index] == null;
    }

    // 드롭 시 슬롯에 벗어나지 않았는지 확인
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
        _playerInfoUI.descrPanel.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = GameManager.Inven.equips[Index].name; // 아이템 이름
        _playerInfoUI.descrPanel.transform.GetChild(1).GetComponent<Image>().sprite = _iconImg.sprite; // 아이콘 이미지

        StateItemData itemData = GameManager.Data.itemDatas[GameManager.Inven.equips[Index].id] as StateItemData;
        int[] stats = {itemData.level, itemData.attack, itemData.defense, itemData.speed, itemData.attackSpeed, itemData.maxHp, itemData.maxMp};
        string descLines = string.Format(GameManager.Inven.equips[Index].desc, $"{itemData.level}\n", $"{itemData.attack}\n", $"{itemData.defense}\n", $"{itemData.speed}\n", $"{itemData.attackSpeed}\n", $"{itemData.maxHp}\n", $"{itemData.maxMp}\n");
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

        _playerInfoUI.descrPanel.transform.GetChild(2).GetComponentInChildren<TMP_Text>().text = desc;
    }
}
