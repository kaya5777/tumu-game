using System;
using UnityEngine;

namespace TsumGame.Core
{
    /// <summary>
    /// ゲーム全体で使用する静的イベントクラス
    /// UI とゲームロジックの疎結合を実現
    /// </summary>
    public static class GameEvents
    {
        /// <summary>スコアが変更された時</summary>
        public static event Action<int> OnScoreChanged;

        /// <summary>タイマーが更新された時</summary>
        public static event Action<float> OnTimeChanged;

        /// <summary>ゲーム状態が変更された時</summary>
        public static event Action<GameState> OnGameStateChanged;

        /// <summary>コンボが変更された時</summary>
        public static event Action<int> OnComboChanged;

        /// <summary>ピースが消去された時</summary>
        public static event Action<int> OnPiecesErased;

        /// <summary>ゲームオーバー時</summary>
        public static event Action OnGameOver;

        // イベント発火メソッド
        public static void TriggerScoreChanged(int newScore)
        {
            OnScoreChanged?.Invoke(newScore);
        }

        public static void TriggerTimeChanged(float remainingTime)
        {
            OnTimeChanged?.Invoke(remainingTime);
        }

        public static void TriggerGameStateChanged(GameState newState)
        {
            OnGameStateChanged?.Invoke(newState);
        }

        public static void TriggerComboChanged(int comboCount)
        {
            OnComboChanged?.Invoke(comboCount);
        }

        public static void TriggerPiecesErased(int pieceCount)
        {
            OnPiecesErased?.Invoke(pieceCount);
        }

        public static void TriggerGameOver()
        {
            OnGameOver?.Invoke();
        }

        /// <summary>
        /// すべてのイベントリスナーをクリア（シーン切り替え時などに使用）
        /// メモリリーク防止
        /// </summary>
        public static void ClearAllEvents()
        {
            OnScoreChanged = null;
            OnTimeChanged = null;
            OnGameStateChanged = null;
            OnComboChanged = null;
            OnPiecesErased = null;
            OnGameOver = null;
        }
    }
}
