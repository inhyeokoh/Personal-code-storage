using UnityEngine;

public class VirtualCharacterCamera : MonoBehaviour
{
    public static VirtualCharacterCamera instance;
    public GameObject virtualCharacter;
    Transform tr;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 가상의 플레이어 생성 후, 카메라 위치 조정
        if (virtualCharacter == null)
        {
            virtualCharacter = GameManager.Resources.Instantiate($"Prefabs/VirtualCharacter/{PlayerController.instance._class} Variant", transform.parent);
            transform.SetParent(virtualCharacter.transform);
            tr = GetComponent<Transform>();
            tr.position = new Vector3(0, 2f, 6f);
            tr.rotation = Quaternion.Euler(0, 180f, 0);
        }
    }
}
