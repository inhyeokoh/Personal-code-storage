using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UI_Shop : UI_Entity
{
    public GameObject descrPanel;
    public UI_ShopPurchase shopPurchase;
    public UI_ShopSell shopSell;
    public UI_ShopRepurchase shopRepurchase;

    public Toggle[] panel_U_Buttons;

    public Rect panelRect;
    Vector2 _descrUISize;

    // 드래그 Field
    private Vector2 _shopUIPos;
    private Vector2 _dragBeginPos;
    private Vector2 _offset;

    enum Enum_UI_Shop
    {
        Interact,
        Panel,
        TradeOptions,
        DescrPanel,
        NotifyFull,
        PurchaseCountConfirm
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_Shop);
    }

    public override void PopupOnEnable()
    {
        // 인벤토리도 같이 열려야함
        GameManager.UI.Inventory.gameObject.SetActive(true);
        StartCoroutine(DeactivateCloseButtonWithDelay());
    }

    public override void PopupOnDisable()
    {
        GameManager.UI.Inventory.closeBtn.SetActive(true);
        shopSell.ReturnSellListToInven();
        shopRepurchase.EmptyTempForSold();
        GameManager.UI.Inventory.gameObject.SetActive(false);
    }

    protected override void Init()
    {
        base.Init();
        descrPanel = _entities[(int)Enum_UI_Shop.DescrPanel].gameObject;
        panel_U_Buttons = _entities[(int)Enum_UI_Shop.TradeOptions].GetComponentsInChildren<Toggle>();
        shopPurchase = _entities[(int)Enum_UI_Shop.Panel].GetComponentInChildren<UI_ShopPurchase>();
        shopSell = _entities[(int)Enum_UI_Shop.Panel].GetComponentInChildren<UI_ShopSell>();
        shopRepurchase = _entities[(int)Enum_UI_Shop.Panel].GetComponentInChildren<UI_ShopRepurchase>();
        panelRect = _entities[(int)Enum_UI_Shop.Panel].GetComponent<RectTransform>().rect;
        _descrUISize = _GetUISize(descrPanel);

        _SetTradeOptionToggles();

        foreach (var _subUI in _subUIs)
        {
            _subUI.ClickAction = (PointerEventData data) =>
            {
                GameManager.UI.GetPopupForward(GameManager.UI.Shop);
            };

            // UI위에 커서 있을 시 캐릭터 행동 제약
/*            _subUI.PointerEnterAction = (PointerEventData data) =>
            {
            };*/

/*            _subUI.PointerExitAction = (PointerEventData data) =>
            {
                GameManager.UI.PointerOnUI(false);
            };*/
        }

        // 상점 창 드래그 시작
        _entities[(int)Enum_UI_Shop.Interact].BeginDragAction = (PointerEventData data) =>
        {
            _shopUIPos = transform.position;
            _dragBeginPos = data.position;
        };

        // 상점 창 드래그
        _entities[(int)Enum_UI_Shop.Interact].DragAction = (PointerEventData data) =>
        {
            _offset = data.position - _dragBeginPos;
            transform.position = _shopUIPos + _offset;
        };
    }

    public void RestrictItemDescrPos()
    {
        Vector2 option = new Vector2(300f, -165f);
        StartCoroutine(RestrictUIPos(descrPanel, _descrUISize, option));
    }

    public void StopRestrictItemDescrPos(PointerEventData data)
    {
        StopCoroutine(RestrictUIPos(descrPanel, _descrUISize));
    }

    // UI 사각형 좌표의 좌측하단과 우측상단 좌표를 전역 좌표로 바꿔서 사이즈 계산
    Vector2 _GetUISize(GameObject UI)
    {
        Vector2 leftBottom = UI.transform.TransformPoint(UI.GetComponent<RectTransform>().rect.min);
        Vector2 rightTop = UI.transform.TransformPoint(UI.GetComponent<RectTransform>().rect.max);
        Vector2 UISize = rightTop - leftBottom;
        return UISize;
    }

    // UI가 화면 밖으로 넘어가지 않도록 위치 제한
    IEnumerator RestrictUIPos(GameObject UI, Vector2 UISize, Vector2? option = null)
    {
        while (true)
        {
            Vector3 mousePos = Input.mousePosition;
            float x = Math.Clamp(mousePos.x + option.Value.x, UISize.x / 2, Screen.width - (UISize.x / 2));
            float y = Math.Clamp(mousePos.y + option.Value.y, UISize.y / 2, Screen.height - (UISize.y / 2));
            UI.transform.position = new Vector2(x, y);
            yield return null;
        }
    }

    IEnumerator DeactivateCloseButtonWithDelay()
    {
        yield return new WaitUntil(() => GameManager.UI.Inventory.gameObject.activeSelf);
        GameManager.UI.Inventory.closeBtn.SetActive(false);
    }

    void _SetTradeOptionToggles()
    {
        for (int i = 0; i < panel_U_Buttons.Length; i++)
        {
            int index = i;
            panel_U_Buttons[i].onValueChanged.AddListener((value) => _ToggleValueChanged(index));
        }
    }

    // panel_U_Buttons 선택에 따라서 해당되는 내용을 활성화
    void _ToggleValueChanged(int toggleIndex)
    {
        bool isToggleOn = panel_U_Buttons[toggleIndex].isOn;
        GameObject childObject = _entities[(int)Enum_UI_Shop.Panel].transform.GetChild(toggleIndex).gameObject;
        childObject.SetActive(isToggleOn);
    }

    // 상점 품목 인벤토리로 되돌리기
    public void ShopToInven(UI_ShopSlot.Enum_ShopSlotTypes slotType, int index)
    {
        int emptyIndex = GameManager.Inven.EmptySlot;
        switch (slotType)
        {
            case UI_ShopSlot.Enum_ShopSlotTypes.Sell:
                UI_ShopSell shopSell = GetComponentInChildren<UI_ShopSell>();
                GameManager.Inven.items[emptyIndex] = shopSell.shopItems[index];
                shopSell.shopItems[index] = null;

                GameManager.Inven.inven.UpdateInvenSlot(emptyIndex);
                shopSell.UpdateGoldPanel();
                shopSell.transform.GetChild(0).GetChild(index).GetComponent<UI_ShopSlot>().ItemRender();
                break;
            case UI_ShopSlot.Enum_ShopSlotTypes.Repurchase:
                UI_ShopRepurchase shopRepurchase = GetComponentInChildren<UI_ShopRepurchase>();
                GameManager.Inven.items[emptyIndex] = shopRepurchase.tempSoldItems[index];
                GameManager.Inven.Gold -= shopRepurchase.tempSoldItems[index].sellingprice; // 구매 가격 아니고 판매 가격으로 재구매임. 
                shopRepurchase.tempSoldItems[index] = null;

                GameManager.UI.Inventory.UpdateInvenSlot(emptyIndex);
                shopRepurchase.transform.GetChild(0).GetChild(index).GetComponent<UI_ShopSlot>().ItemRender();
                break;
            default:
                break;
        }
    }
}
