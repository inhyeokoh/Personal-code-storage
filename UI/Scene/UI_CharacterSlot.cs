using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_CharacterSlot : UI_Entity
{
    public int Index { get; set; }

    CHARACTER_INFO character;
    Toggle toggle;

    enum Enum_UI_Slot
    {
        Background,
        Label,
        Image,
        MainText,
        Create
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_Slot);
    }

    protected override void Init()
    {
        base.Init();

        toggle = GetComponent<Toggle>();
        character = GameManager.Data.characters[Index];
        LoadCharacter();
    }

    public void LoadCharacter()
    {
        if (character != null) // 캐릭터 정보 존재시
        {
            string gender = character.BaseInfo.Gender ? "Men" : "Women";
            string job = Enum.GetName(typeof(Enum_Class), character.BaseInfo.CharacterClass);

            // 직업에 맞는 이미지 로드
            Image image = _entities[(int)Enum_UI_Slot.Image].GetComponent<Image>();
            image.sprite = GameManager.Resources.Load<Sprite>($"Materials/JobImage/{job}");

            // 캐릭터 간략 정보 표시
            _entities[(int)Enum_UI_Slot.MainText].GetComponent<TMP_Text>().text =
                $"캐릭터명 : {character.BaseInfo.Nickname.ToString(System.Text.Encoding.Unicode)}\n 레벨: {character.Stat.Level}\n 직업: {job}\n" +
                $"성별: {gender}\n 체력: {character.Stat.Hp}/{ character.Stat.MaxHP}\n 공격력: {character.Stat.Attack}\n 이동속도: {character.Stat.Speed}\n ";

            _entities[(int)Enum_UI_Slot.Create].gameObject.SetActive(false); // 캐릭터 생성 버튼 비활성화
            toggle.isOn = false;
            toggle.group = transform.parent.GetComponent<ToggleGroup>();
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
        else
        {
            _entities[(int)Enum_UI_Slot.Image].gameObject.SetActive(false);
            _entities[(int)Enum_UI_Slot.MainText].gameObject.SetActive(false);
            toggle.interactable = false;

            // 캐릭터 생성버튼에 기능 부여
            _entities[(int)Enum_UI_Slot.Create].ClickAction = (PointerEventData data) =>
            {
                GameManager.Data.SelectedSlotNum = Index;
                SceneManager.LoadScene("Create");
            };
        }
    }

    void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            GameManager.Data.SelectedSlotNum = Index;
        }
    }
}
