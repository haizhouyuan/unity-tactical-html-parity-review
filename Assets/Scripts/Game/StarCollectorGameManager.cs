using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tracks score and the win message for Star Collector Adventure.
/// </summary>
public class StarCollectorGameManager : MonoBehaviour
{
    [SerializeField] private int totalStars = 10;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text winText;

    private int collectedStars;

    public void Configure(Text score, Text win, int total)
    {
        scoreText = score;
        winText = win;
        totalStars = total;
        ResetScore();
    }

    private void Start()
    {
        ResetScore();
    }

    public void CollectStar()
    {
        collectedStars = Mathf.Min(collectedStars + 1, totalStars);
        UpdateUi();

        if (collectedStars >= totalStars && winText != null)
        {
            winText.gameObject.SetActive(true);
        }
    }

    private void ResetScore()
    {
        collectedStars = 0;

        if (winText != null)
        {
            winText.gameObject.SetActive(false);
        }

        UpdateUi();
    }

    private void UpdateUi()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + collectedStars + " / " + totalStars;
        }
    }
}
