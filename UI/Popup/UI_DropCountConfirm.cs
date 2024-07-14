using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_DropCountConfirm : UI_Entity
{
    TMP_Text _mainText;
    int _slotIndex;

    enum Enum_UI_DropCountConfirm
    {
        MainText,
        InputField,
        Accept,
        Cancel
    }


    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_DropCountConfirm);
    }

    protected override void Init()
    {
        base.Init();

        _mainText = _entities[(int)Enum_UI_DropCountConfirm.MainText].transform.GetChild(0).GetComponent<TMP_Text>();

        // 입력한 수량에 맞게 버리기
        _entities[(int)Enum_UI_DropCountConfirm.Accept].ClickAction = (PointerEventData data) => {
            string input = _entities[(int)Enum_UI_DropCountConfirm.InputField].GetComponent<TMP_InputField>().text;
            int dropCount = Convert.ToInt32(input);
            GameManager.Inven.DropInvenItem(_slotIndex, dropCount);
            transform.parent.gameObject.SetActive(false);
        };

        _entities[(int)Enum_UI_DropCountConfirm.Cancel].ClickAction = (PointerEventData data) => {
            transform.parent.gameObject.SetActive(false);
        };

        transform.parent.gameObject.SetActive(false);
    }

    public void ChangeText(int slotIndex)
    {
        _slotIndex = slotIndex;
        _mainText.text = $"{GameManager.Inven.items[slotIndex].name} 아이템을 몇개나 버리시겠습니까?";
    }
}

