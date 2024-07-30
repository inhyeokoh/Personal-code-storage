using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_InGameConfirmY : UI_Entity
{
    bool _init;
    bool _useBlocker = true;
    TMP_Text _mainText;
    Enum_ConfirmTypes confirmType;

    enum Enum_UI_InGameConfirmY
    {
        Panel,
        MainText,
        Accept
    }

    public enum Enum_ConfirmTypes
    {
        InvenFull,
        MoreThanHave,
        LessThanZero,
        ShopBasketFull,
        NotEnoughMoney,
    }

    public override void PopupOnEnable()
    {
        if (!_init || !_useBlocker) return;

        GameManager.UI.UseBlocker(true);
    }

    public override void PopupOnDisable()
    {
        GameManager.UI.UseBlocker(false);
        GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockMouseClick, false);
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_InGameConfirmY);
    }

    protected override void Init()
    {
        base.Init();
        confirmType = Enum_ConfirmTypes.NotEnoughMoney;
        _mainText = _entities[(int)Enum_UI_InGameConfirmY.MainText].transform.GetChild(0).GetComponent<TMP_Text>();

        foreach (var _subUI in _subUIs)
        {
            // UI위에 커서가 있을 시 캐릭터 행동 제약
            _subUI.PointerEnterAction = (PointerEventData data) =>
            {
                GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockMouseClick, true);
            };

            _subUI.PointerExitAction = (PointerEventData data) =>
            {
                GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockMouseClick, false);
            };
        }

        _entities[(int)Enum_UI_InGameConfirmY.Accept].ClickAction = (PointerEventData data) => {
            GameManager.UI.ClosePopup(GameManager.UI.InGameConfirmY);
        };

        gameObject.SetActive(false);
        _init = true;
    }

    public void ChangeText(Enum_ConfirmTypes type)
    {
        confirmType = type;

        switch (confirmType)
        {
            case Enum_ConfirmTypes.InvenFull:
                _mainText.text = $"인벤토리가 부족합니다!";
                break;
            case Enum_ConfirmTypes.MoreThanHave:
                _mainText.text = $"가지고 있는 수량보다 큽니다!";
                break;
            case Enum_ConfirmTypes.LessThanZero:
                _mainText.text = $"0이하의 수량은 버릴 수 없습니다!";
                break;
            case Enum_ConfirmTypes.ShopBasketFull:
                _mainText.text = $"장바구니가 가득 찼습니다!";
                break;
            case Enum_ConfirmTypes.NotEnoughMoney:
                _mainText.text = $"돈이 부족합니다!";
                break;
            default:
                break;
        }
    }

    public override void EnterAction()
    {
        base.EnterAction();
        _entities[(int)Enum_UI_InGameConfirmY.Accept].ClickAction?.Invoke(null);
    }
}
