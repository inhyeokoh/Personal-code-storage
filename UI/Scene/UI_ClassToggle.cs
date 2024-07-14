using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ClassToggle : MonoBehaviour
{
    UI_CharacterOptions characterOptions;

    [SerializeField]
    public Enum_Class className;

    private void Awake()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
        characterOptions = GetComponentInParent<UI_CharacterOptions>();
    }

    void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            GameManager.Data.CurrentCharacter.BaseInfo.CharacterClass = (int)className;
            characterOptions.SwitchImageAndDescription(className);
        }
    }
}