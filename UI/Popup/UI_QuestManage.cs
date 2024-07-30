using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UI_QuestManage : UI_Entity
{
    // ---------진행도별 분류--------
    int _progressClassifyCount;
    Toggle[] _progressClassifyToggles;
    ToggleGroup _classifyToggleGroup;
    List<Quest> _availableQuestList; // 시작가능 퀘스트 목록
    List<Quest> _onGoingQuestList; // 진행중인 퀘스트 목록
    List<Quest> _completedQuestList; // 완료 퀘스트 목록

    ToggleGroup _questNameToggleGroup;

    // 드래그 Field
    Vector2 _UIPos;
    Vector2 _dragBeginPos;
    Vector2 _offset;

    enum Enum_UI_QuestManage
    {
        Panel,
        Interact,
        ProgressClassify,
        ScrollView,
        Close
    }

    enum Enum_QuestProgressClassify
    {
        시작가능,
        진행중,
        완료
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_QuestManage);
    }

    protected override void Init()
    {
        base.Init();

        _progressClassifyCount = Enum.GetValues(typeof(Enum_QuestProgressClassify)).Length;
        _classifyToggleGroup = _entities[(int)Enum_UI_QuestManage.ProgressClassify].gameObject.GetComponent<ToggleGroup>();
        _questNameToggleGroup = _entities[(int)Enum_UI_QuestManage.ScrollView].transform.GetChild(0).GetChild(0).GetComponent<ToggleGroup>();
        _availableQuestList = GameManager.Quest.availableQuestList;
        _onGoingQuestList = GameManager.Quest.onGoingQuestList;
        _completedQuestList = GameManager.Quest.completedQuestList;
        _SetProgressClassifyToggles();

        foreach (var _subUI in _subUIs)
        {
            _subUI.ClickAction = (PointerEventData data) =>
            {
                //GameManager.UI.GetPopupForward(GameManager.UI);
            };
        }
        // 버튼 기능 할당      
        _entities[(int)Enum_UI_QuestManage.Close].ClickAction = (PointerEventData data) =>
        {
            //GameManager.UI.ClosePopup(GameManager.UI);
        };

        // 팝업 드래그
        _entities[(int)Enum_UI_QuestManage.Interact].BeginDragAction = (PointerEventData data) =>
        {
            _UIPos = transform.position;
            _dragBeginPos = data.position;
        };

        _entities[(int)Enum_UI_QuestManage.Interact].DragAction = (PointerEventData data) =>
        {
            _offset = data.position - _dragBeginPos;
            transform.position = _UIPos + _offset;
        };

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 퀘스트 진행도에 따른 토글 분류
    /// </summary>
    void _SetProgressClassifyToggles()
    {
        _progressClassifyToggles = new Toggle[_progressClassifyCount];
        for (int i = 0; i < _progressClassifyCount; i++)
        {
            GameObject progressToggle = GameManager.Resources.Instantiate("Prefabs/UI/Scene/QuestProgressToggle", _entities[(int)Enum_UI_QuestManage.ProgressClassify].transform);
            TMP_Text togName = progressToggle.GetComponentInChildren<TMP_Text>();
            switch (i)
            {
                case 0:
                    togName.text = Enum.GetName(typeof(Enum_QuestProgressClassify), 0);
                    break;
                case 1:
                    togName.text = Enum.GetName(typeof(Enum_QuestProgressClassify), 1);
                    break;
                case 2:
                    togName.text = Enum.GetName(typeof(Enum_QuestProgressClassify), 2);
                    break;
                default:
                    break;
            }

            int index = i;
            _progressClassifyToggles[i] = progressToggle.GetComponent<Toggle>();
            _progressClassifyToggles[i].group = _classifyToggleGroup;
            _progressClassifyToggles[i].onValueChanged.AddListener((value) => _ProgressTypeChanged(index));
        }

        _progressClassifyToggles[0].isOn = true; // 첫번째 항목 선택
    }
    void _ProgressTypeChanged(int toggleIndex)
    {
        bool isToggleOn = _progressClassifyToggles[toggleIndex].isOn;
    }

/*    void _ShowQuests()
    {
        bool isFirstToggle = true;

        foreach (var quest in accessibleQuests)
        {
            switch (quest.progress)
            {
                case Enum_QuestProgress.Available:
                    CreateOrReuseToggle(available, quest, ref isFirstToggle);
                    break;
                case Enum_QuestProgress.Ongoing:
                    CreateOrReuseToggle(ongoing, quest, ref isFirstToggle);
                    break;
                case Enum_QuestProgress.CanComplete:
                    CreateOrReuseToggle(canComplete, quest, ref isFirstToggle);
                    break;
                default:
#if UNITY_EDITOR
                    Debug.Assert(false);
#endif
                    break;
            }
        }
    }

    void CreateOrReuseToggle(GameObject parent, Quest quest, ref bool isFirstToggle)
    {
        Toggle toggleComponent = GetOrCreateToggle(parent);
        toggleComponent.group = _questNameToggleGroup;
        toggleComponent.GetComponentInChildren<TMP_Text>().text = quest.questData.title;
        toggleComponent.onValueChanged.RemoveAllListeners();
        toggleComponent.onValueChanged.AddListener((value) => {
            if (value)
            {
                selectedQuest = quest;
            }
            else
            {
                selectedQuest = null;
            }
        });

        toggleComponent.gameObject.SetActive(true);
        parent.gameObject.SetActive(true);

        if (isFirstToggle)
        {
            toggleComponent.isOn = true;
            selectedQuest = quest;
            isFirstToggle = false;
        }
    }

    Toggle GetOrCreateToggle(GameObject parent)
    {
        foreach (var toggle in allQuestToggles)
        {
            if (!toggle.gameObject.activeSelf)
            {
                toggle.transform.SetParent(parent.transform.GetChild(1).transform);
                return toggle;
            }
        }

        GameObject toggleObject = GameManager.Resources.Instantiate("Prefabs/UI/Scene/QuestNameToggle", parent.transform.GetChild(1).transform);
        Toggle newToggle = toggleObject.GetComponent<Toggle>();
        allQuestToggles.Add(newToggle);
        return newToggle;
    }

    void _DeactivateQuestToggles()
    {
        foreach (var toggle in allQuestToggles)
        {
            toggle.gameObject.SetActive(false);
        }
    }*/
}
