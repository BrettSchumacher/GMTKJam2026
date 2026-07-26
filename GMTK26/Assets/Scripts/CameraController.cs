using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public CinemachineVirtualCamera PlayerCamera;
    public CinemachineBrain brain;

    private Stack<CinemachineVirtualCamera> CinemachineCameraStack = new();

    private void Awake()
    {
        if (Instance)
        {
            Debug.LogError("Duplciate CameraController detected");
            return;
        }

        Instance = this;
    }

    public void SetPlayerFov(float newFov)
    {
        if (!PlayerCamera)
        {
            return;
        }

        PlayerCamera.m_Lens.FieldOfView = newFov;
    }
    
    public void PushCamera(CinemachineVirtualCamera camera, bool clearStack = false)
    {
        if (!camera)
        {
            Debug.LogError("Invalid Camera");
            return;
        }

        if (CinemachineCameraStack.Count > 0 && camera == CinemachineCameraStack.Peek())
        {
            return;
        }

        if (CinemachineCameraStack.Count > 0)
        {
            Debug.Log("Going from camera " + CinemachineCameraStack.Peek().name + " to " + camera.name);
            CinemachineCameraStack.Peek().enabled = false;
        }
        else
        {
            Debug.Log("Setting camera to " + camera.name);
        }
        
        if (clearStack)
        {
            CinemachineCameraStack.Clear();
        }
        
        CinemachineCameraStack.Push(camera);
        SetCamera(camera);
    }

    public void PopInputState()
    {
        if (CinemachineCameraStack.Count == 0)
        {
            return;
        }

        CinemachineCameraStack.Pop().enabled = false;
        
        CinemachineVirtualCamera newCamera =  CinemachineCameraStack.Count == 0 ? PlayerCamera : CinemachineCameraStack.Peek();
        SetCamera(newCamera);
    }

    private void SetCamera(CinemachineVirtualCamera camera)
    {
        camera.enabled = true;
    }
}
