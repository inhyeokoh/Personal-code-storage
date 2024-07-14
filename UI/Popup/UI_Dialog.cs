using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Dialog : UI_Entity
{
    bool _init;
    VCamSwitcher vCamSwitcher;

    #region 대화창
    TMP_Text mainText;
    GameObject nextButton;
    GameObject acceptButton;
    int dialogCount;
    #endregion

    Quest selectedQuest;

    enum Enum_UI_Dialog
    {
        DialogPanel,
        MainText,
        Next,
        Accept,
        Cancel
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_Dialog);
    }

    public override void PopupOnEnable()
    {
        if (PlayerController.instance._interaction.InteractingNpcID == -1) return;

        GameManager.UI.CloseAllPopups(this);
        GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockPlayerInput, true);
        nextButton.SetActive(false);
        acceptButton.SetActive(false);
        vCamSwitcher?.SwitchToVCam2();

        mainText.text = GameManager.Data.npcDict[PlayerController.instance._interaction.InteractingNpcID].DefaultText;
    }

    public override void PopupOnDisable()
    {
        if (PlayerController.instance._interaction.InteractingNpcID == -1) return;

        GameManager.UI.ReOpenPopups();
        GameManager.UI.BlockPlayerActions(UIManager.Enum_ControlInputAction.BlockPlayerInput, false);
        vCamSwitcher?.SwitchToVCam1();
    }

    protected override void Init()
    {
        base.Init();

        vCamSwitcher = GameObject.FindWithTag("vCamSwitcher").GetComponent<VCamSwitcher>();
        nextButton = _entities[(int)Enum_UI_Dialog.Next].gameObject;
        acceptButton = _entities[(int)Enum_UI_Dialog.Accept].gameObject;
        mainText = _entities[(int)Enum_UI_Dialog.MainText].GetComponent<TMP_Text>();

        _entities[(int)Enum_UI_Dialog.Next].ClickAction = (PointerEventData data) => {
            _ContinueDialog();
        };

        _entities[(int)Enum_UI_Dialog.Accept].ClickAction = (PointerEventData data) => {
            switch (selectedQuest.progress)
            {
                case Enum_QuestProgress.Available:
                    GameManager.Quest.ReceiveQuest(selectedQuest.questData.questID);
                    GameManager.UI.ClosePopup(GameManager.UI.Dialog);
                    break;
                case Enum_QuestProgress.CanComplete:
                    GameManager.Quest.CompleteQuest(selectedQuest.questData.questID);
                    GameManager.UI.ClosePopup(GameManager.UI.Dialog);
                    break;
                default:
#if UNITY_EDITOR
                    Debug.Assert(false);
#endif
                    break;
            }
        };

        _entities[(int)Enum_UI_Dialog.Cancel].ClickAction = (PointerEventData data) => {
            switch (GameManager.Data.npcDict[PlayerController.instance._interaction.InteractingNpcID].npcType)
            {
                case Enum_NpcType.Quest:
                    if (selectedQuest == null || selectedQuest.progress == Enum_QuestProgress.Available)
                    {
                        GameManager.UI.ClosePopup(GameManager.UI.QuestAccessible);
                    }
                    else if (selectedQuest.progress == Enum_QuestProgress.CanComplete)
                    {
                        // 나가기는 퀘스트 완료 보상 안 받음
                        GameManager.UI.ClosePopup(GameManager.UI.QuestAccessible);
                        GameManager.UI.ClosePopup(GameManager.UI.QuestComplete);
                    }
                    break;
                case Enum_NpcType.Shop:
                    GameManager.UI.ClosePopup(GameManager.UI.Shop);
                    break;
                default:
#if UNITY_EDITOR
                    Debug.Assert(false);
#endif
                    break;
            }
            GameManager.UI.ClosePopup(GameManager.UI.Dialog);
        };

        gameObject.SetActive(false);
    }

    public void StartDialog()
    {
        GameManager.UI.ClosePopup(GameManager.UI.QuestAccessible);
        nextButton.SetActive(true);
        selectedQuest = GameManager.Quest.CurrentSelectedQuest;
        dialogCount = 0;
        _ContinueDialog();
    }

    void _ContinueDialog()
    {
        switch (selectedQuest.progress)
        {
            case Enum_QuestProgress.Available:
                mainText.text = selectedQuest.questData.conversationText[dialogCount++];
                if (dialogCount == selectedQuest.questData.conversationText.Length)
                {
                    nextButton.SetActive(false);
                    acceptButton.SetActive(true);
                }
                break;
            case Enum_QuestProgress.Ongoing:
                mainText.text = selectedQuest.questData.ongoingText;
                nextButton.SetActive(false);
                break;
            case Enum_QuestProgress.CanComplete:
                mainText.text = selectedQuest.questData.completeText[dialogCount++];
                if (dialogCount == selectedQuest.questData.completeText.Length)
                {
                    nextButton.SetActive(false);
                    GameManager.UI.OpenPopup(GameManager.UI.QuestComplete);
                }
                break;
            default:
#if UNITY_EDITOR
                Debug.Assert(false);
#endif
                break;
        }
    }
}