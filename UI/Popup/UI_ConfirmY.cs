using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 확인 버튼이 있는 팝업. 취소로 변경 가능.
/// </summary>
public class UI_ConfirmY : UI_Entity
{
    bool _init;
    bool _useBlocker = true;
    TMP_Text _mainText;
    Enum_ConfirmTypes confirmType;

    enum Enum_UI_Confirm
    {
        Panel,
        Interact,
        MainText,
        Accept
    }

    public enum Enum_ConfirmTypes
    {
        TimeOut,
        ExistID,
        ExistUser,
        SignUpSuccess,
        LoginFail,
        NoSpecialCharacters,
        LimitNickNameLength,
        ExistNickName,
        CharacterDeleteFail,
        CharacterDeleteSuccess
    }

    public override void PopupOnEnable()
    {
        if (!_init || !_useBlocker) return;

        GameManager.UI.UseBlocker(true);
    }

    public override void PopupOnDisable()
    {
        GameManager.UI.UseBlocker(false);
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_Confirm);
    }

    protected override void Init()
    {
        base.Init();

        _mainText = _entities[(int)Enum_UI_Confirm.MainText].GetComponent<TMP_Text>();

        _entities[(int)Enum_UI_Confirm.Accept].ClickAction = (PointerEventData data) => {
            if (confirmType == Enum_ConfirmTypes.SignUpSuccess)
            {
                GameManager.UI.ClosePopup(GameManager.UI.SignUp);
            }
            else
            {
                GameManager.UI.ClosePopup(GameManager.UI.ConfirmY);
            }
        };

        gameObject.SetActive(false);
        _init = true;
    }

    public void ChangeText(Enum_ConfirmTypes type)
    {
        confirmType = type;

        switch (confirmType)
        {
            case Enum_ConfirmTypes.TimeOut:
                _mainText.text = $"Time Out";
                break;
            case Enum_ConfirmTypes.ExistID:
                _mainText.text = $"존재하는 ID입니다.";
                break;
            case Enum_ConfirmTypes.ExistUser:
                _mainText.text = $"이미 가입된 회원입니다.";
                break;
            case Enum_ConfirmTypes.SignUpSuccess:
                _mainText.text = $"성공적으로 가입되었습니다.";
                break;
            case Enum_ConfirmTypes.LoginFail:
                _mainText.text = $"로그인에 실패했습니다. 잠시 후에 다시 시도하십시오.";
                break;
            case Enum_ConfirmTypes.NoSpecialCharacters:
                _mainText.text = $"특수문자는 사용이 불가합니다.";
                break;
            case Enum_ConfirmTypes.LimitNickNameLength:
                _mainText.text = $"2자 이상 12자 이하로 입력하십시오.";
                break;
            case Enum_ConfirmTypes.ExistNickName:
                _mainText.text = $"존재하는 닉네임 입니다!";
                break;
            case Enum_ConfirmTypes.CharacterDeleteFail:
                _mainText.text = $"캐릭터 삭제에 실패하였습니다.";
                break;
            case Enum_ConfirmTypes.CharacterDeleteSuccess:
                _mainText.text = $"캐릭터가 삭제 되였습니다.";
                break;
            default:
                break;
        }
    }

    public override void EnterAction()
    {
        base.EnterAction();
        _entities[(int)Enum_UI_Confirm.Accept].ClickAction?.Invoke(null);
    }
}
