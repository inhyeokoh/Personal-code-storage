using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FontSetter
{
    public const string path_BMJUA_Font = "Fonts/BMJUA_TTF SDF";
    public const string targetDirectory = "Assets/Resources/Prefabs";

    [MenuItem("Custom/Change Scene Object Fonts(현재 씬 오브젝트들의 모든 폰트 교체)")]
    public static void SetSceneFontsToBMJUA()
    {
        GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var gameObject in rootGameObjects)
        {
            TMP_Text[] allTMPTextComponents = gameObject.GetComponentsInChildren<TMP_Text>();
            foreach (TMP_Text tmpTextComponent in allTMPTextComponents)
            {
                tmpTextComponent.font = Resources.Load<TMP_FontAsset>(path_BMJUA_Font);
                EditorUtility.SetDirty(tmpTextComponent); // 변경 사항을 저장
            }
        }
    }

    [MenuItem("Custom/Change Prefab Fonts(경로 하위의 모든 프리팹의 폰트 교체)")]
    public static void SetPrefabFontsToBMJUA()
    {
        // BMJUA 폰트 로드
        TMP_FontAsset bmjuaFont = Resources.Load<TMP_FontAsset>(path_BMJUA_Font);
        if (bmjuaFont == null)
        {
            Debug.LogError("해당 폰트를 찾을 수 없습니다.");
            return;
        }

        // 특정 경로의 모든 프리팹 로드
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { targetDirectory });
        foreach (string guid in prefabGUIDs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                TMP_Text[] allTMPTextComponents = prefab.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text tmpTextComponent in allTMPTextComponents)
                {
                    tmpTextComponent.font = bmjuaFont;
                    EditorUtility.SetDirty(tmpTextComponent); // 변경 사항을 저장
                }

                // 프리팹이 수정되었음을 표시하고 저장
                PrefabUtility.SavePrefabAsset(prefab);
                Debug.Log($"프리팹 '{prefab.name}'의 폰트를 BMJUA로 변경하였습니다.");
            }
        }
    }
}
