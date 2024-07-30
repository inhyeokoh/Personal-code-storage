using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UI_TradeContents : UI_Entity
{
    public UI_TradeItemSlot highlightedSlot;
    public UI_TradeItemSlot[] tradeItemSlots;
    int _totalSlotCount = 6;

    TMP_Text _nickname;
    GameObject _items;
    int _goldAmount;
    public TMP_Text goldText;

    public const int TRADE_USER_ME = 0;
    public const int TRADE_USER_OTHER = 1;

    [SerializeField]
    int tradeUser;

    enum Enum_UI_TradeContents
    {
        Panel,
        Nickname,
        Items,
        GoldPanel,
        InputGold
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_TradeContents);
    }

    protected override void Init()
    {
        base.Init();
        _nickname = _entities[(int)Enum_UI_TradeContents.Nickname].transform.GetChild(0).GetComponent<TMP_Text>();
        _items = _entities[(int)Enum_UI_TradeContents.Items].gameObject;
        goldText = _entities[(int)Enum_UI_TradeContents.GoldPanel].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();

        if (tradeUser == TRADE_USER_ME)
        {
            _entities[(int)Enum_UI_TradeContents.InputGold].ClickAction = (PointerEventData data) => {
                GameManager.UI.OpenPopup(GameManager.UI.InGameConfirmYN);
                GameManager.UI.InGameConfirmYN.ChangeText(UI_InGameConfirmYN.Enum_ConfirmTypes.InputTradeGoldAmount);
            };
        }

        _DrawNames();
        _CachingSlots();

        GameManager.UI.PersonalTrade.gameObject.SetActive(false);
    }

    void _DrawNames()
    {
        switch (tradeUser)
        {
            case TRADE_USER_ME:
                _nickname.text = GameManager.Data.NickName;
                break;
            case TRADE_USER_OTHER:
                //_nickName.text = personalTrade.tradePartnerName;
                break;
            default:
                break;
        }        
    }

    void _CachingSlots()
    {
        tradeItemSlots = new UI_TradeItemSlot[_totalSlotCount];
        for (int i = 0; i < _totalSlotCount; i++)
        {
            tradeItemSlots[i] = _items.transform.GetChild(i).GetComponent<UI_TradeItemSlot>();
            tradeItemSlots[i].Index = i;
        }
    }

    public void UpdateItemSlotUI(int slotIndex)
    {
        tradeItemSlots[slotIndex].ItemRender();
    }

    public void UpdateGoldUI(int amount)
    {
        _goldAmount += amount;
        goldText.text = _goldAmount.ToString();
        GameManager.Inven.Gold -= amount;
    }

    public void ResetContents()
    {
        switch (tradeUser)
        {
            case TRADE_USER_ME:
                for (int i = 0; i < GameManager.Inven.tradeItems.Length; i++)
                {
                    if (GameManager.Inven.tradeItems[i] != null)
                    {
                        GameManager.Inven.TradeItemToInven(i);
                    }
                }
                GameManager.Inven.Gold += _goldAmount;
                break;
            case TRADE_USER_OTHER:
                break;
            default:
                break;
        }

        _goldAmount = 0;
        goldText.text = _goldAmount.ToString();
    }
}