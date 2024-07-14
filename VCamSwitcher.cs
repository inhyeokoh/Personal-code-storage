using UnityEngine;

public class VCamSwitcher : MonoBehaviour
{
    public CharacterFollowCamera charFollowCamera;
    public DialogCamera dialogCamera;

    public enum Enum_ZoomTypes
    {
        Default,
        DialogZoom,
    }
    public Enum_ZoomTypes ZoomState { get; set; }

    void Start()
    {
        charFollowCamera.vCam.Priority = 10;
        dialogCamera.vCam.Priority = 0;
        ZoomState = Enum_ZoomTypes.Default;
    }

    public void SwitchToVCam2()
    {
        charFollowCamera.vCam.Priority = 0;
        dialogCamera.vCam.Priority = 10;

        ZoomState = Enum_ZoomTypes.DialogZoom;
        dialogCamera.SetDialogCamera();
    }

    public void SwitchToVCam1()
    {
        charFollowCamera.vCam.Priority = 10;
        dialogCamera.vCam.Priority = 0;
        ZoomState = Enum_ZoomTypes.Default;
        dialogCamera.OutDialogCamera();
    }
}
