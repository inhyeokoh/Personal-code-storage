using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Esc, Enter, Tab 기능
/// </summary>
public class UserInputOnUI : MonoBehaviour
{
    public static UserInputOnUI instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 현재 화면상에 가장 앞에 위치한 팝업의 확인 버튼 클릭
    /// </summary>
    /// <param name="context"></param>
    public void Enter(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            if (GameManager.UI._activePopupList.Count > 0)
            {
                GameManager.UI._activePopupList.Last.Value.EnterAction();
            }
        }
    }

    /// <summary>
    /// 화면상 가장 앞에 있는 팝업 닫기. 팝업이 없는 상태에서 ESC 입력 -> 이전씬으로 이동
    /// </summary>
    /// <param name="context"></param>
    public void Escape(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            if (GameManager.UI._activePopupList.Count > 0)
            {
                GameManager.UI._activePopupList.Last.Value.EscAction();
            }
            else
            {
                GameManager.Scene.LoadPreviousScene();
            }           
        }
    }
    
    /// <summary>
    /// InputField 간 이동
    /// </summary>
    /// <param name="context"></param>
    public void Tab(InputAction.CallbackContext context)
    {
        if (context.action.phase == InputActionPhase.Performed)
        {
            if (GameManager.UI._activePopupList.Count > 0)
            {
                GameManager.UI._activePopupList.Last.Value.TabAction();
            }
        }
    }
}
