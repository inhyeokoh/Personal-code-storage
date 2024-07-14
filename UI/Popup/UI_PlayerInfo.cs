using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UI_PlayerInfo : UI_Entity
{
    bool _init;
    public GameObject dragImg;
    public GameObject descrPanel;
    GameObject _infoBoard;
    GameObject _statusBoard;
    GameObject equipSlots;

    int _leftSlotCount = 5;

    public Rect panelRect;
    Vector2 _descrUISize;

    // 드래그 Field
    Vector2 _playerInfoUIPos;
    Vector2 _dragBeginPos;
    Vector2 _offset;

    CharacterStatus status;
    TMP_Text _name;
    TMP_Text _job;
    TMP_Text _gender;
    TMP_Text _levelText;
    TMP_Text _expText;
    TMP_Text _hpText;
    TMP_Text _mpText;
    TMP_Text _attackText;
    TMP_Text _atkspeedText;
    TMP_Text _defenseText;
    TMP_Text _moveSpeedText;

    enum Enum_UI_PlayerInfo
    {
        Interact,
        Panel,
        Panel_U,
        Equipments,
        InfoPanel,
        Close,
        DragImg,
        DescrPanel,
        DropConfirm,
        DropCountConfirm
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_PlayerInfo);
    }

    public override void PopupOnEnable()
    {
        if (!_init) return;

        for (int i = 0; i < GameManager.Inven.equips.Count; i++)
        {
            UpdateEquipUI(i);
        }
    }

    public override void PopupOnDisable()
    {
        GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockMouseClick, false);
    }

    protected override void Init()
    {
        base.Init();
        equipSlots = _entities[(int)Enum_UI_PlayerInfo.Equipments].gameObject;
        dragImg = _entities[(int)Enum_UI_PlayerInfo.DragImg].gameObject;
        descrPanel = _entities[(int)Enum_UI_PlayerInfo.DescrPanel].gameObject;
        _infoBoard = _entities[(int)Enum_UI_PlayerInfo.InfoPanel].transform.GetChild(1).gameObject;
        _statusBoard = _entities[(int)Enum_UI_PlayerInfo.InfoPanel].transform.GetChild(3).gameObject;
        panelRect = _entities[(int)Enum_UI_PlayerInfo.Panel].GetComponent<RectTransform>().rect;
        _descrUISize = _GetUISize(descrPanel);
        _DrawSlots();
        _DrawCharacterInfo();
        status = PlayerController.instance._playerStat;


        foreach (var _subUI in _subUIs)
        {
            _subUI.ClickAction = (PointerEventData data) =>
            {
                GameManager.UI.GetPopupForward(GameManager.UI.PlayerInfo);
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

        // 유저 정보 창 드래그 시작
        _entities[(int)Enum_UI_PlayerInfo.Interact].BeginDragAction = (PointerEventData data) =>
        {
            _playerInfoUIPos = transform.position;
            _dragBeginPos = data.position;
        };

        // 유저 정보 창 드래그
        _entities[(int)Enum_UI_PlayerInfo.Interact].DragAction = (PointerEventData data) =>
        {
            _offset = data.position - _dragBeginPos;
            transform.position = _playerInfoUIPos + _offset;
        };

        // 유저 정보 창 닫기
        _entities[(int)Enum_UI_PlayerInfo.Close].ClickAction = (PointerEventData data) =>
        {
            GameManager.UI.ClosePopup(GameManager.UI.PlayerInfo);            
        };

        _init = true;
    }

    // 유저 정보창 장비 슬롯 생성
    void _DrawSlots()
    {
        for (int i = 0; i < _leftSlotCount; i++)
        {
            GameObject _equipSlot = GameManager.Resources.Instantiate("Prefabs/UI/Scene/EquipSlot", equipSlots.transform.GetChild(1));
            _equipSlot.name = "EquipSlot_" + i;
            _equipSlot.GetComponent<UI_EquipSlot>().Index = i;
        }

        for (int i = _leftSlotCount; i < GameManager.Inven.EquipSlotCount; i++)
        {
            GameObject _equipSlot = GameManager.Resources.Instantiate("Prefabs/UI/Scene/EquipSlot", equipSlots.transform.GetChild(2));
            _equipSlot.name = "EquipSlot_" + i;
            _equipSlot.GetComponent<UI_EquipSlot>().Index = i;
        }
    }

    // 유저 정보창 기본정보 및 스탯 표기
    void _DrawCharacterInfo()
    {
        CHARACTER_INFO character = GameManager.Data.CurrentCharacter;
        for (int i = 0; i < 5; i++)
        {
            GameManager.Resources.Instantiate("Prefabs/UI/Scene/Status", _infoBoard.transform);
        }

        _infoBoard.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "캐릭터명";
        _name = _infoBoard.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
        _name.text = $"{character.BaseInfo.Nickname.ToString(System.Text.Encoding.Unicode)}";

        _infoBoard.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = "직업";
        _job = _infoBoard.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>();
        string strJob = Enum.GetName(typeof(Enum_Class), character.BaseInfo.CharacterClass);
        _job.text = $"{strJob}";

        _infoBoard.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = "성별";
        _gender = _infoBoard.transform.GetChild(2).GetChild(1).GetComponent<TMP_Text>();
        string strGender = character.BaseInfo.Gender ? "Men" : "Women";
        _gender.text = $"{strGender}";

        _infoBoard.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = "레벨";
        _levelText = _infoBoard.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>();
        _levelText.text = $"{character.Stat.Level}";

        _infoBoard.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = "경험치";
        _expText = _infoBoard.transform.GetChild(4).GetChild(1).GetComponent<TMP_Text>();
        _expText.text = $"{character.Stat.Exp}/{character.Stat.MaxEXP}";

        for (int i = 0; i < 6; i++)
        {
            GameManager.Resources.Instantiate("Prefabs/UI/Scene/Status", _statusBoard.transform);
        }

        _statusBoard.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "HP";
        _hpText = _statusBoard.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();
        _hpText.text = $"{character.Stat.Hp}/{character.Stat.MaxHP}";

        _statusBoard.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = "MP";
        _mpText = _statusBoard.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>();
        _mpText.text = $"{character.Stat.Mp}/{character.Stat.MaxMP}";
        
         _statusBoard.transform.GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = "공격력";
        _attackText = _statusBoard.transform.GetChild(2).GetChild(1).GetComponent<TMP_Text>();
        _attackText.text = $"{character.Stat.Attack}";

        _statusBoard.transform.GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = "공격 속도";
        _atkspeedText = _statusBoard.transform.GetChild(3).GetChild(1).GetComponent<TMP_Text>();
        _atkspeedText.text = $"{character.Stat.AttackSpeed}";

        _statusBoard.transform.GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = "방어력";
        _defenseText = _statusBoard.transform.GetChild(4).GetChild(1).GetComponent<TMP_Text>();
        _defenseText.text = $"{character.Stat.Defense}";

        _statusBoard.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>().text = "이동 속도";
        _moveSpeedText = _statusBoard.transform.GetChild(5).GetChild(1).GetComponent<TMP_Text>();
        _moveSpeedText.text = $"{character.Stat.Speed}";
    }

    public void UpdateStatus()
    {
        if (!_init) return;

        _levelText.text = $"{status.Level}";
        _expText.text = $"{status.EXP}/{status.MaxEXP}";
        _hpText.text = $"{status.HP}/{status.SumMaxHP}";
        _mpText.text = $"{status.MP}/{status.SumMaxMP}";
        _attackText.text = $"{status.SumAttack}";
        _atkspeedText.text = $"{status.SumAttackSpeed}";
        _defenseText.text = $"{status.SumDefense}";
        _moveSpeedText.text = $"{status.Speed}";
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

    // 아이템 배열 정보에 맞게 UI 갱신 시키는 메서드
    public void UpdateEquipUI(int slotIndex)
    {
        if (slotIndex < _leftSlotCount)
        {
            UI_EquipSlot equipSlot = equipSlots.transform.GetChild(1).GetChild(slotIndex).GetComponent<UI_EquipSlot>();
            equipSlot.ItemRender();
        }
        else
        {
            UI_EquipSlot equipSlot = equipSlots.transform.GetChild(2).GetChild(slotIndex - _leftSlotCount).GetComponent<UI_EquipSlot>();
            equipSlot.ItemRender();
        }
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
