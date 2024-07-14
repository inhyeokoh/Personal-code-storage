using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SignUp : UI_Entity
{
    public TMP_Text msg;

    enum Enum_UI_SignUp
    {
        Panel,
        IDField,
        PWField,
        CheckResult,
        Create,
        Cancel        
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_SignUp);
    }

    protected override void Init()
    {
        base.Init();

        inputFields = new List<TMP_InputField>();
        TMP_InputField IDField = _entities[(int)Enum_UI_SignUp.IDField].GetComponent<TMP_InputField>();
        TMP_InputField PWField = _entities[(int)Enum_UI_SignUp.PWField].GetComponent<TMP_InputField>();
        inputFields.Add(IDField);
        inputFields.Add(PWField);

        foreach (var _subUI in _subUIs)
        {
            _subUI.ClickAction = (PointerEventData data) =>
            {
                GameManager.UI.GetPopupForward(GameManager.UI.SignUp);
            };
        }

        msg = _entities[(int)Enum_UI_SignUp.CheckResult].GetComponentInChildren<TMP_Text>();

        //서버에 회원가입 요청
        _entities[(int)Enum_UI_SignUp.Create].ClickAction = (PointerEventData data) => {
            C_SIGNUP signup_ask_pkt = new C_SIGNUP();
            signup_ask_pkt.SignupId = _entities[(int)Enum_UI_SignUp.IDField].GetComponent<TMP_InputField>().text;
            signup_ask_pkt.SignupPw = CryptoLib.BytesToString(CryptoLib.EncryptSHA256(_entities[(int)Enum_UI_SignUp.PWField].GetComponent<TMP_InputField>().text), encoding: "ascii");

            GameManager.Network.Send(PacketHandler.Instance.SerializePacket(signup_ask_pkt));
        };

        _entities[(int)Enum_UI_SignUp.Cancel].ClickAction = (PointerEventData data) => {
            GameManager.UI.ClosePopup(GameManager.UI.SignUp);
        };

        gameObject.SetActive(false);
    }

    public override void EnterAction()
    {
        base.EnterAction();
        _entities[(int)Enum_UI_SignUp.Create].ClickAction?.Invoke(null);
    }
}
