using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UI_Login : UI_Entity
{
    enum Enum_UI_Logins
    {
        Panel,
        IDField,
        PWField,        
        Login,
        SignUp,
        Quit
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_Logins);
    }

    protected override void Init()
    {
        base.Init();

        inputFields = new List<TMP_InputField>();
        TMP_InputField IDField = _entities[(int)Enum_UI_Logins.IDField].GetComponent<TMP_InputField>();
        TMP_InputField PWField = _entities[(int)Enum_UI_Logins.PWField].GetComponent<TMP_InputField>();
        inputFields.Add(IDField);
        inputFields.Add(PWField);

        _entities[(int)Enum_UI_Logins.SignUp].ClickAction = (PointerEventData data) => {
            GameManager.UI.OpenOrClose(GameManager.UI.SignUp);
        };

        _entities[(int)Enum_UI_Logins.Login].ClickAction = (PointerEventData data) => {
#if SERVER
            C_LOGIN login_ask_pkt = new C_LOGIN();
            login_ask_pkt.LoginId = _entities[(int)Enum_UI_Logins.IDField].GetComponent<TMP_InputField>().text;
            login_ask_pkt.LoginPw = CryptoLib.BytesToString(CryptoLib.EncryptSHA256(_entities[(int)Enum_UI_Logins.PWField].GetComponent<TMP_InputField>().text), encoding:"ascii");

            GameManager.Network.Send(PacketHandler.Instance.SerializePacket(login_ask_pkt));
#elif DEBUG_MODE
            C_LOGIN login_ask_pkt = new C_LOGIN();
            login_ask_pkt.LoginId = "asdf3";
            login_ask_pkt.LoginPw = CryptoLib.BytesToString(CryptoLib.EncryptSHA256("1234"), encoding: "ascii");
            GameManager.Network.Send(PacketHandler.Instance.SerializePacket(login_ask_pkt));
#elif CLIENT_TEST_TITLE
            GameManager.ThreadPool.UniAsyncJob(() =>
            {
                var loadAsync = SceneManager.LoadSceneAsync("Select");
                GameManager.ThreadPool.UniAsyncLoopJob(() => { return loadAsync.progress < 0.9f; });
            });
#endif
        };       

        _entities[(int)Enum_UI_Logins.Quit].ClickAction = (PointerEventData data) => {
            GameManager.Scene.ExitGame();
        };

#if DEBUG_MODE
        _entities[(int)Enum_UI_Logins.Login].ClickAction?.Invoke(null);
#endif
    }

    public override void EnterAction()
    {
        base.EnterAction();
        _entities[(int)Enum_UI_Logins.Login].ClickAction?.Invoke(null);
    }

    public override void EscAction()
    {        
    }
}
