using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class SceneControlManager : SubClass<GameManager>
{
    int curSceneIdx;
    public Enum_Scenes curScene;

    public enum Enum_Scenes
    {
        Title,
        Select,
        Create,
        StatePattern,
        Inventory,
        Loading
    }

    protected override void _Clear()
    {
    }

    protected override void _Excute()
    {
    }

    protected override void _Init()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
#if SERVER || DEBUG_MODE || CLIENT_TEST_TITLE
        GameManager.UI.SetGamePopups(UIManager.Enum_PopupSetJunction.Title);
#endif
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        curSceneIdx = SceneManager.GetActiveScene().buildIndex;
        Enum_Scenes curScene = (Enum_Scenes)curSceneIdx;

        if (curSceneIdx == (int)Enum_Scenes.Title)
        {
            GameManager.UI.OpenPopup(GameManager.UI.Login);
        }
#if CLIENT_TEST_TITLE // 서버 미연결 상태에서 로그인 성공 시, 씬 이동전에 팝업 닫는 기능을 대체
        else if (curSceneIdx == (int)Enum_Scenes.Select || curSceneIdx == (int)Enum_Scenes.Create)
        {
            GameManager.UI.ClosePopup(GameManager.UI.Login);
        }
#endif
#if SERVER || DEBUG_MODE || CLIENT_TEST_TITLE
        else if (curSceneIdx == (int)Enum_Scenes.StatePattern)
        {
            GameManager.UI.ConnectPlayerInput();
            GameManager.Data.NpcTableParsing("NpcTable");
            GameManager.UI.SetGamePopups(UIManager.Enum_PopupSetJunction.StatePattern);
            GameManager.Inven.ConnectInven();

            for (int i = 0; i < GameManager.Data.dropTestItems.Count; i++)
            {
                ItemManager._item.ItemInstance(GameManager.Data.dropTestItems[i], GameManager.Data.dropTestItemsPos[i], Quaternion.identity);
            }
        }
#endif
    }

    // 이전에 있던 씬으로 이동
    public void LoadPreviousScene()
    {
        if (curSceneIdx == (int)Enum_Scenes.Title)
        {
            ExitGame();
        }
        else if (curSceneIdx <= (int)Enum_Scenes.Create)
        {
            SceneManager.LoadScene(--curSceneIdx);
        }
        else if (curSceneIdx == (int)Enum_Scenes.StatePattern || curSceneIdx == (int)Enum_Scenes.Inventory)
        {
            // TODO 게임 종료 묻는 팝업 띄우고 로그인 화면으로 전환
            ExitGame();
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

