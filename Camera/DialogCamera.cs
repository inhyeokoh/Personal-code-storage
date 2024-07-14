using Cinemachine;
using UnityEngine;

public class DialogCamera : MonoBehaviour
{    
    public CinemachineVirtualCamera vCam;
    CinemachineTransposer _cmTransposer;
    Camera _mainCamera;

    GameObject virtualPlayer;

    void Start()
    {
        _cmTransposer = vCam.GetCinemachineComponent<CinemachineTransposer>();
        _mainCamera = Camera.main.GetComponent<Camera>();
    }

    public void SetDialogCamera()
    {
        int npcID = PlayerController.instance._interaction.InteractingNpcID;
        Transform npcTr = GameManager.Data.npcDict[npcID].transform;

        // 가상의 플레이어 생성
        if (virtualPlayer == null)
        {
            virtualPlayer = GameManager.Resources.Instantiate("Prefabs/VirtualCharacter/Warrior Variant");
            virtualPlayer.name = "VirtualPlayer";
        }
        virtualPlayer.SetActive(true);
        virtualPlayer.transform.position = npcTr.position + npcTr.forward * 5f;

        // 가상 카메라 설정
        _cmTransposer.m_FollowOffset = new Vector3(-5.5f, 4f, 7.5f);
        vCam.m_Lens.FieldOfView = 65f;
        vCam.Follow = npcTr;
        vCam.LookAt = virtualPlayer.transform;

        // 레이어 마스크 동적 변경
        _mainCamera.cullingMask &= ~(1 << 3);  // 3번 몬스터 레이어 제외
        _mainCamera.cullingMask &= ~(1 << 7);  // 7번 실제 캐릭터 레이어 제외
        _mainCamera.cullingMask &= ~(1 << 8);  // 8번 캐릭터 이펙트 레이어 제외
        _mainCamera.cullingMask &= ~(1 << 9);  // 9번 몬스터 이펙트 레이어 제외
        _mainCamera.cullingMask |= (1 << 16); // 16번 가상 캐릭터 레이어 추가
    }

    public void OutDialogCamera()
    {
        virtualPlayer.SetActive(false);

        // 레이어 마스크 동적 변경
        _mainCamera.cullingMask &= ~(1 << 16); // 16번 가상 캐릭터 레이어 제외
        _mainCamera.cullingMask |= (1 << 3);   // 3번 몬스터 레이어 추가
        _mainCamera.cullingMask |= (1 << 7);   // 7번 실제 캐릭터 레이어 추가
        _mainCamera.cullingMask |= (1 << 8);   // 8번 캐릭터 이펙트 레이어 추가
        _mainCamera.cullingMask |= (1 << 9);   // 9번 몬스터 이펙트 레이어 추가
    }
}
