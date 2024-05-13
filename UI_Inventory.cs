using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Inventory : UI_Entity
{
    GameObject _content;
    public GameObject dragImg;
    public GameObject descrPanel;
    public GameObject dropConfirmPanel;
    public GameObject dropCountConfirmPanel;

    public Rect panelRect;
    Vector2 _descrUISize;

    TMP_Text[] upTogNames;
    Toggle[] upToggles;

    List<ItemData> _items;
    int _totalSlotCount;

    // 드래그 Field
    private Vector2 _invenPos;
    private Vector2 _dragBeginPos;
    private Vector2 _offset;

    enum Enum_UI_Inventory
    {
        Interact,
        Panel,
        Panel_U,
        Panel_D,
        Sort,
        Expansion,
        ScrollView,
        TempAdd,
        Close,
        DragImg,
        DescrPanel,
        DropConfirm,
        DropCountConfirm
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_Inventory);
    }

    private void OnDisable()
    {
        GameManager.UI.PointerOnUI(false);
    }

    protected override void Init()
    {
        base.Init();
        _content = _entities[(int)Enum_UI_Inventory.ScrollView].transform.GetChild(0).GetChild(0).gameObject; // Content 담기는 오브젝트
        upTogNames = _entities[(int)Enum_UI_Inventory.Panel_U].GetComponentsInChildren<TMP_Text>();
        upToggles = _entities[(int)Enum_UI_Inventory.Panel_U].GetComponentsInChildren<Toggle>();
        panelRect = _entities[(int)Enum_UI_Inventory.Panel].GetComponent<RectTransform>().rect;
        dragImg = _entities[(int)Enum_UI_Inventory.DragImg].gameObject;
        descrPanel = _entities[(int)Enum_UI_Inventory.DescrPanel].gameObject;
        dropConfirmPanel = _entities[(int)Enum_UI_Inventory.DropConfirm].gameObject;
        dropCountConfirmPanel = _entities[(int)Enum_UI_Inventory.DropCountConfirm].gameObject;
        _descrUISize = _GetUISize(descrPanel);

        _items = GameManager.Inven.items;
        _totalSlotCount = GameManager.Inven.totalSlotCount;

        _SetPanel_U();
        _DrawSlots();

        foreach (var _subUI in _subUIs)
        {
            _subUI.ClickAction = (PointerEventData data) =>
            {
                GameManager.UI.GetPopupForward(GameManager.UI.Inventory);
            };

            // UI위에 커서 있을 시 캐릭터 행동 제약
            _subUI.PointerEnterAction = (PointerEventData data) =>
            {
                GameManager.UI.PointerOnUI(true);
            };

            _subUI.PointerExitAction = (PointerEventData data) =>
            {
                GameManager.UI.PointerOnUI(false);
            };
        }

        // 인벤토리 창 드래그 시작
        _entities[(int)Enum_UI_Inventory.Interact].BeginDragAction = (PointerEventData data) =>
        {
            _invenPos = transform.position;
            _dragBeginPos = data.position;
        };

        // 인벤토리 창 드래그
        _entities[(int)Enum_UI_Inventory.Interact].DragAction = (PointerEventData data) =>
        {
            _offset = data.position - _dragBeginPos;
            transform.position = _invenPos + _offset;
        };

        // 인벤토리 정렬
        _entities[(int)Enum_UI_Inventory.Sort].ClickAction = (PointerEventData data) =>
        {
            GameManager.Inven.SortItems();
        };

        // 인벤토리 확장
        _entities[(int)Enum_UI_Inventory.Expansion].ClickAction = (PointerEventData data) =>
        {
            _ExpandSlot();
        };

        // 아이템 획득 - 임시
        _entities[(int)Enum_UI_Inventory.TempAdd].ClickAction = (PointerEventData data) =>
        {
            _PressGetItem();
        };

        // 인벤토리 닫기
        _entities[(int)Enum_UI_Inventory.Close].ClickAction = (PointerEventData data) =>
        {
            GameManager.UI.ClosePopup(GameManager.UI.Inventory);
        };

        gameObject.SetActive(false);
    }

    // 인벤토리 내 초기 슬롯 생성
    void _DrawSlots()
    {
        for (int i = 0; i < _totalSlotCount; i++)
        {
            GameObject _itemSlot = GameManager.Resources.Instantiate("Prefabs/UI/Scene/ItemSlot", _content.transform);
            _itemSlot.name = "ItemSlot_" + i;
            _itemSlot.GetComponent<UI_ItemSlot>().index = i;
        }
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

    void _SetPanel_U()
    {
        upTogNames[0].text = "All";
        upTogNames[1].text = "Equipment";
        upTogNames[2].text = "Consumption";
        upTogNames[3].text = "Material";
        upTogNames[4].text = "Etc";

        upToggles[0].onValueChanged.AddListener((value) => _ToggleValueChanged(value, "All")); // 전체보기
        for (int i = 1; i < upToggles.Length; i++) // 전체보기를 제외한 분류
        {
            string typeName = upTogNames[i].text;
            upToggles[i].onValueChanged.AddListener((value) => _ToggleValueChanged(value, typeName));
        }
    }

    void _ToggleValueChanged(bool value, string typeName)
    {
        if (value)
        {
            if (typeName == "All")
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    if (_items[i] == null)
                    {
                        continue;
                    }
                    UI_ItemSlot slot = _content.transform.GetChild(i).GetComponent<UI_ItemSlot>();
                    slot.RenderBright();
                }
            }
            else
            {
                _RenderByType(typeName);
            }
        }
    }

    // 선택한 타입에 해당 아이템은 색 밝게, 나머지는 약간 어둡게
    void _RenderByType(string typeName)
    {
        // 문자열을 해당 열거형으로 변환
        Enum_ItemType targetType;
        if (!Enum.TryParse(typeName, out targetType))
        {
            return; //못 바꾸면 return
        }

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] == null)
            {
                continue;
            }

            UI_ItemSlot slot = _content.transform.GetChild(i).GetComponent<UI_ItemSlot>();
            if (_items[i].itemType != targetType) // 다른 타입은 어둡게 그리기
            {
                slot.RenderDark();
            }
            else
            {
                slot.RenderBright();
            }
        }
    }

    // 아이템 배열 정보에 맞게 UI 갱신 시키는 메서드
    public void UpdateInvenUI(int slotIndex)
    {
        UI_ItemSlot slot = _content.transform.GetChild(slotIndex).GetComponent<UI_ItemSlot>();
        slot.ItemRender();
    }

    // 인벤 확장
    void _ExpandSlot(int newSlot = 6)
    {
        for (int i = _totalSlotCount; i < _totalSlotCount + newSlot; i++)
        {
            GameObject _itemSlot = GameManager.Resources.Instantiate("Prefabs/UI/Scene/ItemSlot", _content.transform);
            _itemSlot.name = "ItemSlot_" + i;
            _itemSlot.GetComponent<UI_ItemSlot>().index = i;
        }
        GameManager.Inven.totalSlotCount += newSlot;
        _totalSlotCount = GameManager.Inven.totalSlotCount;
        GameManager.Inven.ExtendItemList();
    }

    void _PressGetItem()
    {
        var item = ItemParsing.StateItemDataReader(500);
        item.count = 70;

        GameManager.Inven.GetItem(item);
        // TODO 장비아이템은 고유번호
    }

    public bool CheckUIOutDrop()
    {
        if (dragImg.transform.localPosition.x < panelRect.xMin || dragImg.transform.localPosition.y < panelRect.yMin ||
            dragImg.transform.localPosition.x > panelRect.xMax || dragImg.transform.localPosition.y > panelRect.yMax)
        {
            return true;
        }

        return false;
    }
}