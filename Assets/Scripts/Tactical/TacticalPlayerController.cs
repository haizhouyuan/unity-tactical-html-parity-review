using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TacticalPlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5.2f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 3.2f;
    [SerializeField] private float proneSpeed = 1.9f;
    [SerializeField] private float jumpSpeed = 5.1f;
    [SerializeField] private float gravity = -18f;
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private Transform cameraTarget;

    private CharacterController controller;
    private TacticalGameManager game;
    private float verticalVelocity;
    private float yaw;
    private float pitch = 24f;
    private float nextFootstepTime;
    private TacticalStance stance = TacticalStance.Stand;
    private TacticalCameraMode cameraMode = TacticalCameraMode.ThirdPerson;
    private bool ads;

    public Transform CameraTarget => cameraTarget == null ? transform : cameraTarget;
    public TacticalStance Stance => stance;
    public TacticalCameraMode CameraMode => cameraMode;
    public bool IsAds => ads;
    public float Pitch => pitch;
    public float ControllerHeight => controller == null ? 0f : controller.height;
    public float VerticalVelocity => verticalVelocity;

    public void ResetView(float yawDegrees, float pitchDegrees)
    {
        EnsureController();
        yaw = yawDegrees;
        pitch = Mathf.Clamp(pitchDegrees, -38f, 68f);
        cameraMode = TacticalCameraMode.ThirdPerson;
        stance = TacticalStance.Stand;
        ads = false;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        UpdateControllerHeight();
    }

    private void Awake()
    {
        EnsureController();
        game = FindAnyObjectByType<TacticalGameManager>();
        yaw = transform.eulerAngles.y;
    }

    private void EnsureController()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }

    private void Update()
    {
        if (game == null || game.IsPlayerDown || game.IsInLobby)
        {
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        HandleLook();
        var input = ReadMoveInput(keyboard);
        var wantsSprint = keyboard.leftShiftKey.isPressed && input.sqrMagnitude > 0.01f && game.TryUseStamina(Time.deltaTime * 22f);
        var speed = stance == TacticalStance.Prone ? proneSpeed : stance == TacticalStance.Crouch ? crouchSpeed : wantsSprint ? sprintSpeed : walkSpeed;

        if (!wantsSprint)
        {
            game.RecoverStamina(Time.deltaTime * 16f);
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            TryJump();
        }

        verticalVelocity += gravity * Time.deltaTime;
        var movement = transform.TransformDirection(input) * speed;
        movement.y = verticalVelocity;
        controller.Move(movement * Time.deltaTime);
        MaybePlayFootstep(input, wantsSprint);

        HandleActions(keyboard);
        UpdateControllerHeight();
    }

    private static Vector3 ReadMoveInput(Keyboard keyboard)
    {
        var x = 0f;
        var z = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;

        return Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f);
    }

    private void HandleActions(Keyboard keyboard)
    {
        if (keyboard.fKey.wasPressedThisFrame) game.TryPickupNearest();
        if (keyboard.rKey.wasPressedThisFrame) game.Reload();
        if (keyboard.vKey.wasPressedThisFrame) ToggleCameraMode();
        if (keyboard.cKey.wasPressedThisFrame) SetStance(stance == TacticalStance.Crouch ? TacticalStance.Stand : TacticalStance.Crouch);
        if (keyboard.zKey.wasPressedThisFrame) SetStance(stance == TacticalStance.Prone ? TacticalStance.Stand : TacticalStance.Prone);
        if (keyboard.digit1Key.wasPressedThisFrame) game.SelectWeapon("pistol");
        if (keyboard.digit2Key.wasPressedThisFrame) game.SelectWeapon("shotgun");
        if (keyboard.digit3Key.wasPressedThisFrame) game.SelectWeapon("rifle");
        if (keyboard.digit4Key.wasPressedThisFrame) game.SelectWeapon("dmr");
        if (keyboard.digit5Key.wasPressedThisFrame) game.UseHeal(TacticalLootKind.Bandage);
        if (keyboard.digit6Key.wasPressedThisFrame) game.UseHeal(TacticalLootKind.FirstAid);
        if (keyboard.digit7Key.wasPressedThisFrame) game.UseHeal(TacticalLootKind.Medkit);

        var mouse = Mouse.current;
        SetAds(mouse != null && mouse.rightButton.isPressed);
        if (keyboard.enterKey.wasPressedThisFrame) game.StartRound();
        var firePressed = mouse != null && mouse.leftButton.wasPressedThisFrame;
        var fireHeld = mouse != null && mouse.leftButton.isPressed && game.CurrentWeaponAutomatic;
        if (firePressed || fireHeld)
        {
            game.FireCurrentWeapon();
        }
    }

    private void MaybePlayFootstep(Vector3 input, bool sprinting)
    {
        if (!controller.isGrounded || input.sqrMagnitude < 0.05f || Time.time < nextFootstepTime)
        {
            return;
        }

        nextFootstepTime = Time.time + (sprinting ? 0.24f : stance == TacticalStance.Prone ? 0.52f : 0.39f);
        game.TryPlayFootstepSfx(sprinting ? 1f : stance == TacticalStance.Crouch ? 0.45f : 0.65f);
    }

    public void ToggleCameraMode()
    {
        cameraMode = cameraMode == TacticalCameraMode.ThirdPerson ? TacticalCameraMode.FirstPerson : TacticalCameraMode.ThirdPerson;
    }

    public void SetCameraMode(TacticalCameraMode mode)
    {
        cameraMode = mode;
    }

    public void SetStance(TacticalStance nextStance, bool snapHeight = false)
    {
        stance = nextStance;
        UpdateControllerHeight(snapHeight);
    }

    public void SetAds(bool aiming)
    {
        ads = aiming;
    }

    public bool TryJump()
    {
        var nearGround = controller.isGrounded || Mathf.Abs(verticalVelocity) <= 1.1f;
        if (!nearGround || stance != TacticalStance.Stand)
        {
            return false;
        }

        verticalVelocity = jumpSpeed;
        game.TryUseStamina(8f);
        return true;
    }

    private void HandleLook()
    {
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        var delta = mouse.delta.ReadValue();
        yaw += delta.x * mouseSensitivity;
        pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, -38f, 68f);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private void UpdateControllerHeight(bool snap = false)
    {
        EnsureController();
        if (controller == null)
        {
            return;
        }

        var targetHeight = stance == TacticalStance.Prone ? 0.75f : stance == TacticalStance.Crouch ? 1.25f : 2.05f;
        controller.height = snap ? targetHeight : Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 12f);
        controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
        if (cameraTarget != null)
        {
            var targetY = stance == TacticalStance.Prone ? 0.65f : stance == TacticalStance.Crouch ? 1.05f : 1.55f;
            cameraTarget.localPosition = snap ? new Vector3(0f, targetY, 0f) : Vector3.Lerp(cameraTarget.localPosition, new Vector3(0f, targetY, 0f), Time.deltaTime * 12f);
        }
    }
}

public enum TacticalCameraMode
{
    ThirdPerson,
    FirstPerson
}

public enum TacticalStance
{
    Stand,
    Crouch,
    Prone
}
