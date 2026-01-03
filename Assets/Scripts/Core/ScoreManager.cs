using UnityEngine;
using System.Collections;

namespace TsumGame.Core
{
    /// <summary>
    /// スコアとコンボを管理するクラス
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameConfig gameConfig;

        private int currentScore = 0;
        private int currentCombo = 0;
        private Coroutine comboResetCoroutine;

        public int CurrentScore => currentScore;
        public int CurrentCombo => currentCombo;

        /// <summary>
        /// スコアを追加
        /// </summary>
        public void AddScore(int pieceCount)
        {
            if (pieceCount < gameConfig.minPiecesToErase)
                return;

            // コンボを増加
            IncrementCombo();

            // スコア計算
            float comboMultiplier = gameConfig.GetComboMultiplier(currentCombo);
            int baseScore = gameConfig.baseScorePerPiece * pieceCount;

            // 長い連結ボーナス（ツムツムらしさ！）
            float chainBonus = CalculateChainBonus(pieceCount);

            int scoreToAdd = Mathf.RoundToInt(baseScore * comboMultiplier * chainBonus);

            currentScore += scoreToAdd;

            // 特別なメッセージ表示（7個以上）
            if (pieceCount >= 7)
            {
                Debug.Log($"<color=yellow>★EXCELLENT★ {pieceCount}チェーン！ボーナス x{chainBonus:F1}！</color>");
            }

            // イベント発火
            GameEvents.TriggerScoreChanged(currentScore);
            GameEvents.TriggerPiecesErased(pieceCount);

            // コンボリセットタイマー開始
            StartComboResetTimer();
        }

        /// <summary>
        /// 連結数に応じたボーナス倍率を計算
        /// </summary>
        private float CalculateChainBonus(int pieceCount)
        {
            if (pieceCount >= 15)
                return 5.0f;  // 15個以上: 5倍！
            else if (pieceCount >= 12)
                return 4.0f;  // 12-14個: 4倍
            else if (pieceCount >= 10)
                return 3.0f;  // 10-11個: 3倍
            else if (pieceCount >= 7)
                return 2.0f;  // 7-9個: 2倍（ツムツム風）
            else if (pieceCount >= 5)
                return 1.5f;  // 5-6個: 1.5倍
            else
                return 1.0f;  // 3-4個: 通常
        }

        /// <summary>
        /// コンボを増加
        /// </summary>
        private void IncrementCombo()
        {
            currentCombo++;
            GameEvents.TriggerComboChanged(currentCombo);
        }

        /// <summary>
        /// コンボをリセット
        /// </summary>
        public void ResetCombo()
        {
            currentCombo = 0;
            GameEvents.TriggerComboChanged(currentCombo);

            // リセットコルーチンを停止
            if (comboResetCoroutine != null)
            {
                StopCoroutine(comboResetCoroutine);
                comboResetCoroutine = null;
            }
        }

        /// <summary>
        /// スコアをリセット
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            currentCombo = 0;

            GameEvents.TriggerScoreChanged(currentScore);
            GameEvents.TriggerComboChanged(currentCombo);

            if (comboResetCoroutine != null)
            {
                StopCoroutine(comboResetCoroutine);
                comboResetCoroutine = null;
            }
        }

        /// <summary>
        /// コンボリセットタイマー開始
        /// </summary>
        private void StartComboResetTimer()
        {
            // 既存のタイマーを停止
            if (comboResetCoroutine != null)
            {
                StopCoroutine(comboResetCoroutine);
            }

            // 新しいタイマー開始
            comboResetCoroutine = StartCoroutine(ComboResetTimerCoroutine());
        }

        /// <summary>
        /// コンボリセットタイマーコルーチン
        /// </summary>
        private IEnumerator ComboResetTimerCoroutine()
        {
            yield return new WaitForSeconds(gameConfig.comboResetTime);

            // タイムアウト: コンボリセット
            currentCombo = 0;
            GameEvents.TriggerComboChanged(currentCombo);

            comboResetCoroutine = null;
        }
    }
}
