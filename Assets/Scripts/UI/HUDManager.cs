using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TaxiController taxi;
    [SerializeField] private Slider fuelBar;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI deliveryText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;

    private void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.OnScoreChanged += UpdateUI;
            gm.OnGameOver += ShowGameOver;
            gm.OnLevelComplete += ShowLevelComplete;
        }
    }

    private void Update()
    {
        if (taxi == null) return;

        if (fuelBar != null)
            fuelBar.value = taxi.FuelPercent;

        UpdateUI();
    }

    private void UpdateUI()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (scoreText != null) scoreText.text = $"Score: {gm.Score}";
        if (levelText != null) levelText.text = $"Level: {gm.Level}";
        if (deliveryText != null) deliveryText.text = $"{gm.DeliveredCount} / {gm.RequiredDeliveries}";
    }

    private void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void ShowLevelComplete()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
    }

    public void OnRestartButton()
    {
        GameManager.Instance?.RestartGame();
    }
}