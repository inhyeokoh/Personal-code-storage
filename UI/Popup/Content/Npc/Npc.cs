using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Enum_NpcType
{
    Quest,
    Shop
    // TODO : 제작 Npc 등등
}

public class Npc : MonoBehaviour
{
    string npcName;
    [SerializeField]
    int npcID;

    public string NpcName
    {
        get { return npcName; }
        set { npcName = value; }
    }
    public int NpcID
    {
        get { return npcID; }
        set { npcID = value; }
    }
    public Enum_NpcType npcType { get; set; }
    public string DefaultText { get; set; }

    [SerializeField]
    QuestMarkers questMarker;

    /// <summary>
    /// NPC가 퀘스트 진행 상황 변화에 맞게 퀘스트 아이콘 업데이트할 수 있도록 함
    /// </summary>
    public void DetectQuestProgress(Quest quest)
    {
        quest.OnQuestProgressChanged += UpdateQuestIcon;
    }

    /// <summary>
    /// NPC와 상호작용 시작. 대화 팝업이 생성되고, NPC 타입에 따라 퀘스트 팝업이나 상점 팝업 열림.
    /// </summary>
    public void StartInteract()
    {
        GameManager.UI.OpenPopup(GameManager.UI.Dialog);
        switch (npcType)
        {
            case Enum_NpcType.Quest:
                if (GameManager.UI.QuestAccessible.CheckAccessibleQuests(npcID))
                {
                    GameManager.UI.OpenPopup(GameManager.UI.QuestAccessible);
                }
                break;
            case Enum_NpcType.Shop:
                GameManager.UI.Shop.shopPurchase.DrawSellingItems(npcID); // 수정 필요
                GameManager.UI.OpenPopup(GameManager.UI.Shop);
                break;
            default:
#if UNITY_EDITOR
                Debug.Assert(false);
#endif
                break;
        }
    }

    /// <summary>
    /// 퀘스트 상태에 따라 아이콘 업데이트
    /// </summary>
    public void UpdateQuestIcon()
    {
        if (GameManager.Quest.questsByNpcID.TryGetValue(npcID, out var quests))
        {
            int progressCount = GameManager.Quest.questsByNpcID[npcID].Select(quest => quest.progress switch
            {
                Enum_QuestProgress.CanComplete => 3,
                Enum_QuestProgress.Available => 2,
                Enum_QuestProgress.Ongoing => 1,
                _ => 0
            }).Max();

            questMarker.ProgressCount = progressCount;
        }
        else
        {
            return; // 키가 존재하지 않음
        }
    }
}
