using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UI_InGameMain : UI_Entity
{
    //컴포넌트들이 필요한 오브젝트들

    Slider HPSbr;
    Slider MPSbr;
    TMP_Text HPText;
    TMP_Text MPText;

    // 현재 플레이어 정보
    CharacterStatus Player;



    enum Enum_UI_ingameMain
    {
        InGamePanel = 0,
        HPSliderBar,
        MPSliderBar,
        ButtonQuest,
        ButtonChest,
        ButtonText,
        ButtonMenu,
        ButtonOptions,
        ButtonMagic,
        SkillKeySlot,
        SkillKeySlot2,
        SkillKeySlot3,
        SkillKeySlot4,
    }
    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_ingameMain);
    }


    /// <summary>
    ///  현재 자식들의 컴포넌트들을 가져옴
    /// </summary>
    protected override void Init()
    {
        base.Init();

        HPSbr = _entities[(int)Enum_UI_ingameMain.HPSliderBar].GetComponent<Slider>();
        MPSbr = _entities[(int)Enum_UI_ingameMain.MPSliderBar].GetComponent<Slider>();
           
        HPSbr = _entities[(int)Enum_UI_ingameMain.HPSliderBar].GetComponent<Slider>();
        MPSbr = _entities[(int)Enum_UI_ingameMain.MPSliderBar].GetComponent<Slider>();

        HPText = HPSbr.GetComponentInChildren<TMP_Text>();
        MPText = MPSbr.GetComponentInChildren<TMP_Text>();      
    }

    /// <summary>
    /// 현재 플레이어를 탐색
    /// </summary>
    
    public void PlayerCheck()
    {
        Player = PlayerController.instance._playerStat;
    }


    public void HPCheck()
    {

    }
    
    public void MPCheck()
    {

    }   
}

