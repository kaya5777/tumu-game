using UnityEngine;

namespace TsumGame.Core
{
    /// <summary>
    /// タイマーを管理するクラス
    /// 60秒のカウントダウン
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameConfig gameConfig;

        private float remainingTime;
        private bool isRunning = false;

        public float RemainingTime => remainingTime;
        public bool IsRunning => isRunning;

        private void Update()
        {
            if (!isRunning) return;

            remainingTime -= Time.deltaTime;

            // イベント発火
            GameEvents.TriggerTimeChanged(remainingTime);

            // 時間切れチェック
            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                isRunning = false;
                GameEvents.TriggerGameOver();
            }
        }

        /// <summary>
        /// タイマー開始
        /// </summary>
        public void StartTimer()
        {
            remainingTime = gameConfig.gameTime;
            isRunning = true;
            GameEvents.TriggerTimeChanged(remainingTime);
        }

        /// <summary>
        /// タイマー一時停止
        /// </summary>
        public void PauseTimer()
        {
            isRunning = false;
        }

        /// <summary>
        /// タイマー再開
        /// </summary>
        public void ResumeTimer()
        {
            isRunning = true;
        }

        /// <summary>
        /// タイマーリセット
        /// </summary>
        public void ResetTimer()
        {
            remainingTime = gameConfig.gameTime;
            isRunning = false;
            GameEvents.TriggerTimeChanged(remainingTime);
        }

        /// <summary>
        /// 時間を追加（ボーナスなど）
        /// </summary>
        public void AddTime(float seconds)
        {
            remainingTime += seconds;
            GameEvents.TriggerTimeChanged(remainingTime);
        }
    }
}
