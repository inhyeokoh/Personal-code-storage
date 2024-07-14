using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_DropConfirm : UI_Entity
{
    TMP_Text _mainText;
    int _slotIndex;
    Enum_DropUIParent dropUIParent;

    enum Enum_UI_DropConfirm
    {
        MainText,
        Accept,
        Cancel
    }
    public enum Enum_DropUIParent
    {
        Inven,
        PlayerInfo
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_DropConfirm);
    }

    protected override void Init()
    {
        base.Init();
        dropUIParent = Enum_DropUIParent.Inven;

        _mainText = _entities[(int)Enum_UI_DropConfirm.MainText].transform.GetChild(0).GetComponent<TMP_Text>();
                
        _entities[(int)Enum_UI_DropConfirm.Accept].ClickAction = (PointerEventData data) => {
            switch (dropUIParent)
            {
                case Enum_DropUIParent.Inven:
                    GameManager.Inven.DropInvenItem(_slotIndex);
                    break;
                case Enum_DropUIParent.PlayerInfo:
                    GameManager.Inven.DropEquipItem(_slotIndex);
                    break;
                default:
                    break;
            }

            GameManager.UI.ClosePopup(GameManager.UI.InGameConfirmYN);
        };

        _entities[(int)Enum_UI_DropConfirm.Cancel].ClickAction = (PointerEventData data) => {
            GameManager.UI.ClosePopup(GameManager.UI.InGameConfirmYN);
        };

        transform.gameObject.SetActive(false);
    }

    public void ChangeText(Enum_DropUIParent UIName , int slotIndex)
    {
        dropUIParent = UIName;

        _slotIndex = slotIndex;
        switch (dropUIParent)
        {
            case Enum_DropUIParent.Inven:
                _mainText.text = $"{GameManager.Inven.items[slotIndex].name} 아이템을 버리시겠습니까?";
                break;
            case Enum_DropUIParent.PlayerInfo:
                _mainText.text = $"{GameManager.Inven.equips[slotIndex].name} 아이템을 버리시겠습니까?";
                break;
            default:
                break;
        }
    }
}
