using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Google.Protobuf;

public class UI_CharacterOptions : UI_Entity
{
    CHARACTER_INFO character;
    Image classImage;
    TMP_Text classDesc;

    enum Enum_UI_JobSelect
    {
        Background,
        Class,
        Gender,
        ClassImage,
        ClassDescription,
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_JobSelect);
    }

    protected override void Init()
    {
        base.Init();
        GameManager.Data.CurrentCharacter = new CHARACTER_INFO();
        character = GameManager.Data.CurrentCharacter;

        classImage = _entities[(int)Enum_UI_JobSelect.ClassImage].GetComponent<Image>();
        classDesc = _entities[(int)Enum_UI_JobSelect.ClassDescription].GetComponentInChildren<TMP_Text>();
        _SetDefaultInfo();
    }

    void _SetDefaultInfo()
    {
        character.BaseInfo = new CHARACTER_BASE();
        character.BaseInfo.CharacterClass = 0;
        character.BaseInfo.Gender = true;

        character.Stat = new CHARACTER_STATUS();
        character.Pos = new VECTOR3();

        SwitchImageAndDescription(Enum_Class.Warrior);
    }

    public void SwitchImageAndDescription(Enum_Class className)
    {
        // 설명란 변경
        switch (className)
        {
            case Enum_Class.Warrior:
                classDesc.text = $"전사는 큰 방어력과 체력을 가지고 있습니다.";
                break;
            case Enum_Class.Wizard:
                classDesc.text = $"마법사는 적에게 큰 데미지를 줄 수 있거나 팀을 치유할 수 있습니다.";
                break;
            case Enum_Class.Archer:
                classDesc.text = $"궁수는 장거리에서도 치명적인 데미지를 줄 수 있습니다.";
                break;
            /*case Enum_Class.Default:
                classDesc.text = $"디폴트";
                break;*/
            default:
                break;
        }

        // 이미지 변경
        string strClassName = Enum.GetName(typeof(Enum_Class), className);
        classImage.sprite = GameManager.Resources.Load<Sprite>($"Materials/JobImage/{strClassName}");
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
