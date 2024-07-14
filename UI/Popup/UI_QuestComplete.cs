using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_QuestComplete : UI_Entity
{
    TMP_Text _summaryText;
    public GameObject closeBtn;
    GameObject _rewards;
    QuestData currentQuestData;

    #region UI 드래그
    Vector2 _UIPos;
    Vector2 _dragBeginPos;
    Vector2 _offset;
    #endregion

    enum Enum_UI_QuestReward
    {
        Interact,
        SummaryPanel,
        RewardPanel,
        Accept,
        Close
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_QuestReward);
    }

    public override void PopupOnEnable()
    {
        if (GameManager.Quest.CurrentSelectedQuest == null) return;

        currentQuestData = GameManager.Quest.CurrentSelectedQuest.questData;
        _DrawRewards();
        _summaryText.text = currentQuestData.summaryText;
        closeBtn.SetActive(false);
    }

    public override void PopupOnDisable()
    {
        if (GameManager.Quest.CurrentSelectedQuest == null) return;

        currentQuestData = null;
        _DeleteRewards();
    }

    protected override void Init()
    {
        base.Init();
        #region 초기설정
        _summaryText = _entities[(int)Enum_UI_QuestReward.SummaryPanel].transform.GetChild(1).GetComponent<TMP_Text>();
        _rewards = _entities[(int)Enum_UI_QuestReward.RewardPanel].transform.GetChild(1).gameObject;
        closeBtn = _entities[(int)Enum_UI_QuestReward.Close].gameObject;
        #endregion

        foreach (var _subUI in _subUIs)
        {
            _subUI.ClickAction = (PointerEventData data) =>
            {
                GameManager.UI.GetPopupForward(this);
            };
        }

        // 팝업 드래그
        _entities[(int)Enum_UI_QuestReward.Interact].BeginDragAction = (PointerEventData data) =>
        {
            _UIPos = transform.position;
            _dragBeginPos = data.position;
        };
        _entities[(int)Enum_UI_QuestReward.Interact].DragAction = (PointerEventData data) =>
        {
            _offset = data.position - _dragBeginPos;
            transform.position = _UIPos + _offset;
        };

        _entities[(int)Enum_UI_QuestReward.Accept].ClickAction = (PointerEventData data) =>
        {
            GameManager.Quest.CompleteQuest(currentQuestData.questID);
            GameManager.UI.ClosePopup(GameManager.UI.Dialog);
            GameManager.UI.ClosePopup(this);
        };

        _entities[(int)Enum_UI_QuestReward.Close].ClickAction = (PointerEventData data) =>
        {
            GameManager.UI.ClosePopup(this);
        };

        gameObject.SetActive(false);
    }

    // 보상 생성
    void _DrawRewards()
    {
        if (currentQuestData.expReward != -1)
        {
            GameObject expReward = GameManager.Resources.Instantiate("Prefabs/UI/Scene/Reward", _rewards.transform);
            expReward.GetComponentInChildren<Image>().sprite = GameManager.Resources.Load<Sprite>("Materials/ItemIcons/Exp");
            expReward.GetComponentInChildren<TMP_Text>().text = currentQuestData.expReward.ToString();
        }

        if (currentQuestData.goldReward != -1)
        {
            GameObject goldReward = GameManager.Resources.Instantiate("Prefabs/UI/Scene/Reward", _rewards.transform);
            goldReward.GetComponentInChildren<Image>().sprite = GameManager.Resources.Load<Sprite>("Materials/ItemIcons/coins");
            goldReward.GetComponentInChildren<TMP_Text>().text = currentQuestData.goldReward.ToString();
        }

        // itemRewards 없는경우 추가
        for (int i = 0; i < currentQuestData.itemRewards.Count; i++)
        {
            GameObject itemReward = GameManager.Resources.Instantiate("Prefabs/UI/Scene/Reward", _rewards.transform);
            itemReward.GetComponentInChildren<Image>().sprite = currentQuestData.itemRewards[i].icon;
            itemReward.GetComponentInChildren<TMP_Text>().text = currentQuestData.itemRewards[i].count.ToString();
        }
    }

    void _DeleteRewards()
    {
        if (_rewards.transform.childCount == 0) return;

        for (int i = 0; i < _rewards.transform.childCount; i++)
        {
            GameManager.Resources.Destroy(_rewards.transform.GetChild(i).gameObject);
        }
    }
}
