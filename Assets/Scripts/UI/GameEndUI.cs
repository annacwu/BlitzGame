using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndUI : MonoBehaviour
{
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private Transform summaryContainer;
    [SerializeField] private GameObject scoreSummaryTextPrefab;

    public void Show()
    {
        endGamePanel.SetActive(true);
    }

    public void Hide()
    {
        endGamePanel.SetActive(false);
    }
    public void AddScoreSummary(ulong clientID, int score)
    {
        GameObject scoreEntry = Instantiate(scoreSummaryTextPrefab, summaryContainer);
        scoreEntry.GetComponent<TMPro.TMP_Text>().text = $"Player {clientID}'s score: {score}";
    }

}
