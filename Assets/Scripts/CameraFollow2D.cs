using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.2f;

    [Range(0.05f, 0.49f)]
    public float horizontalMargin = 0.33f;

    [Range(0.05f, 0.49f)]
    public float verticalMargin = 0.33f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 cameraPosition = transform.position;
        Vector3 targetPosition = cameraPosition;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(target.position);

        float leftLimit = horizontalMargin;
        float rightLimit = 1f - horizontalMargin;
        float bottomLimit = verticalMargin;
        float topLimit = 1f - verticalMargin;

        if (viewportPos.x < leftLimit || viewportPos.x > rightLimit)
        {
            float clampedX = Mathf.Clamp(viewportPos.x, leftLimit, rightLimit);
            float deltaXViewport = viewportPos.x - clampedX;
            float worldWidth = Camera.main.orthographicSize * 2f * Camera.main.aspect;
            float deltaXWorld = deltaXViewport * worldWidth;
            targetPosition.x += deltaXWorld;
        }

        if (viewportPos.y < bottomLimit || viewportPos.y > topLimit)
        {
            float clampedY = Mathf.Clamp(viewportPos.y, bottomLimit, topLimit);
            float deltaYViewport = viewportPos.y - clampedY;
            float worldHeight = Camera.main.orthographicSize * 2f;
            float deltaYWorld = deltaYViewport * worldHeight;
            targetPosition.y += deltaYWorld;
        }

        targetPosition.z = cameraPosition.z;
        transform.position = Vector3.SmoothDamp(cameraPosition, targetPosition, ref velocity, smoothTime);
    }
}
