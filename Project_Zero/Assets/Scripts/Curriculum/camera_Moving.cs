using UnityEngine;

public class CameraController : MonoBehaviour
{
    private bool isAlt;
    private Vector2 curPoint;
    private float dragSpeed = 0.01f;
    private Vector2 prevPoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            isAlt = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt)) {
            isAlt = false;
        }

        if (Input.GetMouseButton(0)) {
            curPoint = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                prevPoint = Input.mousePosition;
            }

            if (isAlt) {

                Vector3 move = (curPoint - prevPoint) * (-1) * dragSpeed;
                Debug.Log(move);
                transform.Translate(move);
            }
            prevPoint = Input.mousePosition;
        }
    }

    private void LateUpdate()
    {
        
    }
}