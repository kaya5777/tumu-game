using UnityEngine;
using TMPro;
using TsumGame.Core;

namespace TsumGame.UI
{
    /// <summary>
    /// タイマー表示を管理するUIクラス
    /// </summary>
    public class TimerView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Warning Settings")]
        [SerializeField] private float warningThreshold = 10f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.red;

        private void OnEnable()
        {
            // イベント購読
            GameEvents.OnTimeChanged += OnTimeChanged;
        }

        private void OnDisable()
        {
            // イベント解除
            GameEvents.OnTimeChanged -= OnTimeChanged;
        }

        private void Start()
        {
            // 初期表示
            UpdateTimerDisplay(0f);
        }

        /// <summary>
        /// 時間変更時
        /// </summary>
        private void OnTimeChanged(float remainingTime)
        {
            UpdateTimerDisplay(remainingTime);
        }

        /// <summary>
        /// タイマー表示更新
        /// </summary>
        private void UpdateTimerDisplay(float time)
        {
            if (timerText == null) return;

            // MM:SS フォーマット
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);

            timerText.text = $"TIME: {minutes:00}:{seconds:00}";

            // 残り時間が少ない場合は警告色
            if (time <= warningThreshold && time > 0f)
            {
                timerText.color = warningColor;

                // 点滅効果（オプション）
                float blinkSpeed = 2f;
                float alpha = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI) + 1f) / 2f;
                Color currentColor = timerText.color;
                currentColor.a = Mathf.Lerp(0.5f, 1f, alpha);
                timerText.color = currentColor;
            }
            else
            {
                timerText.color = normalColor;
            }

            // 時間切れ
            if (time <= 0f)
            {
                timerText.color = warningColor;
                timerText.text = "TIME: 00:00";
            }
        }
    }
}
