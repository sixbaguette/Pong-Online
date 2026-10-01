using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    private BallServer m_ballServer;

    private void Update()
    {
        if (m_ballServer == null)
        {
            m_ballServer = FindAnyObjectByType<BallServer>();
            return;
        }

        if (scoreText != null)
        {
            scoreText.text = $"{m_ballServer.ScoreLeft.Value} - {m_ballServer.ScoreRight.Value}";
        }
    }
}