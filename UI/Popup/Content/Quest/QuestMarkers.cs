using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestMarkers : MonoBehaviour
{
    int progressCount = 0;
    /// <summary>
    /// Enum_QuestProgress.CanComplete => 3
    /// Enum_QuestProgress.Available => 2
    /// Enum_QuestProgress.Ongoing => 1
    /// </summary>
    public int ProgressCount
    {
        get { return progressCount; }
        set
        {
            progressCount = value;
            UpdateQuestMarkers();
        }
    }

    private void UpdateQuestMarkers()
    {
        // 모든 자식 오브젝트 비활성화
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        transform.GetChild(ProgressCount).gameObject.SetActive(true);
    }
}
