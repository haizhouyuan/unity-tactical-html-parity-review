using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Beginner-friendly character controller for the star collector prototype.
/// WASD or arrow keys move the capsule around the arena.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class StarCollectorPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float gravity = -18f;

    private CharacterController controller;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        var input = ReadMoveInput();
        input = Vector3.ClampMagnitude(input, 1f);

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        var movement = input * moveSpeed;
        movement.y = verticalVelocity;
        controller.Move(movement * Time.deltaTime);

        if (input.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(input, Vector3.up);
        }
    }

    private static Vector3 ReadMoveInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector3.zero;
        }

        var x = 0f;
        var z = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            x -= 1f;
        }
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            x += 1f;
        }
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            z -= 1f;
        }
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            z += 1f;
        }

        return new Vector3(x, 0f, z);
    }
}
