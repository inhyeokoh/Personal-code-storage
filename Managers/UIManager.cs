using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIManager : SubClass<GameManager>
{
    PlayerInput pi;
    InputActionMap playerActionMap;
    InputAction moveAction;
    InputAction fireAction;

    public GameObject popupCanvas;
    public GameObject toolTipCanvas;

    #region OutGame Popup UI
    public UI_Login Login;
    public UI_SignUp SignUp;
    public UI_InputName InputName;
    public UI_Setting Settings;
    public GameObject BlockAll;
    public UI_ConfirmYN ConfirmYN;
    public UI_ConfirmY ConfirmY;
    GameObject Blocker;
    #endregion

    #region InGame Popup UI
    public UI_Inventory Inventory;
    public UI_PlayerInfo PlayerInfo;
    public UI_Shop Shop;
    public UI_InGameConfirmYN InGameConfirmYN; // 확인,취소 버튼 있는 팝업
    public UI_InGameConfirmY InGameConfirmY; // 확인 버튼만 있는 팝업
    public UI_Dialog Dialog;
    public UI_QuestAccessible QuestAccessible;
    public UI_QuestComplete QuestComplete;
    public UI_SkillWindow SkillWindow;
    public UI_PersonalTrade PersonalTrade;
    #endregion

    #region InGame Scene UI
    public UI_InGameMain InGameMain;
    #endregion

    #region ToolTips
    public UI_ItemToolTip itemToolTip;
    #endregion

    int blockerCount = 0; 

    public LinkedList<UI_Entity> _activePopupList;
    public List<UI_Entity> _tempClosed;

    public enum Enum_ControlInputAction
    {
        None, // 제어 X
        BlockMouseClick, // 좌클릭, 우클릭 제어
        BlockPlayerInput // ActionMap 중 "Player" 제어
    }
    Enum_ControlInputAction _currentInputActionControl = Enum_ControlInputAction.None;

    protected override void _Clear()
    {
    }

    protected override void _Excute()
    {
    }

    protected override void _Init()
    {
        // 커서 화면 밖으로 안 나가도록. 게임 제작중에는 불편해서 주석처리
        // Cursor.lockState = CursorLockMode.Confined;
        _activePopupList = new LinkedList<UI_Entity>();
        _tempClosed = new List<UI_Entity>();
        popupCanvas = GameObject.Find("PopupCanvas");
#if SERVER || DEBUG_MODE || CLIENT_TEST_TITLE
        Object.DontDestroyOnLoad(popupCanvas);
#elif CLIENT_TEST_PROPIM || CLIENT_TEST_HYEOK
        ConnectPlayerInput();
        toolTipCanvas = GameObject.Find("ToolTipCanvas");
        if (toolTipCanvas != null)
        {
            itemToolTip = GameManager.Resources.Instantiate($"Prefabs/UI/ToolTip/ItemToolTip", toolTipCanvas.transform).GetComponent<UI_ItemToolTip>();
        }
        GameManager.Resources.Instantiate($"Prefabs/UI/Base/UserInputOnUI"); // UI 키입력 기능들을 수행할 수 있는 프리팹 생성
        Inventory = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/Inventory", popupCanvas.transform).GetComponent<UI_Inventory>();
        PlayerInfo = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/PlayerInfo", popupCanvas.transform).GetComponent<UI_PlayerInfo>();
        SkillWindow = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/SkillWindow", popupCanvas.transform).GetComponent<UI_SkillWindow>();
        Shop = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/ShopUI", popupCanvas.transform).GetComponent<UI_Shop>();
        Blocker = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/Blocker", popupCanvas.transform);
        InGameConfirmYN = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/InGameConfirmYN", popupCanvas.transform).GetComponent<UI_InGameConfirmYN>();
        InGameConfirmY = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/InGameConfirmY", popupCanvas.transform).GetComponent<UI_InGameConfirmY>();
        Dialog = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/Dialog", popupCanvas.transform).GetComponent<UI_Dialog>();
        QuestAccessible = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/QuestAccessible", popupCanvas.transform).GetComponent<UI_QuestAccessible>();
        QuestComplete = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/QuestComplete", popupCanvas.transform).GetComponent<UI_QuestComplete>();
        PersonalTrade = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/PersonalTrade", popupCanvas.transform).GetComponent<UI_PersonalTrade>();
#endif
    }

    public enum Enum_PopupSetJunction
    {
        Title,
        InGame
    }

    public void SetGamePopups(Enum_PopupSetJunction sceneName)
    {
        switch (sceneName)
        {
            case Enum_PopupSetJunction.Title:
                GameManager.Resources.Instantiate($"Prefabs/UI/Base/UserInputOnUI"); // UI 키입력을 수행할 수 있는 프리팹 생성
                Login = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/Login", popupCanvas.transform).GetComponent<UI_Login>();
                SignUp = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/SignUp", popupCanvas.transform).GetComponent<UI_SignUp>();
                Settings = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/Settings", popupCanvas.transform).GetComponent<UI_Setting>();
                InputName = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/InputName", popupCanvas.transform).GetComponent<UI_InputName>();
                ConfirmYN = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/ConfirmYN", popupCanvas.transform).GetComponent<UI_ConfirmYN>();
                ConfirmY = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/ConfirmY", popupCanvas.transform).GetComponent<UI_ConfirmY>();
                Blocker = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/Blocker", popupCanvas.transform).gameObject;
                BlockAll = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/BlockAll", popupCanvas.transform).gameObject;
                BlockAll.transform.SetAsLastSibling();
                break;
            case Enum_PopupSetJunction.InGame:
                for (int i = 0; i < popupCanvas.transform.childCount; i++)
                {
                    if (popupCanvas.transform.GetChild(i).gameObject.name == "Settings") continue; // OutGamePopup들 중에 환경설정 팝업 제외하고 삭제

                    GameManager.Resources.Destroy(popupCanvas.transform.GetChild(i).gameObject);
                }
                toolTipCanvas = GameObject.Find("ToolTipCanvas");
                if (toolTipCanvas != null)
                {
                    itemToolTip = GameManager.Resources.Instantiate($"Prefabs/UI/ToolTip/ItemToolTip", toolTipCanvas.transform).GetComponent<UI_ItemToolTip>();
                }
                Inventory = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/Inventory", popupCanvas.transform).GetComponent<UI_Inventory>();
                PlayerInfo = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/PlayerInfo", popupCanvas.transform).GetComponent<UI_PlayerInfo>();
                Shop = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/ShopUI", popupCanvas.transform).GetComponent<UI_Shop>();
                Blocker = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/Blocker", popupCanvas.transform).gameObject;
                InGameConfirmYN = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/InGameConfirmYN", popupCanvas.transform).GetComponent<UI_InGameConfirmYN>();
                InGameConfirmY = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/InGameConfirmY", popupCanvas.transform).GetComponent<UI_InGameConfirmY>();
                Dialog = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/Dialog", popupCanvas.transform).GetComponent<UI_Dialog>();
                QuestAccessible = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/QuestAccessible", popupCanvas.transform).GetComponent<UI_QuestAccessible>();
                QuestComplete = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/QuestComplete", popupCanvas.transform).GetComponent<UI_QuestComplete>();
                PersonalTrade = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/PersonalTrade", popupCanvas.transform).GetComponent<UI_PersonalTrade>();
                SkillWindow = GameManager.Resources.Instantiate($"Prefabs/UI/Popup/SkillWindow", popupCanvas.transform).GetComponent<UI_SkillWindow>();
                break;
            default:
                break;
        }
    }

    public void ConnectPlayerInput()
    {
        pi = GameObject.Find("PlayerController").GetComponent<PlayerInput>();
        playerActionMap = pi.actions.FindActionMap("Player");
        moveAction = pi.currentActionMap.FindAction("Move");
        fireAction = pi.currentActionMap.FindAction("Fire");
    }

    public void OpenPopup(UI_Entity targetPopup)
    {
        _activePopupList.AddLast(targetPopup);
        SortPopupView();
        targetPopup.gameObject.SetActive(true);
        targetPopup.PopupOnEnable();
    }

    public void ClosePopup(UI_Entity targetPopup)
    {
        _activePopupList.Remove(targetPopup);
        targetPopup.gameObject.SetActive(false);
        targetPopup.PopupOnDisable();
    }


    // 특정 팝업과 그 하위 팝업들을 닫음
    public void ClosePopupAndChildren(UI_Entity targetPopup)
    {
        if (_activePopupList.Contains(targetPopup))
        {
            _ClosePopupRecursively(targetPopup);
        }
    }

    // targetPopup과 그 하위 팝업들을 재귀적으로 닫음
    void _ClosePopupRecursively(UI_Entity targetPopup)
    {
        ClosePopup(targetPopup);

        // targetPopup의 하위 UI_Entity들에 대해 반복
        foreach (var child in targetPopup.childPopups)
        {
            _ClosePopupRecursively(child);
        }
    }

    /// <summary>
    /// 팝업 모두 닫기
    /// </summary>
    public void CloseAllPopups()
    {
        for (int i = 0; i < popupCanvas.transform.childCount; i++)
        {
            GameObject child = popupCanvas.transform.GetChild(i).gameObject;
            if (child.activeSelf)
            {
                UI_Entity childEntity = child.GetComponent<UI_Entity>();
                ClosePopup(childEntity);
            }
        }
    }

    /// <summary>
    /// 일부 팝업 제외하고 모두 닫기
    /// </summary>
    public void CloseAllPopups(UI_Entity except)
    {
        _tempClosed.Clear();
        for (int i = 0; i < popupCanvas.transform.childCount; i++)
        {
            GameObject child = popupCanvas.transform.GetChild(i).gameObject;
            if (child.activeSelf)
            {
                UI_Entity childEntity = child.GetComponent<UI_Entity>();
                if (childEntity == except) continue;

                ClosePopup(childEntity);
                _tempClosed.Add(childEntity);
            }
        }
    }

    public void ReOpenPopups()
    {
        foreach (var popup in _tempClosed)
        {
            OpenPopup(popup);
        }
        _tempClosed.Clear();
    }

    // 가장 마지막에 연 팝업이 화면상 가장 위에 오도록
    public void SortPopupView()
    {
        if (_activePopupList.Last.Value != Blocker)
        {
            _activePopupList.Last.Value.transform.SetAsLastSibling();
        }
    }

    // 클릭한 팝업이 가장 앞으로 오도록
    public void GetPopupForward(UI_Entity targetPopup)
    {
        _activePopupList.Remove(targetPopup);
        _activePopupList.AddLast(targetPopup);
        SortPopupView();
    }

    public void UseBlocker(bool activate)
    {
        if (activate)
        {
            if (blockerCount == 0)
            {
                Blocker.SetActive(true);
            }
            blockerCount++;
        }
        else
        {
            blockerCount--;
            if (blockerCount <= 0)
            {
                Blocker.SetActive(false);
                blockerCount = 0; // 음수 방지
            }
        }

        // Blocker의 위치를 활성화 팝업 바로 위로 설정
        int activatePopupIndex = 0;
        for (int i = 0; i < popupCanvas.transform.childCount; i++)
        {
            if (popupCanvas.transform.GetChild(i).gameObject.activeSelf)
            {
                activatePopupIndex = i;
            }
        }
        int beforeLast = activatePopupIndex - 1;
        Blocker.transform.SetSiblingIndex(beforeLast);
    }


    public void OpenOrClose(UI_Entity targetPopup)
    {
        if (!targetPopup.gameObject.activeSelf)
        {
            OpenPopup(targetPopup);
        }
        else
        {
            ClosePopup(targetPopup);
        }
    }

    public bool PointerOnUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// InputAction 제어 메서드
    /// </summary>
    /// <param name="blockType"> 일부 행동 제어, ActionMap 전체 제어 구분 </param>
    /// <param name="block"> true = block </param>
    public void BlockPlayerActions(Enum_ControlInputAction blockType, bool block)
    {
        switch (blockType)
        {
            case Enum_ControlInputAction.BlockMouseClick:
                _SetActionState(moveAction, block);
                _SetActionState(fireAction, block);
                break;
            case Enum_ControlInputAction.BlockPlayerInput:
                _SetActionMap(playerActionMap, block);
                break;
            default:
                break;
        }
    }

    void _SetActionState(InputAction action, bool state)
    {
        if (_currentInputActionControl == Enum_ControlInputAction.BlockPlayerInput) return; // 전체 제어중이면 return

        if (state)
        {
            action.Disable();
            _currentInputActionControl = Enum_ControlInputAction.BlockMouseClick;
        }
        else
        {
            action.Enable();
            _currentInputActionControl = Enum_ControlInputAction.None;
        }
    }

    void _SetActionMap(InputActionMap actionMap, bool state)
    {
        if (state)
        {
            actionMap.Disable();
            _currentInputActionControl = Enum_ControlInputAction.BlockPlayerInput;
        }
        else
        {
            actionMap.Enable();
            _currentInputActionControl = Enum_ControlInputAction.None;
        }
    }
}