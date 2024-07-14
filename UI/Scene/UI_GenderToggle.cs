using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_GenderToggle : MonoBehaviour
{
    public enum Enum_Gender
    {
        남자,
        여자
    }

    [SerializeField]
    public Enum_Gender gender;

    private void Awake()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    void OnToggleValueChanged(bool isOn)
    {
        bool b_gender = true;
        if (isOn)
        {
            switch (gender)
            {
                case Enum_Gender.남자:
                    b_gender = true;
                    break;
                case Enum_Gender.여자:
                    b_gender = false;
                    break;
            }
            GameManager.Data.CurrentCharacter.BaseInfo.Gender = b_gender;
        }
    }
}
