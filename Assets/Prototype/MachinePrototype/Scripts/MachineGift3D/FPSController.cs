using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInventory))]
public class FPSController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CharacterController controller;

    [SerializeField]
    private Rigidbody playerRigidbody;

    [SerializeField]
    private Transform cameraRoot;

    [Header("Movement")]
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float jumpHeight = 2f;

    [SerializeField]
    private float gravity = -9.81f;

    [Header("Mouse")]
    [SerializeField]
    private float sensitivity = 0.1f;

    [Header("Head Bob")]
    [SerializeField]
    private bool enableHeadBob = true;

    [SerializeField]
    private float bobFrequency = 8f;

    [SerializeField]
    private float bobAmplitude = 0.04f;

    [SerializeField]
    private float bobSpeedMultiplier = 1f;

    [SerializeField]
    private float minBobMoveSpeed = 0.1f;

    [SerializeField]
    private float bobReturnSpeed = 10f;

    private PlayerInputActions input;
    private PlayerInventory inventory;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalVelocity;
    private float pitch;

    [Header("Interaction")]
    [SerializeField]
    private float interactDistance = 3f;

    [SerializeField]
    private LayerMask interactLayer;

    [SerializeField]
    private Camera playerCamera;

    [Header("Crosshair")]
    [SerializeField]
    private RectTransform crosshair;

    [SerializeField]
    private float normalSize = 16f;

    [SerializeField]
    private float interactSize = 22f;

    [SerializeField]
    private float resizeSpeed = 10f;

    [Header("Interaction Guide")]
    [SerializeField]
    private CanvasGroup interactionGuide;

    [SerializeField]
    private float guideFadeSpeed = 10f;

    [SerializeField]
    private float guideHiddenScale = 0.92f;

    [SerializeField]
    private float guideVisibleScale = 1f;

    private bool canInteract;
    private float guideAnimationProgress;
    private Vector3 guideBaseScale = Vector3.one;
    private bool hasGuideBaseScale;
    private Vector3 cameraRootBaseLocalPosition;
    private float bobTimer;
    private Vector3 lastPosition;
    private float horizontalSpeed;
    private bool gameplayInputLocked;

    private void Awake()
    {
        input = new PlayerInputActions();

        if (controller == null)
            controller =
                GetComponent<CharacterController>();

        if (GetComponent<PlayerInventory>() == null)
        {
            gameObject.AddComponent<PlayerInventory>();
        }

        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        inventory = GetComponent<PlayerInventory>();
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed +=
            OnMovePerformed;

        input.Player.Move.canceled +=
            OnMoveCanceled;

        input.Player.Look.performed +=
            OnLookPerformed;

        input.Player.Look.canceled +=
            OnLookCanceled;

        input.Player.Jump.performed +=
            OnJumpPerformed;

        input.Player.Interact.performed +=
            OnInteractPerformed;
    }

    private void OnDisable()
    {
        input.Player.Move.performed -=
            OnMovePerformed;

        input.Player.Move.canceled -=
            OnMoveCanceled;

        input.Player.Look.performed -=
            OnLookPerformed;

        input.Player.Look.canceled -=
            OnLookCanceled;

        input.Player.Jump.performed -=
            OnJumpPerformed;

        input.Player.Interact.performed -=
            OnInteractPerformed;

        input.Disable();
    }

    private void Start()
    {
        if (cameraRoot != null)
        {
            cameraRootBaseLocalPosition = cameraRoot.localPosition;
        }

        lastPosition = transform.position;

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;

        ConfigureRigidbody();
    }

    private void Update()
    {
        if (gameplayInputLocked)
        {
            return;
        }

        MouseLook();

        Movement();

        Gravity();

        UpdateMovementSpeed();

        UpdateHeadBob();

        CheckInteractable();

        UpdateCrosshair();

        UpdateInteractionGuide();
    }

    private void MouseLook()
    {
        if (cameraRoot == null)
        {
            return;
        }

        float mouseX =
            lookInput.x *
            sensitivity;

        float mouseY =
            lookInput.y *
            sensitivity;

        transform.Rotate(
            Vector3.up *
            mouseX
        );

        pitch -= mouseY;

        pitch = Mathf.Clamp(
            pitch,
            -89f,
            89f
        );

        cameraRoot.localRotation =
            Quaternion.Euler(
                pitch,
                0,
                0
            );
    }

    private void Movement()
    {
        if (controller == null)
        {
            return;
        }

        Vector3 move =
            transform.right *
            moveInput.x +

            transform.forward *
            moveInput.y;

        controller.Move(
            move *
            moveSpeed *
            Time.deltaTime
        );
    }

    private void Jump()
    {
        if (controller == null)
            return;

        if (!controller.isGrounded)
            return;

        verticalVelocity =
            Mathf.Sqrt(
                jumpHeight *
                -2f *
                gravity
            );
    }

    private void Gravity()
    {
        if (controller == null)
        {
            return;
        }

        if (
            controller.isGrounded &&
            verticalVelocity < 0
        )
        {
            verticalVelocity = -2f;
        }

        verticalVelocity +=
            gravity *
            Time.deltaTime;

        controller.Move(
            Vector3.up *
            verticalVelocity *
            Time.deltaTime
            );
    }

    private void UpdateHeadBob()
    {
        if (!enableHeadBob || cameraRoot == null || controller == null)
        {
            return;
        }

        bool isMoving =
            horizontalSpeed > minBobMoveSpeed &&
            controller.isGrounded;

        if (isMoving)
        {
            float speedRatio =
                Mathf.Clamp01(horizontalSpeed / moveSpeed);

            bobTimer +=
                bobFrequency *
                Mathf.Lerp(0.75f, 1.35f, speedRatio) *
                bobSpeedMultiplier *
                Time.deltaTime;

            float bobOffset =
                Mathf.Sin(bobTimer) *
                bobAmplitude *
                speedRatio;

            Vector3 targetPosition =
                cameraRootBaseLocalPosition +
                Vector3.up *
                bobOffset;

            cameraRoot.localPosition =
                Vector3.Lerp(
                    cameraRoot.localPosition,
                    targetPosition,
                    bobReturnSpeed *
                    Time.deltaTime
                );

            return;
        }

        bobTimer = 0f;

        cameraRoot.localPosition =
            Vector3.Lerp(
                cameraRoot.localPosition,
                cameraRootBaseLocalPosition,
                bobReturnSpeed *
                Time.deltaTime
            );
    }

    private void UpdateMovementSpeed()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movementDelta = currentPosition - lastPosition;
        movementDelta.y = 0f;

        horizontalSpeed =
            movementDelta.magnitude /
            Mathf.Max(Time.deltaTime, 0.0001f);

        lastPosition = currentPosition;
    }

    private void TryInteract()
    {
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (
            Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactDistance,
                interactLayer
            )
        )
        {
            IInteractable target =
                hit.collider
                .GetComponentInParent<IInteractable>();

            if (target != null && target.CanInteract(inventory))
            {
                target.Interact(inventory);
            }
        }
    }

    private void CheckInteractable()
    {
        canInteract = false;

        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (
            Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactDistance,
                interactLayer
            )
        )
        {
            IInteractable target =
                hit.collider
                .GetComponentInParent<IInteractable>();

            if (target != null && target.CanInteract(inventory))
            {
                canInteract = true;
            }
        }
    }

    private void UpdateCrosshair()
    {
        if (crosshair == null)
        {
            return;
        }

        float targetSize =
            canInteract
            ? interactSize
            : normalSize;

        Vector2 current =
            crosshair.sizeDelta;

        Vector2 target =
            Vector2.one * targetSize;

        crosshair.sizeDelta =
            Vector2.Lerp(
                current,
                target,
                resizeSpeed *
                Time.deltaTime
            );
    }

    private void UpdateInteractionGuide()
    {
        if (interactionGuide == null)
        {
            return;
        }

        CacheInteractionGuideScale();

        if (canInteract)
        {
            interactionGuide.gameObject.SetActive(true);

            guideAnimationProgress =
                Mathf.MoveTowards(
                    guideAnimationProgress,
                    1f,
                    guideFadeSpeed * Time.deltaTime
                );
        }
        else
        {
            guideAnimationProgress =
                Mathf.MoveTowards(
                    guideAnimationProgress,
                    0f,
                    guideFadeSpeed * Time.deltaTime
                );
        }

        float easedProgress =
            canInteract
                ? EaseInSine(guideAnimationProgress)
                : 1f - EaseOutSine(1f - guideAnimationProgress);

        interactionGuide.alpha = easedProgress;

        float scaleMultiplier =
            Mathf.Lerp(
                guideHiddenScale,
                guideVisibleScale,
                easedProgress
            );

        interactionGuide.transform.localScale =
            guideBaseScale *
            scaleMultiplier;

        bool visible = guideAnimationProgress > 0.01f;
        interactionGuide.interactable = false;
        interactionGuide.blocksRaycasts = false;
        interactionGuide.gameObject.SetActive(visible || canInteract);
    }

    private void CacheInteractionGuideScale()
    {
        if (hasGuideBaseScale)
        {
            return;
        }

        guideBaseScale = interactionGuide.transform.localScale;
        hasGuideBaseScale = true;
    }

    private float EaseInSine(float value)
    {
        return 1f -
            Mathf.Cos(
                Mathf.Clamp01(value) *
                Mathf.PI *
                0.5f
            );
    }

    private float EaseOutSine(float value)
    {
        return Mathf.Sin(
            Mathf.Clamp01(value) *
            Mathf.PI *
            0.5f
        );
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (gameplayInputLocked)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        if (gameplayInputLocked)
        {
            lookInput = Vector2.zero;
            return;
        }

        lookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        lookInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (gameplayInputLocked)
        {
            return;
        }

        Jump();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (gameplayInputLocked)
        {
            return;
        }

        TryInteract();
    }

    public void SetGameplayInputLocked(bool locked)
    {
        if (gameplayInputLocked == locked)
        {
            return;
        }

        gameplayInputLocked = locked;

        if (locked)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
        }
        else
        {
            lastPosition = transform.position;
        }

        Cursor.lockState =
            locked ? CursorLockMode.None : CursorLockMode.Locked;

        Cursor.visible = locked;

        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(!locked);
        }

        if (interactionGuide != null)
        {
            interactionGuide.gameObject.SetActive(!locked);
        }

        ConfigureRigidbody();
    }

    private void ConfigureRigidbody()
    {
        if (playerRigidbody == null)
        {
            return;
        }

        playerRigidbody.useGravity = false;
        playerRigidbody.isKinematic = true;
        playerRigidbody.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;
    }
}
