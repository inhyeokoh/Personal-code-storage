using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_SkillKeySlot : UI_Entity
{

    bool skillChange;


    // 현재 키슬롯에 저장되있는 스킬
    public Skill skill;
    public CoolTimeCheck coolTimeCheck;
        
    [SerializeField]
    KeySlotUISetting keySlotUISetting;

    Image skillIcon;
    RectTransform keySlotImageRect;

    // 드랍앤 드래그를 했을때 위치를 변경시키기 위한 트랜스폼
    Transform canvas;
    Transform previousParent;


    [SerializeField]
    SelectSkillMove MoveSkillObject;

    public Image SkillIcon { get { return skillIcon; } set { skillIcon = value; } }
    enum Enum_UI_SKillKeySlot
    {
        SkillIcon = 0,
        CoolTime,
        Fill,
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_SKillKeySlot);
    }


    private void Update()
    {
       // print($"현재 스킬 키슬롯 위치{imageRect.position}");
    }

    protected override void Init()
    {
        base.Init();

        skillIcon = _entities[(int)Enum_UI_SKillKeySlot.SkillIcon].GetComponent<Image>();
        keySlotImageRect = _entities[(int)Enum_UI_SKillKeySlot.SkillIcon].GetComponent<RectTransform>();

        canvas = gameObject.transform.root;
        previousParent = transform.parent;
        /*keySlotUISetting =*/



        // 현재 스킬 정보가 닿았을때
        _entities[(int)Enum_UI_SKillKeySlot.SkillIcon].DropAction = (PointerEventData data) =>
        {
            if (data.pointerDrag != null)
            {
              //  print("발생함");
            }
            else
            {
                //print("발생 안함");
            }
       
            // 엑티브 스킬이랑 패시브 스킬이면
            if (data.pointerDrag.gameObject.tag == "ActiveSkill" || data.pointerDrag.gameObject.tag == "PassiveSkill")
            {
               // print(data.pointerDrag.gameObject.name);
                // 아까 닿았던 스킬 정보를 가져오고
                skill = data.pointerDrag.transform.parent.GetComponent<UI_SkillUISlot>().SkillReturn();
                coolTimeCheck.CoolTimeChanage(skill);

                keySlotUISetting.KeySlotCheck(this, skill);
            }  
            
            if (data.pointerDrag.gameObject.tag == "KeySlotSkill")
            {
                UI_SkillKeySlot dataUIKeySlot = data.pointerDrag.transform.parent.GetComponent<UI_SkillKeySlot>();
                //if (dataUIKeySlot.skill == null) return;

                if (skill == null)
                {
                    keySlotUISetting.KeySlotNullChanage(this, dataUIKeySlot);
                   
                    MoveSkillObject.GetComponent<Image>().raycastTarget = true;
                    MoveSkillObject.gameObject.SetActive(false);
                }
                else
                {
                    keySlotUISetting.KeySlotChanage(this, dataUIKeySlot);                   
                }

                skillChange = true;
            }
        };
        _entities[(int)Enum_UI_SKillKeySlot.SkillIcon].BeginDragAction = (PointerEventData data) =>
        {
            if (skill == null)
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
        _entities[(int)Enum_UI_SKillKeySlot.SkillIcon].DragAction = (PointerEventData data) =>
        {
            if (skill == null)
            {
                data.pointerDrag = null;
                return;
            }

            MoveSkillObject.gameObject.transform.position = data.position;
        };

        _entities[(int)Enum_UI_SKillKeySlot.SkillIcon].EndDragAction = (PointerEventData data) =>
        {
            if (skill == null)
            {
                data.pointerDrag = null;
                return;
            }

           /* if (data.pointerDrag)*/

            MoveSkillObject.gameObject.transform.SetParent(previousParent);
            MoveSkillObject.transform.SetAsLastSibling();
            MoveSkillObject.GetComponent<Image>().raycastTarget = true;
            MoveSkillObject.gameObject.SetActive(false);

            if (data.pointerCurrentRaycast.gameObject == null || 
            data.pointerCurrentRaycast.gameObject.tag != "KeySlotSkill")
            {
                if (MoveSkillObject.transform.position.x > (keySlotImageRect.position.x + 96f)        
                || MoveSkillObject.transform.position.x < (keySlotImageRect.position.x - 96f)        
                || MoveSkillObject.transform.position.y > (keySlotImageRect.position.y + 52.50f)         
                || MoveSkillObject.transform.position.y < (keySlotImageRect.position.y + -52.50f))
                {
                    keySlotUISetting.KeySlotSkillReset(skill);
                }
            }         
        };
          
    }


    public void QuickSlot(Skill skill, CharacterState playerState, CharacterStatus playerStat)
    {
        if (skill == null)
        {
            playerState.ChangeState((int)Enum_CharacterState.Idle);
            //print("스킬이 없습니다.");
            return;
        }
        else
        {
            if (playerState.SkillUseCheck)
            {
                print("스킬 사용중 입니다.");
            }
            else if (skill.CoolTime > 0)
            {
                print("스킬이 아직 쿨타임 입니다.");
            }
            else if (playerStat.MP < skill.SkillMP)
            {
                print("마나가 부족합니다.");
            }
            else
            {
                playerStat.MP -= skill.SkillMP;
                PlayerController.instance._effector.EffectBurstStop();
                skill.Use();
            }
        }
    }
    public void Use(CharacterState playerState, CharacterStatus playerStat)
    {
        QuickSlot(skill, playerState, playerStat);
    }

}
