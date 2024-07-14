using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UI_Setting : UI_Entity
{
    // ---------왼쪽 패널--------
    int settingTypesCount;
    Toggle[] settingTypeToggles;
    ToggleGroup toggleGroup;

    // ---------오른쪽 패널------
    // 볼륨설정
    int volSlidersCount;
    Slider[] volSliders;
    // ------------------------

    // 드래그 Field
    Vector2 _UIPos;
    Vector2 _dragBeginPos;
    Vector2 _offset;

    enum Enum_UI_Settings
    {
        Panel,
        Interact,
        Panel_L,
        Panel_R,
        Close,
        Reset,
        Accept,
        Cancel
    }

    enum Enum_SettingTypes
    {
        오디오,
        게임플레이,
        키세팅
    }

    enum Enum_SliderTypes
    {
        전체음량,
        배경음악,
        효과음,
        음성
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_Settings);
    }

    protected override void Init()
    {
        base.Init();

        settingTypesCount = Enum.GetValues(typeof(Enum_SettingTypes)).Length;
        volSlidersCount = Enum.GetValues(typeof(Enum_SliderTypes)).Length;
        toggleGroup = _entities[(int)Enum_UI_Settings.Panel_L].gameObject.GetComponent<ToggleGroup>();

        _SetPanel_L();
        _SetVolOptions();
        GameManager.Sound.SliderSetting(volSliders[0], volSliders[1], volSliders[2], volSliders[3]);

        foreach (var _subUI in _subUIs)
        {
            _subUI.ClickAction = (PointerEventData data) =>
            {
                GameManager.UI.GetPopupForward(GameManager.UI.Settings);
            };
        }
        // 버튼 기능 할당      
        _entities[(int)Enum_UI_Settings.Close].ClickAction = (PointerEventData data) =>
        {
            GameManager.UI.ClosePopup(GameManager.UI.Settings);
        };
        _entities[(int)Enum_UI_Settings.Reset].ClickAction = (PointerEventData data) =>
        {
            _ResetVolOptions(0.5f);
        };
        _entities[(int)Enum_UI_Settings.Accept].ClickAction = (PointerEventData data) =>
        {
            GameManager.UI.ClosePopup(GameManager.UI.Settings);
        };
        _entities[(int)Enum_UI_Settings.Cancel].ClickAction = (PointerEventData data) =>
        {
            GameManager.UI.ClosePopup(GameManager.UI.Settings);
        };

        // 팝업 드래그
        _entities[(int)Enum_UI_Settings.Interact].BeginDragAction = (PointerEventData data) =>
        {
            _UIPos = transform.position;
            _dragBeginPos = data.position;
        };

        _entities[(int)Enum_UI_Settings.Interact].DragAction = (PointerEventData data) =>
        {
            _offset = data.position - _dragBeginPos;
            transform.position = _UIPos + _offset;
        };

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 왼쪽 패널에 있는 토글 선택에 따라 해당되는 내용을 Panel_R 에 활성화
    /// ex. Panel_L 볼륨 설정 -> Panel_R 볼륨 설정 옵션, Panel_L 게임 키 -> Panel_R 키설정
    /// </summary>
    void _SetPanel_L()
    {
        settingTypeToggles = new Toggle[settingTypesCount];
        for (int i = 0; i < settingTypesCount; i++)
        {
            GameObject settingsLeftToggle = GameManager.Resources.Instantiate("Prefabs/UI/Scene/SettingsLeftToggle", _entities[(int)Enum_UI_Settings.Panel_L].transform);
            TMP_Text togName = settingsLeftToggle.GetComponentInChildren<TMP_Text>();
            switch (i)
            {
                case 0:
                    togName.text = Enum.GetName(typeof(Enum_SettingTypes), 0);
                    break;
                case 1:
                    togName.text = Enum.GetName(typeof(Enum_SettingTypes), 1);
                    break;
                case 2:
                    togName.text = Enum.GetName(typeof(Enum_SettingTypes), 2);
                    break;
                default:
                    break;
            }

            int index = i;
            settingTypeToggles[i] = settingsLeftToggle.GetComponent<Toggle>();
            settingTypeToggles[i].group = toggleGroup;
            settingTypeToggles[i].onValueChanged.AddListener((value) => _SettingTypeChanged(index));
        }

        settingTypeToggles[0].isOn = true; // 첫번째 항목 선택
    }
    void _SettingTypeChanged(int toggleIndex)
    {
        bool isToggleOn = settingTypeToggles[toggleIndex].isOn;
        Transform childObject = _entities[(int)Enum_UI_Settings.Panel_R].transform.GetChild(toggleIndex);
        childObject.gameObject.SetActive(isToggleOn);
    }

    /// <summary>
    /// 볼륨 패널 오브젝트 생성과 동시에 텍스트 변경 + 저장된 볼륨값 가져오기
    /// </summary>
    void _SetVolOptions()
    {
        volSliders = new Slider[volSlidersCount];
        for (int i = 0; i < volSlidersCount; i++)
        {
            GameObject volume = GameManager.Resources.Instantiate("Prefabs/UI/Scene/Volume", _entities[(int)Enum_UI_Settings.Panel_R].transform.GetChild(0).transform);
            volSliders[i] = volume.GetComponentInChildren<Slider>(); // 볼륨 슬라이더
            Toggle onOffToggle = volume.GetComponentInChildren<Toggle>();

            TMP_Text[] texts = volume.GetComponentsInChildren<TMP_Text>();
            TMP_Text volType = texts[0];
            TMP_Text volValue = texts[1];
            TMP_Text volOn = texts[2];

            // 저장된 볼륨 설정 + 텍스트 변경
            switch (i)
            {
                case 0:
                    volType.text = Enum.GetName(typeof(Enum_SliderTypes), 0);
                    volSliders[i].value = GameManager.Data.volOptions.MasterVol;
                    onOffToggle.onValueChanged.AddListener((value) => GameManager.Sound.MuteOnOff(SoundManager.Enum_SoundSlider.Master, value));
                    break;
                case 1:
                    volType.text = Enum.GetName(typeof(Enum_SliderTypes), 1);
                    volSliders[i].value = GameManager.Data.volOptions.BgmVol;
                    onOffToggle.onValueChanged.AddListener((value) => GameManager.Sound.MuteOnOff(SoundManager.Enum_SoundSlider.BGM, value));
                    break;
                case 2:
                    volType.text = Enum.GetName(typeof(Enum_SliderTypes), 2);
                    volSliders[i].value = GameManager.Data.volOptions.EffectVol;
                    onOffToggle.onValueChanged.AddListener((value) => GameManager.Sound.MuteOnOff(SoundManager.Enum_SoundSlider.Effect, value));
                    break;
                case 3:
                    volType.text = Enum.GetName(typeof(Enum_SliderTypes), 3);
                    volSliders[i].value = GameManager.Data.volOptions.VoiceVol;
                    onOffToggle.onValueChanged.AddListener((value) => GameManager.Sound.MuteOnOff(SoundManager.Enum_SoundSlider.Voice, value));
                    break;
                default:
                    break;
            }
            volValue.text = $"{Mathf.Floor(volSliders[i].value * 100)} %"; // 초기 텍스트 설정
            volSliders[i].onValueChanged.AddListener((value) => {
                volValue.text = $"{Mathf.Floor(value * 100)} %";
            });
            volOn.text = $"{volType.text} 켜기";
        }
    }

    // 볼륨 초기화
    void _ResetVolOptions(float value)
    {
        foreach (var slider in volSliders)
        {
            slider.value = value;
        }
    }
}
