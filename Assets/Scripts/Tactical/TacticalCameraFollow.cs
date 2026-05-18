using UnityEngine;

public class TacticalCameraFollow : MonoBehaviour
{
    [SerializeField] private TacticalPlayerController player;
    [SerializeField] private float firstPersonForwardOffset = 0.18f;
    [SerializeField] private float followSpeed = 9f;
    [SerializeField] private float cameraCollisionRadius = 0.22f;
    [SerializeField] private float cameraCollisionPadding = 0.38f;
    [SerializeField] private float minThirdPersonDistance = 1.6f;

    private Camera followCamera;
    private TacticalGameManager game;

    private void Awake()
    {
        followCamera = GetComponent<Camera>();
        game = FindAnyObjectByType<TacticalGameManager>();
    }

    public void SetPlayer(TacticalPlayerController target)
    {
        player = target;
    }

    public void SnapToPlayer()
    {
        if (player == null)
        {
            return;
        }

        CalculateCamera(out var desired, out var desiredRotation);
        transform.position = desired;
        transform.rotation = desiredRotation;
        if (followCamera != null)
        {
            followCamera.fieldOfView = player.IsAds ? 46f : 64f;
        }
    }

    public void SnapToLobby()
    {
        CalculateLobbyCamera(out var desired, out var desiredRotation);
        transform.position = desired;
        transform.rotation = desiredRotation;
        if (followCamera != null)
        {
            followCamera.fieldOfView = 56f;
        }
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        if (game == null)
        {
            game = FindAnyObjectByType<TacticalGameManager>();
        }

        if (game != null && game.IsInLobby)
        {
            CalculateLobbyCamera(out var lobbyPosition, out var lobbyRotation);
            transform.position = Vector3.Lerp(transform.position, lobbyPosition, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, lobbyRotation, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
            if (followCamera != null)
            {
                followCamera.fieldOfView = Mathf.Lerp(followCamera.fieldOfView, 56f, Time.deltaTime * 10f);
            }
            return;
        }

        CalculateCamera(out var desired, out var desiredRotation);
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        if (followCamera != null)
        {
            followCamera.fieldOfView = Mathf.Lerp(followCamera.fieldOfView, player.IsAds ? 46f : 64f, Time.deltaTime * 10f);
        }
    }

    private void CalculateCamera(out Vector3 desired, out Quaternion desiredRotation)
    {
        var target = player.CameraTarget.position;
        var viewRotation = Quaternion.Euler(player.Pitch, player.transform.eulerAngles.y, 0f);
        if (player.CameraMode == TacticalCameraMode.FirstPerson)
        {
            desired = target + viewRotation * new Vector3(0f, 0.05f, firstPersonForwardOffset);
            desiredRotation = viewRotation;
            return;
        }

        var forward = viewRotation * Vector3.forward;
        var flatRotation = Quaternion.Euler(0f, player.transform.eulerAngles.y, 0f);
        var flatForward = flatRotation * Vector3.forward;
        var right = Quaternion.Euler(0f, player.transform.eulerAngles.y, 0f) * Vector3.right;
        var distance = player.IsAds ? 3.25f : 12.0f;
        var shoulder = player.IsAds ? 0.92f : 4.5f;
        var lift = player.IsAds ? 0.18f : 1.6f;
        desired = target - flatForward * distance + right * shoulder + Vector3.up * lift;
        desired = ResolveThirdPersonObstruction(target, desired);
        var lookAhead = player.IsAds ? 6f : 11.0f;
        desiredRotation = Quaternion.LookRotation((target + forward * lookAhead + Vector3.up * 0.15f - desired).normalized, Vector3.up);
    }

    private Vector3 ResolveThirdPersonObstruction(Vector3 target, Vector3 desired)
    {
        var offset = desired - target;
        var distance = offset.magnitude;
        if (distance <= 0.01f)
        {
            return desired;
        }

        var direction = offset / distance;
        var hits = Physics.SphereCastAll(target, cameraCollisionRadius, direction, distance, ~0, QueryTriggerInteraction.Ignore);
        var nearest = distance;
        foreach (var hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            var hitTransform = hit.collider.transform;
            if (player != null && (hitTransform == player.transform || hitTransform.IsChildOf(player.transform)))
            {
                continue;
            }

            if (hit.distance < nearest)
            {
                nearest = hit.distance;
            }
        }

        if (nearest >= distance)
        {
            return desired;
        }

        return target + direction * Mathf.Max(minThirdPersonDistance, nearest - cameraCollisionPadding);
    }

    private void CalculateLobbyCamera(out Vector3 desired, out Quaternion desiredRotation)
    {
        desired = new Vector3(0f, 8.8f, 48f);
        var lookAt = new Vector3(0f, 2.2f, 14f);
        desiredRotation = Quaternion.LookRotation((lookAt - desired).normalized, Vector3.up);
    }
}
