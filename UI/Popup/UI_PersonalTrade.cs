using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PersonalTrade : UI_Entity
{
    public UI_TradeContents othersTradeContents;
    public UI_TradeContents myTradeContents;

    #region 개인거래 UI 드래그
    Vector2 _UIPos;
    Vector2 _dragBeginPos;
    Vector2 _offset;
    #endregion

    public string tradePartnerName;

    enum Enum_UI_PersonalTrade
    {
        Panel,
        Interact,
        Trade,
        TradeContents,
        Tip,
        Close
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_PersonalTrade);
    }

    public override void PopupOnEnable()
    {
        GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockPlayerInput, true);
    }

    public override void PopupOnDisable()
    {
        GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockPlayerInput, false);

        othersTradeContents.ResetContents();
        myTradeContents.ResetContents();
    }

    protected override void Init()
    {
        base.Init();
        #region 초기설정 및 캐싱
        myTradeContents = _entities[(int)Enum_UI_PersonalTrade.TradeContents].transform.GetChild(0).GetComponent<UI_TradeContents>();
        othersTradeContents = _entities[(int)Enum_UI_PersonalTrade.TradeContents].transform.GetChild(0).GetComponent<UI_TradeContents>();
        #endregion

        // 개인 거래창 드래그
        _entities[(int)Enum_UI_PersonalTrade.Interact].BeginDragAction = (PointerEventData data) =>
        {
            _UIPos = transform.position;
            _dragBeginPos = data.position;
        };
        _entities[(int)Enum_UI_PersonalTrade.Interact].DragAction = (PointerEventData data) =>
        {
            _offset = data.position - _dragBeginPos;
            transform.position = _UIPos + _offset;
        };

        _entities[(int)Enum_UI_PersonalTrade.Trade].DragAction = (PointerEventData data) =>
        {
            // TODO tradeItems, gold 패킷 만들어서 서버로 전달
        };

        _entities[(int)Enum_UI_PersonalTrade.Close].ClickAction = (PointerEventData data) =>
        {
            GameManager.UI.ClosePopup(this);
        };
    }
}
