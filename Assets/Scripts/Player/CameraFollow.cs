using UnityEngine;

/// <summary>
/// Smooth third-person camera follow used by the prototype scene.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -8f);
    [SerializeField] private float followSharpness = 8f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        var desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
        transform.LookAt(target.position + Vector3.up);
    }
}
