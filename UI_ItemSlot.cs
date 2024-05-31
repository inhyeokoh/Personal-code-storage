using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : UI_Entity
{
    Image _highlightImg;
    UI_Inventory _inven;
    List<ItemData> _invenItems;

    // 현재 슬롯
    Image _iconImg;
    GameObject _amountText;
    public int Index { get; set; }

    // 드롭 시 위치한 슬롯
    int _otherIndex;

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
        _highlightImg = _entities[(int)Enum_UI_ItemSlot.HighlightImg].GetComponent<Image>();
        _amountText = _iconImg.transform.GetChild(0).gameObject;

        _inven = transform.GetComponentInParent<UI_Inventory>();
        _invenItems = GameManager.Inven.items;        

        ItemRender();

        //드래그 시작
        _entities[(int)Enum_UI_ItemSlot.IconImg].BeginDragAction = (PointerEventData data) =>
        {
            if (!CheckItemNull())
            {
                GameManager.UI.GetPopupForward(GameManager.UI.Inventory);
                _inven.dragImg.SetActive(true);
                _inven.dragImg.GetComponent<Image>().sprite = _iconImg.sprite;  // 드래그 이미지를 현재 이미지로
            }
        };

        //드래그 중
        _entities[(int)Enum_UI_ItemSlot.IconImg].DragAction = (PointerEventData data) =>
        {
            if (!CheckItemNull())
            {
                _inven.dragImg.transform.position = data.position;
            }
        };

        //드래그 끝
        _entities[(int)Enum_UI_ItemSlot.IconImg].EndDragAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;
            
            if (CheckSlotDrop(data) && !_inven.CheckUIOutDrop()) // 드래그 드롭한 오브젝트가 아이템 슬롯이어야함
            {
                _otherIndex = data.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<UI_ItemSlot>().Index;
                GameManager.Inven.DragAndDropItems(Index, _otherIndex);
            }
            else if (_inven.CheckUIOutDrop()) // 인벤토리 UI 밖에 드롭할 경우
            {
                if (CheckSlotDrop(data)) // 드래그 드롭한 오브젝트가 장비 슬롯인 경우
                {
                    _otherIndex = data.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<UI_EquipSlot>().Index;
                    GameManager.Inven.InvenToEquipSlot(Index, _otherIndex);
                }
                else
                {
                    if (_invenItems[Index].count == 1)
                    {
                        // 버릴지 되묻는 팝업
                        _inven.dropConfirmPanel.SetActive(true);
                        _inven.dropConfirmPanel.transform.GetChild(0).GetComponent<UI_DropConfirm>().ChangeText(UI_DropConfirm.Enum_DropUIParent.Inven, Index);
                    }
                    else
                    {
                        // 버릴 아이템 이름 + 수량 적는 팝업
                        _inven.dropCountConfirmPanel.SetActive(true);
                        _inven.dropCountConfirmPanel.transform.GetChild(0).GetComponent<UI_DropCountConfirm>().ChangeText(UI_DropCountConfirm.Enum_DropUIParent.Inven, Index);
                    }
                }
            }

            _inven.dragImg.SetActive(false);
        };

        // 커서가 들어오면 아이템 설명 이미지 띄우기 + 하이라이트 효과
        _entities[(int)Enum_UI_ItemSlot.IconImg].PointerEnterAction = (PointerEventData data) =>
        {
            if (!CheckItemNull())
            {
                _inven.descrPanel.SetActive(true);
                _highlightImg.color = new Color(_highlightImg.color.r, _highlightImg.color.g, _highlightImg.color.b, 0.4f);
                ShowItemInfo();
                _inven.RestrictItemDescrPos();
            }
        };

        // 커서가 나갔을때 아이템 설명 내리기 + 하이라이트 효과 끄기
        _entities[(int)Enum_UI_ItemSlot.IconImg].PointerExitAction = (PointerEventData data) =>
        {
            _highlightImg.color = new Color(_highlightImg.color.r, _highlightImg.color.g, _highlightImg.color.b, 0f);
            _inven.descrPanel.SetActive(false);
            _inven.StopRestrictItemDescrPos(data);
        };

        // 아이템 우클릭
        _entities[(int)Enum_UI_ItemSlot.IconImg].ClickAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            if (data.button == PointerEventData.InputButton.Right)
            {
                // 상점 열려 있는 상태면 상점 판매탭 물품으로 이동
                if (GameManager.UI.Shop.activeSelf)
                {
                    GameManager.UI.Shop.GetComponent<UI_Shop>().panel_U_Buttons[1].isOn = true; // 판매탭 활성화
                    GameManager.Inven.InvenToShop(Index);
                }
                else
                {
                    if (_invenItems[Index].itemType == Enum_ItemType.Equipment) // 장비에 우클릭 한 경우
                    {
                        // TODO 장착 불가 경우
                        GameManager.Inven.EquipItem(Index);
                    }
                }
            }
        };
    }

    // 슬롯 번호에 맞게 아이템 그리기
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
            _highlightImg.color = new Color(_highlightImg.color.r, _highlightImg.color.g, _highlightImg.color.b, 0f);
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
        _inven.descrPanel.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = GameManager.Inven.items[Index].name; // 아이템 이름
        _inven.descrPanel.transform.GetChild(1).GetComponent<Image>().sprite = _iconImg.sprite; // 아이콘 이미지

        if (GameManager.Inven.items[Index].itemType == Enum_ItemType.Equipment) // 장비아이템 설명
        {
            StateItemData itemData = ItemParsing.itemDatas[GameManager.Inven.items[Index].id] as StateItemData;
            if (itemData != null)
            {
                int[] stats = {itemData.level, itemData.attack, itemData.defense, itemData.speed, itemData.attackSpeed, itemData.maxHp, itemData.maxMp};
                string descLines = string.Format(GameManager.Inven.items[Index].desc, $"{itemData.level}\n", $"{itemData.attack}\n", $"{itemData.defense}\n", $"{itemData.speed}\n", $"{itemData.attackSpeed}\n", $"{itemData.maxHp}\n", $"{itemData.maxMp}\n");
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

                _inven.descrPanel.transform.GetChild(2).GetComponentInChildren<TMP_Text>().text = desc;
            }
        }
        else
        {
            _inven.descrPanel.transform.GetChild(2).GetComponentInChildren<TMP_Text>().text =
                GameManager.Inven.items[Index].desc; // 아이템 설명
        }
    }
}

