using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class UI_QuestAccessible : UI_Entity
{
    #region 퀘스트 목록
    List<Quest> accessibleQuests;
    GameObject questList;
    ToggleGroup toggleGroup;
    GameObject canComplete;
    GameObject available;
    GameObject ongoing;
    Quest selectedQuest;

    List<Toggle> allQuestToggles;
    #endregion

    enum Enum_UI_QuestAccessible
    {
        QuestList,
        Next
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_QuestAccessible);
    }

    public override void PopupOnEnable()
    {
        if (accessibleQuests == null) return;

        _ShowQuests();
    }

    public override void PopupOnDisable()
    {
        if (accessibleQuests == null) return;

        _DeactivateQuestToggles();
    }

    protected override void Init()
    {
        base.Init();

        questList = _entities[(int)Enum_UI_QuestAccessible.QuestList].gameObject;
        toggleGroup = questList.GetComponent<ToggleGroup>();
        canComplete = questList.transform.GetChild(0).gameObject;
        available = questList.transform.GetChild(1).gameObject;
        ongoing = questList.transform.GetChild(2).gameObject;

        allQuestToggles = new List<Toggle>();

        _entities[(int)Enum_UI_QuestAccessible.Next].ClickAction = (PointerEventData data) => {
            if (selectedQuest == null) return;

            GameManager.Quest.CurrentSelectedQuest = selectedQuest;
            GameManager.UI.Dialog.StartDialog();
        };

        gameObject.SetActive(false);
    }

    public bool CheckAccessibleQuests(int npcID)
    {
        accessibleQuests = new List<Quest>();
        foreach (var quest in GameManager.Quest.questsByNpcID[npcID])
        {
            // 시작 불가, 완료된 퀘스트는 제외
            if (quest.progress != Enum_QuestProgress.UnAvailable && quest.progress != Enum_QuestProgress.Completed)
            {
                accessibleQuests.Add(quest);
            }
        }

        return accessibleQuests.Count != 0;
    }

    void _ShowQuests()
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
        toggleComponent.group = toggleGroup;
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
        // Get
        foreach (var toggle in allQuestToggles)
        {
            if (!toggle.gameObject.activeSelf)
            {
                toggle.transform.SetParent(parent.transform.GetChild(1).transform);
                return toggle;
            }
        }

        // Create
        GameObject toggleObject = GameManager.Resources.Instantiate("Prefabs/UI/Scene/QuestNameToggle", parent.transform.GetChild(1).transform);
        Toggle newToggle = toggleObject.GetComponent<Toggle>();
        allQuestToggles.Add(newToggle);
        return newToggle;
    }

    void _DeactivateQuestToggles()
    {
        for (int i = 0; i < questList.transform.childCount; i++)
        {
            if (questList.transform.GetChild(i).gameObject.activeSelf)
            {
                questList.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        foreach (var toggle in allQuestToggles)
        {
            toggle.gameObject.SetActive(false);
        }
    }
}