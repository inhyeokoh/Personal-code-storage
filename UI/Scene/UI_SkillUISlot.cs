using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 현재 스킬창안에 있는 스킬슬롯들
/// </summary>
public class UI_SkillUISlot : UI_Entity
{

    // 현재 스킬슬롯 UI에 필요한 오브젝트들
    UI_Entity IconObject;
    UI_SkillWindow SkillUIObject;
    TMP_Text[] skillText;
   
    
    SelectSkillMove MoveSkillObject;

    
    // 드랍앤 드래그를 했을때 위치를 변경시키기 위한 트랜스폼
    Transform canvas;
    Transform previousParent;


   
    // 현재 슬롯안에 있는 스킬
    Skill skill;

    enum Enum_UI_SkillUISolt
    {
        Panel = 0,
        SkillIcon,      
    }
    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_SkillUISolt);
    }

    private void Update()
    {
       // print($"현재 스킬 아이콘 위치 {imageRect.position}");
    }


    protected override void Init()
    {

        // 현재 스킬을 배워도 키슬롯에는 스킬이 달리지만 원래 스킬슬롯에 있던 이미지는 돌아와야 하기 떄문에
        // 원래 스킬슬롯안에 있던 포지션을 저장하는 기능

        base.Init();
        IconObject = _entities[(int)Enum_UI_SkillUISolt.SkillIcon];

        IconObject.GetComponent<Image>().sprite = skill.Icon;
        SkillUIObject = transform.parent.parent.parent.parent.GetComponent<UI_SkillWindow>();
        skillText = _entities[(int)Enum_UI_SkillUISolt.Panel].GetComponentsInChildren<TMP_Text>();
        skillText[0].text = skill.SkillName;

        MoveSkillObject = transform.parent.parent.parent.parent.GetChild((int)Enum_UI_SkillWindow.SelctSkillObject).GetComponent<SelectSkillMove>();
        canvas = gameObject.transform.root;
        previousParent = transform.parent.parent.parent.parent;
            
        // 현재 스킬 레벨 인포
        SkillLevelInfo();


        print(previousParent.name);
        // 스킬슬롯을 눌렀을때 스킬창에 현재 스킬 정보들을 보내줌
        _entities[(int)Enum_UI_SkillUISolt.Panel].ClickAction = (PointerEventData data) =>
        {
            SkillUIObject.SelctSkill(skill);
            SelectSkillText(this);
        };

        _entities[(int)Enum_UI_SkillUISolt.SkillIcon].BeginDragAction = (PointerEventData data) =>
        {
            if (skill.Level == 0 || skill.SkillType == Enum_SkillType.PassiveSkill)
            {
                data.pointerEnter = null;
                return;
            }
        
            MoveSkillObject.gameObject.transform.SetParent(canvas);
            MoveSkillObject.transform.SetAsLastSibling();
            MoveSkillObject.gameObject.SetActive(true);
            MoveSkillObject.SkillClick(skill);
            MoveSkillObject.GetComponent<Image>().raycastTarget = false;

        };
        _entities[(int)Enum_UI_SkillUISolt.SkillIcon].DragAction = (PointerEventData data) =>
        {            
            if (skill.Level == 0 || skill.SkillType == Enum_SkillType.PassiveSkill)
            {
                data.pointerDrag = null;
                return;
            }
            MoveSkillObject.gameObject.transform.position = data.position;        

        };
        _entities[(int)Enum_UI_SkillUISolt.SkillIcon].EndDragAction = (PointerEventData data) =>
        {
            if (skill.Level == 0 || skill.SkillType == Enum_SkillType.PassiveSkill)
            {
                data.pointerDrag = null;
                return;
            }
            MoveSkillObject.gameObject.transform.SetParent(previousParent);
            MoveSkillObject.transform.SetAsLastSibling();
            MoveSkillObject.GetComponent<Image>().raycastTarget = true;
            MoveSkillObject.gameObject.SetActive(false);
    
        };
    }

    // 현재 스킬 레벨 갱신
    public void SkillLevelInfo()
    {
        skillText[1].text = $"현재 레벨 : {skill.Level}";
    }

    // 스킬이 레벨업을 하였으면 정보를 갱신하기위해 스킬 스텟 정보들을 보내주는 메서드
    public void SelectSkillText(UI_SkillUISlot texts)
    {
        SkillUIObject.SelectSkillUIInfoMationText(texts);
        return;
    }
    public void CurrentSkill(Skill skill)
    {
        this.skill = skill;
    }

    public Skill SkillReturn()
    {
        return skill;
    }



                                   





}
