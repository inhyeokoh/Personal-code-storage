using UnityEngine;
using UnityEngine.EventSystems;

public class UI_TradeItemSlot : UI_ItemSlot
{
    ItemData[] _tradeItems;
    UI_TradeContents _tradeContents;

    protected override void Init()
    {
        base.Init();
        _tradeContents = transform.GetComponentInParent<UI_TradeContents>();

        // 커서가 들어오면 아이템 설명 이미지 띄우기 + 하이라이트 효과
        _entities[(int)Enum_UI_ItemSlot.IconImg].PointerEnterAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            highlightImg.color = new Color(highlightImg.color.r, highlightImg.color.g, highlightImg.color.b, 0.4f);
            _tradeContents.highlightedSlot = this;
            GameManager.UI.itemToolTip.ShowItemInfo(item);
        };

        // 커서가 나갔을때 아이템 설명 내리기 + 하이라이트 효과 끄기
        _entities[(int)Enum_UI_ItemSlot.IconImg].PointerExitAction = (PointerEventData data) =>
        {
            if (CheckItemNull()) return;

            highlightImg.color = new Color(highlightImg.color.r, highlightImg.color.g, highlightImg.color.b, 0f);
            _tradeContents.highlightedSlot = null;
            GameManager.UI.itemToolTip.gameObject.SetActive(false);
        };
    }

    protected override ItemData GetItem()
    {
        _tradeItems = GameManager.Inven.tradeItems;
        return _tradeItems[Index];
    }
}