using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_InGameConfirmYN : UI_Entity
{
    bool _useBlocker = true;
    Enum_ConfirmTypes confirmType;
    TMP_Text _mainText;
    TMP_InputField _inputField;
    int _slotIndex;

    public override void PopupOnEnable()
    {
        if (!_useBlocker) return;

        GameManager.UI.UseBlocker(true);
    }

    public override void PopupOnDisable()
    {
        GameManager.UI.UseBlocker(false);
        GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockMouseClick, false);
    }

    enum Enum_UI_InGameConfirmYN
    {
        Panel,
        MainText,
        InputField,
        Accept,
        Cancel
    }

    public enum Enum_ConfirmTypes
    {
        InvenSingleDrop,
        InvenPluralDrop,
        EquipDrop,
        PutInShopBasket,
        InputTradeGoldAmount
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_InGameConfirmYN);
    }

    protected override void Init()
    {
        base.Init();

        inputFields = new List<TMP_InputField>();
        TMP_InputField inputField = _entities[(int)Enum_UI_InGameConfirmYN.InputField].GetComponent<TMP_InputField>();
        inputFields.Add(inputField);

        confirmType = Enum_ConfirmTypes.InvenSingleDrop;
        _mainText = _entities[(int)Enum_UI_InGameConfirmYN.MainText].transform.GetChild(0).GetComponent<TMP_Text>();
        _inputField = _entities[(int)Enum_UI_InGameConfirmYN.InputField].GetComponent<TMP_InputField>();

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

        _entities[(int)Enum_UI_InGameConfirmYN.Accept].ClickAction = (PointerEventData data) => {
            string input;
            int inputCount = 0;
            if (_inputField.gameObject.activeSelf)
            {
                input = _inputField.text;
                inputCount = Convert.ToInt32(input);
            }

            switch (confirmType)
            {
                case Enum_ConfirmTypes.InvenSingleDrop:
                    GameManager.Inven.DropInvenItem(_slotIndex);
                    break;
                case Enum_ConfirmTypes.InvenPluralDrop:
                    if (inputCount > GameManager.Inven.items[_slotIndex].count)
                    {
                        GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmY);
                        GameManager.UI.InGameConfirmY.ChangeText(UI_InGameConfirmY.Enum_ConfirmTypes.MoreThanHave);
                        return;
                    }
                    else if (inputCount <= 0)
                    {
                        GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmY);
                        GameManager.UI.InGameConfirmY.ChangeText(UI_InGameConfirmY.Enum_ConfirmTypes.LessThanZero);
                        return;
                    }
                    GameManager.Inven.DropInvenItem(_slotIndex, inputCount);
                    break;
                case Enum_ConfirmTypes.EquipDrop:
                    GameManager.Inven.DropEquipItem(_slotIndex);
                    break;
                case Enum_ConfirmTypes.PutInShopBasket:
                    UI_ShopPurchase shopPurchase = GameManager.UI.Shop.GetComponentInChildren<UI_ShopPurchase>();
                    if (shopPurchase.AfterPurchaseGold - shopPurchase.shopItemList[_slotIndex].itemPrice * inputCount < 0)
                    {
                        GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmY);
                        GameManager.UI.InGameConfirmY.ChangeText(UI_InGameConfirmY.Enum_ConfirmTypes.NotEnoughMoney);
                        return;
                    }

                    if (inputCount <= 0)
                    {
                        GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmY);
                        GameManager.UI.InGameConfirmY.ChangeText(UI_InGameConfirmY.Enum_ConfirmTypes.LessThanZero);
                        return;
                    }
                    shopPurchase.AddItemInShopBasket(_slotIndex, inputCount);
                    shopPurchase.UpdateGoldPanel();
                    break;
                case Enum_ConfirmTypes.InputTradeGoldAmount:
                    GameManager.UI.PersonalTrade.myTradeContents.UpdateGoldUI(inputCount);
                    break;
                default:
                    break;
            }

            GameManager.UI.ClosePopup(GameManager.UI.InGameConfirmYN);
        };

        _entities[(int)Enum_UI_InGameConfirmYN.Cancel].ClickAction = (PointerEventData data) => {
            GameManager.UI.ClosePopup(GameManager.UI.InGameConfirmYN);
        };

        gameObject.SetActive(false);
    }

    public void ChangeText(Enum_ConfirmTypes type, int slotIndex = -1)
    {
        confirmType = type;

        _slotIndex = slotIndex;
        switch (confirmType)
        {
            case Enum_ConfirmTypes.InvenSingleDrop:
                _inputField.gameObject.SetActive(false);
                _mainText.text = $"{GameManager.Inven.items[slotIndex].name} 아이템을 버리시겠습니까?";
                break;
            case Enum_ConfirmTypes.InvenPluralDrop:
                _inputField.gameObject.SetActive(true);
                _inputField.GetComponent<TMP_InputField>().text = GameManager.Inven.items[_slotIndex].count.ToString(); // 최대 수량 자동 입력
                _mainText.text = $"{GameManager.Inven.items[slotIndex].name} 아이템을 몇개나 버리시겠습니까?";
                break;
            case Enum_ConfirmTypes.EquipDrop:
                _inputField.gameObject.SetActive(false);
                _mainText.text = $"{GameManager.Inven.equipItems[slotIndex].name} 아이템을 버리시겠습니까?";
                break;
            case Enum_ConfirmTypes.PutInShopBasket:
                _inputField.gameObject.SetActive(true);
                UI_ShopPurchase shopPurchase = GameManager.UI.Shop.GetComponentInChildren<UI_ShopPurchase>();
                _mainText.text = $"{shopPurchase.shopItems[slotIndex].name} 아이템을 몇개나 구매하시겠습니까?";
                break;
            case Enum_ConfirmTypes.InputTradeGoldAmount:
                _inputField.gameObject.SetActive(true);
                _mainText.text = $"얼마를 제시하시겠습니까?";
                break;
            default:
                break;
        }
    }

    public override void EnterAction()
    {
        base.EnterAction();
        _entities[(int)Enum_UI_InGameConfirmYN.Accept].ClickAction?.Invoke(null);
    }
}
