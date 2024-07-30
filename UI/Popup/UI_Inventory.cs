using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Inventory : UI_Entity
{
    List<ItemData> _items;
    List<UI_InventoryItemSlot> _cachedItemSlots;

    Toggle[] itemTypeToggles;
    int itemTypesCount;

    GameObject _content;  // 아이템 슬롯들 부모 오브젝트
    public GameObject closeBtn;
    TMP_Text goldText;
    public Rect panelRect;

    #region 아이템 드래그 이미지, 설명 패널
    public Image dragImg;
    public UI_InventoryItemSlot highlightedSlot;
    #endregion

    #region 인벤토리 UI 드래그
    Vector2 _UIPos;
    Vector2 _dragBeginPos;
    Vector2 _offset;
    #endregion

    enum Enum_UI_Inventory
    {
        Interact,
        Panel,
        ItemTypePanel,
        Sort,
        Expansion,
        ScrollView,
        TempAdd,
        Gold,
        Close,
        DragImg
    }

    enum Enum_FilteringTypes
    {
        전체,
        장비,
        소비,
        기타
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_Inventory);
    }

    public override void PopupOnDisable()
    {
        GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockMouseClick, false); // 포인터가 UI위에 있던 채로 UI가 닫히면 걸었던 행동 제어가 안 꺼지므로 OnDisable에서 꺼줘야함

        if (highlightedSlot != null)
        {
            Color highlighted = highlightedSlot.highlightImg.color;
            highlighted.a = 0f;
            highlightedSlot.highlightImg.color = highlighted;
        }
        GameManager.UI.itemToolTip.gameObject.SetActive(false);
    }

    protected override void Init()
    {
        base.Init();
        #region 초기설정 및 캐싱
        _items = GameManager.Inven.items;
        _content = _entities[(int)Enum_UI_Inventory.ScrollView].transform.GetChild(0).GetChild(0).gameObject;
        panelRect = _entities[(int)Enum_UI_Inventory.Panel].GetComponent<RectTransform>().rect;

        dragImg = _entities[(int)Enum_UI_Inventory.DragImg].GetComponent<Image>();
        closeBtn = _entities[(int)Enum_UI_Inventory.Close].gameObject;
        itemTypesCount = Enum.GetValues(typeof(Enum_FilteringTypes)).Length;

        _DrawSlots();
        _SetItemTypeToggle();
        #endregion

        #region 골드
        goldText = _entities[(int)Enum_UI_Inventory.Gold].transform.GetChild(0).GetComponent<TMP_Text>();
        UpdateGoldPanel(GameManager.Inven.Gold);
        #endregion

        foreach (var _subUI in _subUIs)
        {
            _subUI.ClickAction = (PointerEventData data) =>
            {
                GameManager.UI.GetPopupForward(GameManager.UI.Inventory);
            };

            // UI위에 커서 있을 시 캐릭터 행동 제약
            _subUI.PointerEnterAction = (PointerEventData data) =>
            {
                GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockMouseClick, true);
            };

            _subUI.PointerExitAction = (PointerEventData data) =>
            {
                GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockMouseClick, false);
            };
        }

        // 인벤토리 창 드래그
        _entities[(int)Enum_UI_Inventory.Interact].BeginDragAction = (PointerEventData data) =>
        {
            _UIPos = transform.position;
            _dragBeginPos = data.position;
        };
        _entities[(int)Enum_UI_Inventory.Interact].DragAction = (PointerEventData data) =>
        {
            _offset = data.position - _dragBeginPos;
            transform.position = _UIPos + _offset;
        };

        // 인벤토리 정렬
        _entities[(int)Enum_UI_Inventory.Sort].ClickAction = (PointerEventData data) =>
        {
            GameManager.Inven.SortItems();
        };

        // 인벤토리 확장 -> 추후 확장 아이템으로 변경
        _entities[(int)Enum_UI_Inventory.Expansion].ClickAction = (PointerEventData data) =>
        {
            _ExpandSlot();
        };

        // 테스트 용도 아이템 획득
        _entities[(int)Enum_UI_Inventory.TempAdd].ClickAction = (PointerEventData data) =>
        {
            _PressGetItem();
        };

        _entities[(int)Enum_UI_Inventory.Close].ClickAction = (PointerEventData data) =>
        {
            GameManager.UI.ClosePopup(GameManager.UI.Inventory);
        };
                
        gameObject.SetActive(false);
    }

    // 인벤토리 초기 슬롯 생성
    void _DrawSlots()
    {
        _cachedItemSlots = new List<UI_InventoryItemSlot>(GameManager.Inven.TotalSlotCount);
        for (int i = 0; i < GameManager.Inven.TotalSlotCount; i++)
        {
            _cachedItemSlots.Add(GameManager.Resources.Instantiate("Prefabs/UI/Scene/ItemSlot", _content.transform).GetOrAddComponent<UI_InventoryItemSlot>());
            _cachedItemSlots[i].Index = i;
        }
    }

    void _SetItemTypeToggle()
    {
        itemTypeToggles = new Toggle[itemTypesCount];
        for (int i = 0; i < itemTypesCount; i++)
        {
            itemTypeToggles[i] = GameManager.Resources.Instantiate("Prefabs/UI/Scene/InvenItemTypeToggle", _entities[(int)Enum_UI_Inventory.ItemTypePanel].transform).GetComponent<Toggle>();

            TMP_Text itemTypeToggleName = itemTypeToggles[i].GetComponentInChildren<TMP_Text>();
            itemTypeToggleName.text = Enum.GetName(typeof(Enum_FilteringTypes), i);

            itemTypeToggles[i].isOn = false;
            itemTypeToggles[i].group = _entities[(int)Enum_UI_Inventory.ItemTypePanel].transform.GetComponent<ToggleGroup>();
        }
    }

    public void AddListenerToItemTypeToggle()
    {
        itemTypeToggles[0].onValueChanged.AddListener((value) => _ToggleValueChanged(value, (int)Enum_FilteringTypes.전체));
        for (int i = 1; i < itemTypesCount; i++) // 전체보기를 제외한 분류
        {
            int index = i;
            itemTypeToggles[index].onValueChanged.AddListener((value) => _ToggleValueChanged(value, index));
        }
    }

    void _ToggleValueChanged(bool value, int typeNum)
    {
        if (value)
        {
            if (typeNum == (int)Enum_FilteringTypes.전체)
            {
                _RenderAllItemsBright();
            }
            else
            {
                _RenderByType(typeNum);
            }
        }
    }

    // 전체 아이템을 밝게 렌더링
    void _RenderAllItemsBright()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] == null) continue;

            _cachedItemSlots[i].RenderBright();
        }
    }

    // 선택한 타입에 해당 아이템은 색 밝게, 나머지는 약간 어둡게
    void _RenderByType(int typeNum)
    {
        Enum_ItemType targetType = (Enum_ItemType)(typeNum - 1);

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] == null) continue;

            if (_items[i].itemType != targetType)
            {
                _cachedItemSlots[i].RenderDark();
            }
            else
            {
                _cachedItemSlots[i].RenderBright();
            }
        }
    }

    /// <summary>
    /// 아이템 슬롯 UI 갱신
    /// </summary>
    public void UpdateInvenSlot(int slotIndex)
    {
        _cachedItemSlots[slotIndex].ItemRender();
    }

    /// <summary>
    /// 인벤토리 확장
    /// </summary>
    void _ExpandSlot(int newSlotCount = 6)
    {
        for (int i = GameManager.Inven.TotalSlotCount; i < GameManager.Inven.TotalSlotCount + newSlotCount; i++)
        {
            _cachedItemSlots.Add(GameManager.Resources.Instantiate("Prefabs/UI/Scene/ItemSlot", _content.transform).GetComponent<UI_InventoryItemSlot>());
            _cachedItemSlots[i].Index = i;
        }
        GameManager.Inven.ExtendItemListWithNull(newSlotCount);
    }

    [Obsolete("Just For Test. Not use anymore.")]
    void _PressGetItem()
    {
        var item = GameManager.Data.StateItemDataReader(500);
        item.count = 70;

        GameManager.Inven.GetItem(item);
    }

    /// <summary>
    /// 인벤토리 밖에 드롭 여부 확인
    /// </summary>
    /// <returns> true = 인벤토리 밖 드롭 </returns>
    public bool CheckUIOutDrop()
    {
        if (dragImg.transform.localPosition.x < panelRect.xMin || dragImg.transform.localPosition.y < panelRect.yMin ||
            dragImg.transform.localPosition.x > panelRect.xMax || dragImg.transform.localPosition.y > panelRect.yMax)
        {
            return true;
        }

        return false;
    }

    public void UpdateGoldPanel(long gold)
    {
        goldText.text = gold.ToString();
    }
}