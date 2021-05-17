// Prevents the Camera for reaching outside the play Area. Works alongside CameraMove.cs.
using UnityEngine;

public class cameraStop : MonoBehaviour
{
    public float panSpeed = 25f;
    public float panBorderThickness = 0.10f;
    Vector3 mousePosition;

    void Update()
    {
        Vector3 fakeNewPos = transform.position;
        if (!Input.GetKey(KeyCode.Space))
        {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            //If the Cursor is inside the screen space.
            if (screenRect.Contains(Input.mousePosition))
            {
                //Check if the Mouse Wheel is clicked, and then move the Camera when held down.
                if (Input.GetMouseButtonDown(2)) mousePosition = Input.mousePosition;
                if (Input.GetMouseButton(2))
                {
                    if (Input.mousePosition.y > mousePosition.y) fakeNewPos -= (transform.forward * (mousePosition.y - Input.mousePosition.y) * 0.005f) * Time.deltaTime / Time.timeScale * panSpeed;
                    if (Input.mousePosition.y < mousePosition.y) fakeNewPos += (transform.forward * (Input.mousePosition.y - mousePosition.y) * 0.005f) * Time.deltaTime / Time.timeScale * panSpeed;
                    if (Input.mousePosition.x > mousePosition.x) fakeNewPos -= (transform.right * (mousePosition.x - Input.mousePosition.x) * 0.005f) * Time.deltaTime / Time.timeScale * panSpeed;
                    if (Input.mousePosition.x < mousePosition.x) fakeNewPos += (transform.right * (Input.mousePosition.x - mousePosition.x) * 0.005f) * Time.deltaTime / Time.timeScale * panSpeed;
                }
                else
                {
                    //Prioritise WASD Movement, then check if the Cursor is on the edge of the screen.
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
                    {
                        if (Input.GetKey(KeyCode.W)) fakeNewPos += transform.forward * Time.deltaTime / Time.timeScale * panSpeed;
                        if (Input.GetKey(KeyCode.S)) fakeNewPos -= transform.forward * Time.deltaTime / Time.timeScale * panSpeed;
                        if (Input.GetKey(KeyCode.D)) fakeNewPos += transform.right * Time.deltaTime / Time.timeScale * panSpeed;
                        if (Input.GetKey(KeyCode.A)) fakeNewPos -= transform.right * Time.deltaTime / Time.timeScale * panSpeed;
                    }
                    else
                    {
                        if (Input.mousePosition.y >= Screen.height - (Screen.height * panBorderThickness)) fakeNewPos += transform.forward * Time.deltaTime / Time.timeScale * panSpeed;
                        if (Input.mousePosition.y <= Screen.height * panBorderThickness) fakeNewPos -= transform.forward * Time.deltaTime / Time.timeScale * panSpeed;
                        if (Input.mousePosition.x >= Screen.width - (Screen.width * panBorderThickness)) fakeNewPos += transform.right * Time.deltaTime / Time.timeScale * panSpeed;
                        if (Input.mousePosition.x <= Screen.width * panBorderThickness) fakeNewPos -= transform.right * Time.deltaTime / Time.timeScale * panSpeed;
                    }
                }
            }
            //Lastly check if the Cursor is off screen, priotise the mouse wheel then the WASD.
            else if (!Input.GetKey(KeyCode.Space) && !Input.GetMouseButtonDown(2))
            {
                if (Input.GetMouseButton(2))
                {
                    if (Input.mousePosition.y > mousePosition.y) fakeNewPos -= (transform.forward * (mousePosition.y - Input.mousePosition.y) * 0.005f) * Time.deltaTime / Time.timeScale * panSpeed;
                    if (Input.mousePosition.y < mousePosition.y) fakeNewPos += (transform.forward * (Input.mousePosition.y - mousePosition.y) * 0.005f) * Time.deltaTime / Time.timeScale * panSpeed;
                    if (Input.mousePosition.x > mousePosition.x) fakeNewPos -= (transform.right * (mousePosition.x - Input.mousePosition.x) * 0.005f) * Time.deltaTime / Time.timeScale * panSpeed;
                    if (Input.mousePosition.x < mousePosition.x) fakeNewPos += (transform.right * (Input.mousePosition.x - mousePosition.x) * 0.005f) * Time.deltaTime / Time.timeScale * panSpeed;
                }
                else
                {
                    if (Input.GetKey(KeyCode.W)) fakeNewPos += transform.forward * Time.deltaTime / Time.timeScale * panSpeed;
                    if (Input.GetKey(KeyCode.S)) fakeNewPos -= transform.forward * Time.deltaTime / Time.timeScale * panSpeed;
                    if (Input.GetKey(KeyCode.D)) fakeNewPos += transform.right * Time.deltaTime / Time.timeScale * panSpeed;
                    if (Input.GetKey(KeyCode.A)) fakeNewPos -= transform.right * Time.deltaTime / Time.timeScale * panSpeed;
                }
            }
            transform.position = fakeNewPos;
        }
        
        //Upon Reaching the edge of the World TP to the other side and update values.
        if (transform.position.x > 25)
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
        else if (transform.position.x < 0)
            transform.position = new Vector3(25, transform.position.y, transform.position.z);
        
        if (transform.position.z > 25)
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        else if (transform.position.z < 0)
            transform.position = new Vector3(transform.position.x, transform.position.y, 25);
    }
}