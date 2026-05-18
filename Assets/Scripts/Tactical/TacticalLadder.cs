using UnityEngine;

public class TacticalLadder : MonoBehaviour
{
    [SerializeField] private float groundY = 1.04f;
    [SerializeField] private float topY = 5.14f;
    [SerializeField] private int topFloor = 2;

    public string Prompt => "按 F 使用爬梯";

    public void Configure(float targetTopY, int targetTopFloor)
    {
        topY = targetTopY;
        topFloor = Mathf.Max(2, targetTopFloor);
    }

    public int Use(Transform player)
    {
        var controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        var position = player.position;
        var goingUp = position.y < topY - 0.8f;
        position.y = goingUp ? topY : groundY;
        position.x = transform.position.x + 1.2f;
        position.z = transform.position.z - 1.2f;
        player.position = position;

        if (controller != null)
        {
            controller.enabled = true;
        }

        return goingUp ? topFloor : 1;
    }
}
