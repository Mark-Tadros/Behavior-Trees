// Prevents the Camera for reaching outside the play Area. Works alongside CameraMove.cs.
using UnityEngine;

public class cameraStop : MonoBehaviour
{
    public float panSpeed = 25f;
    Vector3 mousePosition;

    void Update()
    {
        Vector3 fakeNewPos = transform.position;
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        // Check if the Mouse Wheel is clicked, and then move the Camera when held down.
        if (screenRect.Contains(Input.mousePosition) && Input.GetMouseButtonDown(2)) mousePosition = Input.mousePosition;
        if (Input.GetMouseButton(2))
        {
            if (Input.mousePosition.y > mousePosition.y) fakeNewPos -= (transform.forward * (mousePosition.y - Input.mousePosition.y) * 0.0025f) * Time.deltaTime / Time.timeScale * panSpeed;
            else if (Input.mousePosition.y < mousePosition.y) fakeNewPos += (transform.forward * (Input.mousePosition.y - mousePosition.y) * 0.0025f) * Time.deltaTime / Time.timeScale * panSpeed;
            if (Input.mousePosition.x > mousePosition.x) fakeNewPos -= (transform.right * (mousePosition.x - Input.mousePosition.x) * 0.0025f) * Time.deltaTime / Time.timeScale * panSpeed;
            else if (Input.mousePosition.x < mousePosition.x) fakeNewPos += (transform.right * (Input.mousePosition.x - mousePosition.x) * 0.0025f) * Time.deltaTime / Time.timeScale * panSpeed;
        }
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)
            || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) fakeNewPos += transform.forward * Time.deltaTime / Time.timeScale * panSpeed;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) fakeNewPos -= transform.forward * Time.deltaTime / Time.timeScale * panSpeed;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) fakeNewPos += transform.right * Time.deltaTime / Time.timeScale * panSpeed;
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) fakeNewPos -= transform.right * Time.deltaTime / Time.timeScale * panSpeed;
        }
        transform.position = fakeNewPos;
        // Upon Reaching the edge of the World TP to the other side and update values.
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