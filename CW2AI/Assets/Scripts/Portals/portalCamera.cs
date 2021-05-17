//Locates the opposing Camera and Plane and allows both sides to look similar.
using UnityEngine;

public class portalCamera : MonoBehaviour
{
    public Transform playerCamera;
    public Transform portal;
    public Transform otherPortal;
	
	// Update is called once per frame
	public void Update ()
    {
        //Updates the Position to be Offset from the other Camera.
        Vector3 playerOffsetFromPortal = playerCamera.position - otherPortal.position;
        transform.position = portal.position + playerOffsetFromPortal;

        //Updates the Rotation based on the Players Cameras Rotation.
        float angularDifferenceBetweenPortalRotations = Quaternion.Angle(portal.rotation, otherPortal.rotation);
        Quaternion portalRotationalDifference = Quaternion.AngleAxis(angularDifferenceBetweenPortalRotations, Vector3.up);
        Vector3 newCameraDirection = portalRotationalDifference * playerCamera.forward;

        //Updates the Rotation to be Offset from the other Player.
        transform.rotation = Quaternion.LookRotation(newCameraDirection, Vector3.up);
    }
}