using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enum_QuestType
{
    NoneRepeat,
    Repeat, // 바로 반복 수행 가능
    DailyRepeat,
    WeeklyRepeat
}

// 특정 레벨 도달 및 이전 퀘스트 완료 시 NotAvailable -> Available
public enum Enum_QuestProgress
{
    UnAvailable,
    Available,
    Ongoing,
    CanComplete,
    Completed
}

public class Quest
{
    public Enum_QuestProgress progress { get; set; }
    public QuestData questData;

    // 완료 조건
    public int questObjCount;
    public int questMonsterCount;

    public bool previousQuestComplete = true;
    bool _canComplete;

    // 퀘스트 상태 변화 이벤트
    public delegate void QuestProgressChanged();
    public event QuestProgressChanged OnQuestProgressChanged;
    public Quest(int questID)
    {
        SetProgress(Enum_QuestProgress.UnAvailable);
        questData = GameManager.Data.questDict[questID];

        if (GameManager.Data.npcDict.ContainsKey(questData.npcID))
        {
            GameManager.Data.npcDict[questData.npcID].DetectQuestProgress(this);
        }
    }

    public void SetProgress(Enum_QuestProgress newProgress)
    {
        if (progress != newProgress)
        {
            progress = newProgress;
            OnQuestProgressChanged?.Invoke();
        }
    }

    void _CheckGoals()
    {
        foreach (var goal in questData.goals)
        {
            if (!goal.IsCompleted())
            {
                _canComplete = false;
                return;
            }
        }

        SetProgress(Enum_QuestProgress.CanComplete);
        _canComplete = true;
    }


    /// <summary>
    /// 퀘스트 시작 시에 이벤트 수신
    /// </summary>
    public void ReceiveEventWhenQuestStarts()
    {
        if (questData.goals.Count == 0) return;

        foreach (var goal in questData.goals)
        {
            if (goal is MonsterGoal monsterGoal)
            {
                MonsterState.OnMonsterKilled += _OnMonsterKilled;
            }
            else if (goal is ObjectGoal objGoal)
            {
                GameManager.Inven.OnItemGet += OnItemGet;
            }
        }
    }

    void _OnMonsterKilled(MonsterData monsterData)
    {
        foreach (var goal in questData.goals)
        {
            if (goal is MonsterGoal monsterGoal)
            {
                if (monsterData.monster_id == monsterGoal.MonsterID)
                {
                    monsterGoal.IncrementCount(1);
                }
            }
        }
        _CheckGoals();
    }

    public void OnItemGet(ItemData itemData)
    {
        foreach (var goal in questData.goals)
        {
            if (goal is ObjectGoal objGoal)
            {
                if (itemData.id == objGoal.ObjectID)
                {
                    objGoal.IncrementCount(itemData.count);
                }
            }
        }
        _CheckGoals();
    }
}
