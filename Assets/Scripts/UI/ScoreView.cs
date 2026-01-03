using UnityEngine;
using TMPro;
using TsumGame.Core;

namespace TsumGame.UI
{
    /// <summary>
    /// スコア表示を管理するUIクラス
    /// </summary>
    public class ScoreView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI comboText;

        [Header("Animation Settings")]
        [SerializeField] private bool enableScoreAnimation = true;
        [SerializeField] private float animationDuration = 0.3f;

        private int displayedScore = 0;
        private int targetScore = 0;
        private float animationTimer = 0f;

        private void OnEnable()
        {
            // イベント購読
            GameEvents.OnScoreChanged += OnScoreChanged;
            GameEvents.OnComboChanged += OnComboChanged;
        }

        private void OnDisable()
        {
            // イベント解除
            GameEvents.OnScoreChanged -= OnScoreChanged;
            GameEvents.OnComboChanged -= OnComboChanged;
        }

        private void Start()
        {
            // 初期表示
            UpdateScoreDisplay(0);
            UpdateComboDisplay(0);
        }

        private void Update()
        {
            // スコアアニメーション
            if (enableScoreAnimation && displayedScore != targetScore)
            {
                animationTimer += Time.deltaTime;
                float t = Mathf.Clamp01(animationTimer / animationDuration);

                displayedScore = Mathf.RoundToInt(Mathf.Lerp(displayedScore, targetScore, t));
                UpdateScoreDisplay(displayedScore);

                if (t >= 1f)
                {
                    displayedScore = targetScore;
                    animationTimer = 0f;
                }
            }
        }

        /// <summary>
        /// スコア変更時
        /// </summary>
        private void OnScoreChanged(int newScore)
        {
            if (enableScoreAnimation)
            {
                targetScore = newScore;
                animationTimer = 0f;
            }
            else
            {
                displayedScore = newScore;
                UpdateScoreDisplay(newScore);
            }
        }

        /// <summary>
        /// コンボ変更時
        /// </summary>
        private void OnComboChanged(int comboCount)
        {
            UpdateComboDisplay(comboCount);
        }

        /// <summary>
        /// スコア表示更新
        /// </summary>
        private void UpdateScoreDisplay(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {score:N0}";
            }
        }

        /// <summary>
        /// コンボ表示更新
        /// </summary>
        private void UpdateComboDisplay(int comboCount)
        {
            if (comboText != null)
            {
                if (comboCount > 1)
                {
                    comboText.text = $"COMBO x{comboCount}!";
                    comboText.gameObject.SetActive(true);

                    // コンボ数に応じて色変更（オプション）
                    if (comboCount >= 10)
                        comboText.color = Color.red;
                    else if (comboCount >= 5)
                        comboText.color = Color.magenta;
                    else
                        comboText.color = Color.yellow;
                }
                else
                {
                    comboText.gameObject.SetActive(false);
                }
            }
        }
    }
}
