using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using System.Collections;

public class CharacterFollowCamera : MonoBehaviour
{
    public CinemachineVirtualCamera vCam;
    public VCamSwitcher vCamSwitcher;

    Transform player;
    public int _zoomLevel = 0; // 0: 기본줌, 1: 확대줌, 2: 최대확대줌
    float[] _zoomFOV = { 40f, 15f, 8.5f }; // 각 줌 레벨에 따른 FOV
    Vector3[] _followOffsets = {
        new Vector3(0, 25f, -22f),
        new Vector3(0, 6.8f, -23),
        new Vector3(0, 7f, -25f)
    };
    [SerializeField]
    InputActionReference mouseScrollAction;
    CinemachineTransposer cmTransposer;
    CinemachineComposer cmComposer;

    float _targetFOV;
    Vector3 targetFollowOffset;
    float _zoomSpeed = 2f; // 줌 전환 속도

    void Start()
    {
        cmTransposer = vCam.GetCinemachineComponent<CinemachineTransposer>();
        cmComposer = vCam.GetCinemachineComponent<CinemachineComposer>();
        cmComposer.m_TrackedObjectOffset.y = 2.5f;
        player = GameObject.FindWithTag("Player").transform;

        if (vCam != null)
        {
            vCam.Follow = player;
            vCam.LookAt = player;
        }

        _targetFOV = _zoomFOV[_zoomLevel];
        targetFollowOffset = _followOffsets[_zoomLevel];
    }

    void OnEnable()
    {
        mouseScrollAction.action.Enable();
        mouseScrollAction.action.performed += OnMouseScroll;
    }

    void OnDisable()
    {
        mouseScrollAction.action.performed -= OnMouseScroll;
        mouseScrollAction.action.Disable();
    }

    void OnMouseScroll(InputAction.CallbackContext context)
    {
        // UI 위에 있으면 확대줌 적용 X
        if (vCamSwitcher.ZoomState != VCamSwitcher.Enum_ZoomTypes.Default || GameManager.UI.PointerOnUI())
            return;

        float scrollValue = context.ReadValue<float>();
        if (scrollValue > 0)
        {
            // 마우스 휠 위로
            _zoomLevel = Mathf.Min(_zoomLevel + 1, _zoomFOV.Length - 1);
        }
        else if (scrollValue < 0)
        {
            // 마우스 휠 아래로
            _zoomLevel = Mathf.Max(_zoomLevel - 1, 0);
        }

        StopAllCoroutines();
        StartCoroutine(SmoothZoomTransition());
    }

    IEnumerator SmoothZoomTransition()
    {
        float initialFOV = vCam.m_Lens.FieldOfView;
        Vector3 initialFollowOffset = cmTransposer.m_FollowOffset;
        float elapsedTime = 0f;

        _targetFOV = _zoomFOV[_zoomLevel];
        targetFollowOffset = _followOffsets[_zoomLevel];

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * _zoomSpeed;
            vCam.m_Lens.FieldOfView = Mathf.Lerp(initialFOV, _targetFOV, elapsedTime);
            cmTransposer.m_FollowOffset = Vector3.Lerp(initialFollowOffset, targetFollowOffset, elapsedTime);
            yield return null;
        }

        vCam.m_Lens.FieldOfView = _targetFOV;
        cmTransposer.m_FollowOffset = targetFollowOffset;
    }
}