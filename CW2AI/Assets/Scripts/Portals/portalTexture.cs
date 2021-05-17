//Updates the four Portals at the edge of the worlds.
using UnityEngine;

public class portalTexture : MonoBehaviour
{
    public Camera cameraA; public Camera cameraB;
    public Camera cameraC; public Camera cameraD;

    public Material cameraMatA; public Material cameraMatB;
    public Material cameraMatC; public Material cameraMatD;

    // Use this for initialization
    void Start() { UpdateTexture(); }

    // Update is called once per frame
    public void UpdateTexture ()
    {
        //This gets called everytime the player changes or updates the Camera.
        if (cameraB.targetTexture != null) cameraB.targetTexture.Release();
        cameraB.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraMatB.mainTexture = cameraB.targetTexture;

        if (cameraA.targetTexture != null) cameraA.targetTexture.Release();
        cameraA.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraMatA.mainTexture = cameraA.targetTexture;

        if (cameraD.targetTexture != null) cameraD.targetTexture.Release();
        cameraD.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraMatD.mainTexture = cameraD.targetTexture;

        if (cameraC.targetTexture != null) cameraC.targetTexture.Release();
        cameraC.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraMatC.mainTexture = cameraC.targetTexture;
    }
}