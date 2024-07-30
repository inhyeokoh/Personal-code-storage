using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 확인,취소 버튼이 있는 팝업
/// </summary>
public class UI_ConfirmYN : UI_Entity
{
    bool _init;
    bool _useBlocker = true;
    TMP_Text _mainText;
    Enum_ConfirmTypes confirmType = Enum_ConfirmTypes.AskDecidingNickName;
    public enum Enum_ConfirmTypes
    {
        AskDecidingNickName,
        AskDeleteCharacter
    }

    enum Enum_UI_ConfirmYN
    {
        Panel,
        Interact,
        MainText,
        Accept,
        Cancel
    }


    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_ConfirmYN);
    }

    public override void PopupOnEnable()
    {
        if (!_useBlocker) return;

        GameManager.UI.UseBlocker(true);
    }

    public override void PopupOnDisable()
    {
        GameManager.UI.UseBlocker(false);
    }

    protected override void Init()
    {
        base.Init();

        _mainText = _entities[(int)Enum_UI_ConfirmYN.MainText].GetComponent<TMP_Text>();
        _mainText.text = "Default";

        _entities[(int)Enum_UI_ConfirmYN.Accept].ClickAction = (PointerEventData data) => {
            switch (confirmType)
            {
                case Enum_ConfirmTypes.AskDecidingNickName:
#if SERVER || DEBUG_MODE
                    GameObject.Find("CharacterCreate").GetComponent<UI_CharacterCreate>().SendCharacterPacket();
#elif CLIENT_TEST_TITLE
                    CHARACTER_INFO newChar = new CHARACTER_INFO();
                    newChar.BaseInfo = new CHARACTER_BASE();
                    newChar.Stat = new CHARACTER_STATUS();
                    newChar.Vector3 = new VECTOR3();
                    GameManager.Data.characters[GameManager.Data.SelectedSlotNum] = newChar;
                    SceneManager.LoadSceneAsync("Select");
#endif
                    GameManager.UI.ClosePopupAndChildren(GameManager.UI.InputName);
                    break;
                case Enum_ConfirmTypes.AskDeleteCharacter:
                    // 서버에 삭제할 캐릭터 id(번호) 전송
                    C_DELETE_CHARACTER character_delete_pkt = new C_DELETE_CHARACTER();
                    character_delete_pkt.CharacterId = GameManager.Data.CurrentCharacter.BaseInfo.CharacterId;
                    character_delete_pkt.SlotNum = GameManager.Data.SelectedSlotNum;
                    GameManager.Network.Send(PacketHandler.Instance.SerializePacket(character_delete_pkt));

                    GameManager.UI.ClosePopup(GameManager.UI.ConfirmYN);
                    break;
                default:
                    break;
            }
        };

        _entities[(int)Enum_UI_ConfirmYN.Cancel].ClickAction = (PointerEventData data) => {
            GameManager.UI.ClosePopup(GameManager.UI.ConfirmYN);
        };

        gameObject.SetActive(false);
    }

    public void ChangeText(Enum_ConfirmTypes type)
    {
        confirmType = type;

        switch (confirmType)
        {
            case Enum_ConfirmTypes.AskDecidingNickName:
                _mainText.text = $"해당 이름으로 결정하시겠습니까?\n 캐릭터명 : {GameManager.Data.CurrentCharacter.BaseInfo.Nickname.ToString(System.Text.Encoding.Unicode)}";
                break;
            case Enum_ConfirmTypes.AskDeleteCharacter:
                _mainText.text = $"{GameManager.Data.CurrentCharacter.BaseInfo.Nickname.ToString(System.Text.Encoding.Unicode)} 캐릭터를 정말 삭제하시겠습니까?";
                break;
            default:
                break;
        }
    }

    public override void EnterAction()
    {
        base.EnterAction();
        _entities[(int)Enum_UI_ConfirmYN.Accept].ClickAction?.Invoke(null);
    }
}
