// Works alongside the CameraStop.cs. Controls Rotation. Works alongside CameraStop.cs.
using UnityEngine;

public class cameraMove : MonoBehaviour
{
    Camera mainCamera;
    public Camera camera1; public Camera camera2;
    public Camera camera3; public Camera camera4;

    public Transform cameraTransform;

    [HideInInspector] public Vector3 _cameraOffset;

    void Start()
    {
        _cameraOffset = transform.position - cameraTransform.position;
        mainCamera = this.GetComponent<Camera>();
    }
    // Updates the position of the Camera based on the cameraStop Inputs.
    void LateUpdate()
    {
        Vector3 newPos = cameraTransform.position + _cameraOffset;

        transform.position = newPos;
        
        camera1.GetComponent<portalCamera>().Update(); camera2.GetComponent<portalCamera>().Update();
        camera3.GetComponent<portalCamera>().Update(); camera4.GetComponent<portalCamera>().Update();
    }
}