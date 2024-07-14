using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Google.Protobuf;

public class UI_CharacterCreate : UI_Entity
{
    CHARACTER_INFO character;

    enum Enum_UI_JobSelect
    {
        OptionPanel,
        GoBack,
        Create,
        Settings
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_JobSelect);
    }

    protected override void Init()
    {
        base.Init();
        character = GameManager.Data.CurrentCharacter;

        _entities[(int)Enum_UI_JobSelect.Settings].ClickAction = (PointerEventData data) =>
        {
            GameManager.UI.OpenOrClose(GameManager.UI.Settings);
        };

        // 이름 생성 팝업 띄우기
        _entities[(int)Enum_UI_JobSelect.Create].ClickAction = (PointerEventData data) => {
            GameManager.UI.OpenPopup(GameManager.UI.InputName);
        };

        _entities[(int)Enum_UI_JobSelect.GoBack].ClickAction = (PointerEventData data) => {
            GameManager.Scene.LoadPreviousScene();
        };
    }

    public void SendCharacterPacket()
    {
        C_NEW_CHARACTER new_character_pkt = new C_NEW_CHARACTER();
        new_character_pkt.Character = new CHARACTER_BASE();
        new_character_pkt.Character.Gender = character.BaseInfo.Gender;
        new_character_pkt.Character.CharacterClass = character.BaseInfo.CharacterClass;
        new_character_pkt.Character.Nickname = character.BaseInfo.Nickname;
        new_character_pkt.Character.SlotIndex = GameManager.Data.SelectedSlotNum;
        new_character_pkt.Character.CharacterId = character.BaseInfo.CharacterId; // 기본값 0 전송

        GameManager.Network.Send(PacketHandler.Instance.SerializePacket(new_character_pkt));
    }
}
