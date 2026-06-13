using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // The object the camera should follow (the mouse).
    public GameObject targetObject;

    // The initial horizontal gap between the camera and the target. Keeping this
    // constant lets the mouse stay offset to the left of the screen.
    private float distanceToTarget;

    void Start()
    {
        distanceToTarget = transform.position.x - targetObject.transform.position.x;
    }

    void Update()
    {
        float targetObjectX = targetObject.transform.position.x;

        Vector3 newCameraPosition = transform.position;
        newCameraPosition.x = targetObjectX + distanceToTarget;

        transform.position = newCameraPosition;
    }
}
