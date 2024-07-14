using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class UI_Entity : MonoBehaviour, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, ISelectHandler, IDropHandler
{
    public Action<PointerEventData> PointerEnterAction = null;
    public Action<PointerEventData> PointerUpAction = null;
    public Action<PointerEventData> PointerExitAction = null;
    public Action<PointerEventData> ClickAction = null;
    public Action<PointerEventData> BeginDragAction = null;
    public Action<PointerEventData> DragAction = null;
    public Action<PointerEventData> EndDragAction = null;
    public Action<PointerEventData> DropAction = null;
    public Action<BaseEventData> SelectAction = null;

    //나의 UI_Type
    public Type UIType = null;

    //부모 UI_Entity
    UI_Entity _mother;

    // 자식 팝업
    public List<UI_Entity> childPopups = new List<UI_Entity>();

    //모든 하위 UI 요소들 + 현재 UI로부터 파생된 팝업 UI
    protected List<UI_Entity> _subUIs = new List<UI_Entity>();
    //현재 UI의 하위 UI요소들
    protected Dictionary<int, UI_Entity> _entities = new Dictionary<int, UI_Entity>();
    public virtual void EnterAction() { EventSystem.current.SetSelectedGameObject(null); }
    public virtual void EscAction() { GameManager.UI.ClosePopup(this); }

    int curInputFieldIndex;
    protected List<TMP_InputField> inputFields;
    public void TabAction()
    {
        if (inputFields.Count == 0) return;

        // 현재 focus된 inputField 찾기
        for (int i = 0; i < inputFields.Count; i++)
        {
            if (inputFields[i].isFocused)
            {
                curInputFieldIndex = i;
            }
        }

        curInputFieldIndex++;
        // 마지막 inputField 이후엔 초기 inputField로
        if (curInputFieldIndex > inputFields.Count - 1)
        {
            curInputFieldIndex = 0;
        }
        inputFields[curInputFieldIndex].Select();
    }

    //UI컴포넌트들 모음. 오브젝트에 UI컴포넌트가 여러개 있을 경우, 해당 순서가 유의미함.
    static List<Type> _components = new List<Type>()
    {
        typeof(Button),
        typeof(Slider),
        typeof(Toggle),
        typeof(TMP_InputField),
        typeof(TMP_Dropdown),
        typeof(TMP_Text),
        typeof(Image),
        typeof(RawImage),
    };

    protected void Start()
    {
        if (UIType == null)
            Init();
    }

    //만약 상위UI가 있다면 하위 UI에게 전달해주는 용도
    protected void Mount(UI_Entity mother)
    {
        _mother = mother;   //부모 UI_Entity를 받아옴
    }

    protected virtual void Init()
    {
        var tnames = GetUINamesAsType();
        //이름 목록 가져오기
        string[] names = tnames == null ? null : Enum.GetNames(tnames);

        //자식들을 모두 순회하며 UI_Entity를 탐색하거나, UI_Entity를 붙일지 판별
        for (int i = 0; i < transform.childCount; i++)
        {
            var go = transform.GetChild(i).gameObject;
            var comp = go.GetComponent<UI_Entity>();

            //만약 UI_Entity가 아니라면 붙일지 말지 판별 후 부착
            if (comp == null)
                comp = Distinguisher(go, ref names);

            //UI_Entity 대상이 아니라면 생략
            if (comp == null) continue;

            // UI요소라면 다음 코드가 실행됨
            comp.Mount(this);
            _subUIs.Add(comp);
        }
    }

    public UI_Entity Distinguisher(GameObject obj, ref string[] names)
    {
        if (obj == null) return null;

        for (int i = 0; i < _components.Count; i++)
        {
            var component = obj.GetComponent(_components[i]);

            if (component == null)
                continue;

            var uientity = component.gameObject.GetOrAddComponent<UI_SubEntity>();
            uientity.UIType = _components[i];

            if (names == null)
                return uientity;

            for (int str = 0; str < names.Length; str++)
            {
                // 하이어라키 상의 이름과 enum 안의 이름이 일치하면,
                if (component.gameObject.name == names[str])
                {
                    if (!_entities.ContainsKey(str))  // 키가 이미 존재하는지 확인
                    {                        
                        _entities.Add(str, uientity);
                    }
                    break;
                }
            }

            return uientity;
        }

        return null;
    }

    //오류검증 하지않음. 받아서 null체크
    public T GetComp<T>() where T : Component
    {
        return GetComponent<T>();
    }

    public void CloseAllUI()
    {
        if (_mother != null)
            _mother.CloseAllUI();
        else
            GameManager.Resources.Destroy(gameObject);
    }

    public virtual void PopupOnEnable() { }
    public virtual void PopupOnDisable() { }

    protected abstract Type GetUINamesAsType();

    // 포인터가 오브젝트에 들어왔을 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterAction?.Invoke(eventData);
    }

    // 포인터가 올려져 있을때 호출
    public void OnPointerUp(PointerEventData eventData)
    {
        PointerUpAction?.Invoke(eventData);
    }

    // 포인터가 오브젝트 밖으로 나갈 때 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitAction?.Invoke(eventData);
    }

    // 오브젝트에서 포인터를 누르고 동일한 오브젝트에서 뗄 때 호출
    public void OnPointerClick(PointerEventData eventData)
    {
        ClickAction?.Invoke(eventData);
    }

    // 드래그 시작되면 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragAction?.Invoke(eventData);
    }

    // 드래그 대상이 드래그되는 동안 호출
    public void OnDrag(PointerEventData eventData)
    {
        DragAction?.Invoke(eventData);
    }

    // 드래그 끝나면 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragAction?.Invoke(eventData);
    }

    // 오브젝트 내부에 드랍을 했을때 1회 호출
    public void OnDrop(PointerEventData eventData)
    {
        DropAction?.Invoke(eventData);
    }

    // 오브젝트가 선택된 순간, 그 오브젝트에서 호출
    public void OnSelect(BaseEventData eventData)
    {
        SelectAction?.Invoke(eventData);
    }
}