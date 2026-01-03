using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TsumGame.Core;

namespace TsumGame.UI
{
    /// <summary>
    /// ゲームオーバー画面を管理するUIクラス
    /// MVPでは簡易実装
    /// </summary>
    public class GameOverView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button postToXButton;

        [Header("Settings")]
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private string gameTitle = "ツムツムゲーム";
        [SerializeField] private string gameUrl = "https://yourusername.github.io/yourrepo/";

        private int finalScore;

        private void OnEnable()
        {
            // イベント購読
            GameEvents.OnGameOver += OnGameOver;

            // ボタンイベント
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            if (postToXButton != null)
            {
                postToXButton.onClick.AddListener(OnPostToXButtonClicked);
            }
        }

        private void OnDisable()
        {
            // イベント解除
            GameEvents.OnGameOver -= OnGameOver;

            // ボタンイベント解除
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            }

            if (postToXButton != null)
            {
                postToXButton.onClick.RemoveListener(OnPostToXButtonClicked);
            }
        }

        private void Start()
        {
            // 初期状態: 非表示
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        /// <summary>
        /// ゲームオーバー時
        /// </summary>
        private void OnGameOver()
        {
            ShowGameOverPanel();
        }

        /// <summary>
        /// ゲームオーバーパネル表示
        /// </summary>
        private void ShowGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            // 最終スコア取得・保存
            if (scoreManager != null)
            {
                finalScore = scoreManager.CurrentScore;
            }

            // 最終スコア表示
            if (finalScoreText != null)
            {
                finalScoreText.text = $"GAME OVER\n\nScore: {finalScore:N0}";
            }

            Debug.Log($"GameOverView: Showing game over with score {finalScore}");
        }

        /// <summary>
        /// リスタートボタンクリック時
        /// </summary>
        private void OnRestartButtonClicked()
        {
            // ゲームオーバーパネル非表示
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // ゲームリスタート
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        /// <summary>
        /// Xにポストボタンクリック時
        /// </summary>
        private void OnPostToXButtonClicked()
        {
            string tweetText = $"{gameTitle}で{finalScore:N0}点獲得しました！";

            // Twitter Intent URL を作成
            string twitterUrl = $"https://twitter.com/intent/tweet?text={UnityEngine.Networking.UnityWebRequest.EscapeURL(tweetText)}&url={UnityEngine.Networking.UnityWebRequest.EscapeURL(gameUrl)}";

            // ブラウザで開く
            Application.OpenURL(twitterUrl);

            Debug.Log($"GameOverView: Opening X (Twitter) with score {finalScore}");
        }
    }
}

