using UnityEngine;

public class ViewModelObstacleAvoidance : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Camera playerCamera;

    [Header("Collision Check")]
    [SerializeField]
    private LayerMask obstacleLayers = ~0;

    [SerializeField]
    private float checkDistance = 1.25f;

    [SerializeField]
    private float checkRadius = 0.16f;

    [SerializeField]
    private QueryTriggerInteraction triggerInteraction =
        QueryTriggerInteraction.Ignore;

    [Header("Offsets")]
    [SerializeField]
    private float maxPushBack = 0.55f;

    [SerializeField]
    private float veryCloseDistance = 0.35f;

    [SerializeField]
    private float maxLowerAmount = 0.35f;

    [SerializeField]
    private Vector3 pushBackLocalDirection = Vector3.back;

    [SerializeField]
    private Vector3 lowerLocalDirection = Vector3.down;

    [Header("Motion")]
    [SerializeField]
    private float returnSharpness = 12f;

    [SerializeField]
    private float obstacleSharpness = 18f;

    private Vector3 normalLocalPosition;
    private bool hasNormalLocalPosition;

    private void Awake()
    {
        CacheNormalLocalPosition();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInParent<Camera>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        CacheNormalLocalPosition();

        Vector3 targetLocalPosition =
            normalLocalPosition + GetObstacleOffset();

        float sharpness =
            targetLocalPosition == normalLocalPosition
                ? returnSharpness
                : obstacleSharpness;

        transform.localPosition =
            Vector3.Lerp(
                transform.localPosition,
                targetLocalPosition,
                1f - Mathf.Exp(-sharpness * Time.deltaTime)
            );
    }

    private Vector3 GetObstacleOffset()
    {
        if (playerCamera == null)
        {
            return Vector3.zero;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.SphereCast(
                ray,
                checkRadius,
                out RaycastHit hit,
                checkDistance,
                obstacleLayers,
                triggerInteraction
            ))
        {
            return Vector3.zero;
        }

        float closeness =
            1f - Mathf.Clamp01(hit.distance / checkDistance);

        float pushBackAmount =
            Mathf.SmoothStep(0f, maxPushBack, closeness);

        float lowerProgress =
            1f - Mathf.Clamp01(hit.distance / veryCloseDistance);

        float lowerAmount =
            Mathf.SmoothStep(0f, maxLowerAmount, lowerProgress);

        return
            pushBackLocalDirection.normalized * pushBackAmount +
            lowerLocalDirection.normalized * lowerAmount;
    }

    private void CacheNormalLocalPosition()
    {
        if (hasNormalLocalPosition)
        {
            return;
        }

        normalLocalPosition = transform.localPosition;
        hasNormalLocalPosition = true;
    }

    public void SetNormalLocalPosition(Vector3 localPosition)
    {
        normalLocalPosition = localPosition;
        hasNormalLocalPosition = true;
    }
}
