using UnityEngine;
using UnityEngine.UI;

public class EnemyHappinessBar : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public Image fillImage;
    public Canvas canvas;

    [Header("Settings")]
    public float maxDistance = 10f;
    public float minDistance = 2f;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    [Header("Colors")]
    public Color sadColor = Color.red;
    public Color happyColor = Color.green;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (canvas != null)
        {
            canvas.worldCamera = mainCamera;
        }
    }

    void LateUpdate()
    {
        if (target == null || fillImage == null)
        {
            return;
        }

        UpdatePosition();

        float distanceToTarget = Vector2.Distance(transform.parent.position, target.position);
        float happiness = CalculateHappiness(distanceToTarget);

        fillImage.fillAmount = happiness;
        fillImage.color = Color.Lerp(sadColor, happyColor, happiness);
    }

    private float CalculateHappiness(float distance)
    {
        if (distance >= maxDistance)
        {
            return 0f;
        }

        if (distance <= minDistance)
        {
            return 1f;
        }

        float normalizedDistance = (distance - minDistance) / (maxDistance - minDistance);
        return 1f - normalizedDistance;
    }

    private void UpdatePosition()
    {
        if (transform.parent != null)
        {
            transform.position = transform.parent.position + offset;
        }

        // if (canvas != null && mainCamera != null)
        // {
        //     transform.rotation = mainCamera.transform.rotation;
        // }
    }
}
