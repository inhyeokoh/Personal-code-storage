using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UI_ItemToolTip : UI_Entity
{
    Vector2 _descrUISize;
    GameObject _descrPanel;
    TMP_Text _itemNameText;
    Image _itemImage;
    TMP_Text _descrText;

    enum Enum_UI_ItemToolTip
    {
        Panel,
        Name,
        ItemImage,
        Description
    }

    protected override Type GetUINamesAsType()
    {
        return typeof(Enum_UI_ItemToolTip);
    }

    protected override void Init()
    {
        base.Init();
        #region 초기설정 및 캐싱
        _descrPanel = _entities[(int)Enum_UI_ItemToolTip.Panel].gameObject;
        _itemNameText = _entities[(int)Enum_UI_ItemToolTip.Name].transform.GetChild(0).GetComponent<TMP_Text>();
        _itemImage = _entities[(int)Enum_UI_ItemToolTip.ItemImage].GetComponent<Image>();
        _descrText = _entities[(int)Enum_UI_ItemToolTip.Description].transform.GetChild(0).GetComponent<TMP_Text>();
        _descrUISize = _GetUISize(_descrPanel);
        #endregion

        gameObject.SetActive(false);
    }

    /// <summary>
    /// UI 사각형 좌표의 좌측하단과 우측상단 좌표를 전역 좌표로 바꿔서 UI 사이즈 계산.
    /// </summary>
    Vector2 _GetUISize(GameObject UI)
    {
        Vector2 leftBottom = UI.transform.TransformPoint(UI.GetComponent<RectTransform>().rect.min);
        Vector2 rightTop = UI.transform.TransformPoint(UI.GetComponent<RectTransform>().rect.max);
        Vector2 UISize = rightTop - leftBottom;
        return UISize;
    }

    public void ShowItemInfo(ItemData item)
    {
        if (item == null) return;
        gameObject.SetActive(true);

        // 위치 조정
        Vector2 pointerPos = Pointer.current.position.ReadValue();
        Vector2 descrPos = pointerPos + new Vector2(_descrUISize.x / 2, -_descrUISize.y / 2);
        float x = Math.Clamp(descrPos.x, _descrUISize.x / 2, Screen.width - (_descrUISize.x / 2));
        float y = Math.Clamp(descrPos.y, _descrUISize.y / 2, Screen.height - (_descrUISize.y / 2));
        transform.position = new Vector2(x, y);

        _itemNameText.text = item.name; // 아이템 이름
        _itemImage.sprite = item.icon; // 아이템 이미지

        // 아이템 설명
        if (item.itemType == Enum_ItemType.Equipment) // 장비아이템은 유효한 스탯만 표기
        {
            if (GameManager.Data.itemDatas[item.id] is StateItemData itemData)
            {
                int[] stats = { itemData.level, itemData.attack, itemData.defense, itemData.speed, itemData.attackSpeed, itemData.maxHp, itemData.maxMp };
                string descLines = string.Format(item.desc, $"{itemData.level}\n", $"{itemData.attack}\n", $"{itemData.defense}\n",
                    $"{itemData.speed}\n", $"{itemData.attackSpeed}\n", $"{itemData.maxHp}\n", $"{itemData.maxMp}\n");
                string[] lines = descLines.Split("\n");
                string desc = $"{lines[0]} \n";
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    if (stats[i] == 0) continue;
                    desc += $"{lines[i]} \n";
                }

                _descrText.text = desc;
            }
        }
        else
        {
            _descrText.text = item.desc;
        }
    }
}
