using UnityEngine;

/// <summary>
/// Collectible item that disappears when the player touches it.
/// </summary>
public class CollectibleStar : MonoBehaviour
{
    [SerializeField] private StarCollectorGameManager gameManager;
    [SerializeField] private float spinSpeed = 90f;

    public void SetGameManager(StarCollectorGameManager manager)
    {
        gameManager = manager;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<StarCollectorGameManager>();
        }

        if (gameManager != null)
        {
            gameManager.CollectStar();
        }

        gameObject.SetActive(false);
    }
}
