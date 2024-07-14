using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Google.Protobuf;
using System.Collections.Generic;

public class UI_InputName : UI_Entity
{
    public string nickname;

    enum Enum_UI_InputName
    {
        Panel,
        Instruction,
        InputField,
        Accept,
        Cancel,
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_InputName);
    }


    protected override void Init()
    {
        base.Init();

        inputFields = new List<TMP_InputField>();
        TMP_InputField inputField = _entities[(int)Enum_UI_InputName.InputField].GetComponent<TMP_InputField>();
        inputFields.Add(inputField);

        TMP_Text _instruction = _entities[(int)Enum_UI_InputName.Instruction].GetComponentInChildren<TMP_Text>();
        _instruction.text = "한글, 영문, 숫자 포함 2 ~ 12자로 입력하세요.";

        foreach (var _subUI in _subUIs)
        {
            _subUI.ClickAction = (PointerEventData data) =>
            {
                GameManager.UI.GetPopupForward(GameManager.UI.InputName);
            };
        }

        _entities[(int)Enum_UI_InputName.Accept].ClickAction = (PointerEventData data) => {
            nickname = _entities[(int)Enum_UI_InputName.InputField].GetComponent<TMP_InputField>().text;

            string nickChecker = Regex.Replace(nickname, @"[^0-9a-zA-Z가-힣]", "", RegexOptions.Singleline);

            if (nickname.Equals(nickChecker) == false)
            {
                childPopups.Add(GameManager.UI.ConfirmY);
                GameManager.UI.ConfirmY.GetComponent<UI_ConfirmY>().ChangeText(UI_ConfirmY.Enum_ConfirmTypes.NoSpecialCharacters);
            }
            else if (nickname.Length < 2 || nickname.Length > 12)
            {
                childPopups.Add(GameManager.UI.ConfirmY);
                GameManager.UI.ConfirmY.GetComponent<UI_ConfirmY>().ChangeText(UI_ConfirmY.Enum_ConfirmTypes.LimitNickNameLength);
            }
            else
            {
#if SERVER || DEBUG_MODE
                C_NICKNAME nick_DupAsk_pkt = new C_NICKNAME();
                nick_DupAsk_pkt.Nickname = ByteString.CopyFrom(nickname, System.Text.Encoding.Unicode);
                GameManager.Network.Send(PacketHandler.Instance.SerializePacket(nick_DupAsk_pkt));
#elif CLIENT_TEST_TITLE
                GameManager.Data.CurrentCharacter.BaseInfo.Nickname = ByteString.CopyFrom(nickname, System.Text.Encoding.Unicode);
                childPopups.Add(GameManager.UI.ConfirmYN);
                GameManager.UI.OpenPopup(GameManager.UI.ConfirmYN);
                GameManager.UI.ConfirmYN.ChangeText(UI_ConfirmYN.Enum_ConfirmTypes.AskDecidingNickName);      
#endif
            };
        };

        _entities[(int)Enum_UI_InputName.Cancel].ClickAction = (PointerEventData data) => {
            GameManager.UI.ClosePopup(GameManager.UI.InputName);
        };

        gameObject.SetActive(false);
    }

    public override void EnterAction()
    {
        base.EnterAction();
        _entities[(int)Enum_UI_InputName.Accept].ClickAction?.Invoke(null);
    }
}
