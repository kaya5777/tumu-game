using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TsumGame.Board;

namespace TsumGame.Core
{
    /// <summary>
    /// 物理ベースのゲーム全体管理クラス
    /// BoardManager + InputManager を使用
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private Input.InputManager inputManager;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private ScoreManager scoreManager;

        [Header("Debug")]
        [SerializeField] private bool autoStartGame = true;

        private GameState currentState = GameState.Idle;

        public GameState CurrentState => currentState;

        private void Awake()
        {
            // シングルトン設定
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // イベント登録
            if (inputManager != null)
            {
                inputManager.OnDragEnd += HandleDragEnd;
            }

            GameEvents.OnGameOver += HandleGameOver;

            // 自動開始
            if (autoStartGame)
            {
                StartGame();
            }
        }

        private void OnDestroy()
        {
            // イベント解除
            if (inputManager != null)
            {
                inputManager.OnDragEnd -= HandleDragEnd;
            }

            GameEvents.OnGameOver -= HandleGameOver;
        }

        /// <summary>
        /// ゲーム開始
        /// </summary>
        public void StartGame()
        {
            // ボード初期化
            boardManager.Initialize();

            // スコア・タイマーリセット
            scoreManager.ResetScore();
            timeManager.StartTimer();

            // 状態変更
            ChangeState(GameState.Playing);

            Debug.Log("Game Started!");
        }

        /// <summary>
        /// ゲーム状態を変更
        /// </summary>
        private void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            GameEvents.TriggerGameStateChanged(newState);

            // 状態に応じた処理
            switch (newState)
            {
                case GameState.Playing:
                case GameState.Idle:
                    inputManager.SetInputEnabled(true);
                    break;

                case GameState.Connecting:
                case GameState.Erasing:
                case GameState.Falling:
                    inputManager.SetInputEnabled(false);
                    break;

                case GameState.GameOver:
                    inputManager.SetInputEnabled(false);
                    timeManager.PauseTimer();
                    break;
            }

            Debug.Log($"Game State: {newState}");
        }

        /// <summary>
        /// ドラッグ終了時の処理
        /// </summary>
        private void HandleDragEnd(List<Piece> pieces)
        {
            if (pieces == null || pieces.Count < gameConfig.minPiecesToErase)
                return;

            // コルーチンで順次処理
            StartCoroutine(HandlePieceErasureSequence(pieces));
        }

        /// <summary>
        /// ピース消去シーケンス
        /// </summary>
        private IEnumerator HandlePieceErasureSequence(List<Piece> pieces)
        {
            // 1. 消去状態に変更
            ChangeState(GameState.Erasing);

            // 2. スコア加算
            scoreManager.AddScore(pieces.Count);

            // 3. 消去アニメーション
            yield return boardManager.ErasePieces(pieces, gameConfig.pieceEraseAnimationDuration);

            // 4. アイドル状態に戻る（物理ベースは自動的に補充されるのでFalling状態不要）
            ChangeState(GameState.Idle);

            Debug.Log("Erase sequence complete!");
        }

        /// <summary>
        /// ゲームオーバー処理
        /// </summary>
        private void HandleGameOver()
        {
            ChangeState(GameState.GameOver);

            Debug.Log($"Game Over! Final Score: {scoreManager.CurrentScore}");

            // TODO: リザルト画面表示
        }

        /// <summary>
        /// ゲームリスタート
        /// </summary>
        public void RestartGame()
        {
            // コンボリセット
            scoreManager.ResetCombo();

            // 再スタート
            StartGame();
        }

        /// <summary>
        /// ピースをシャッフル（UIボタンから呼ばれる）
        /// </summary>
        public void ShufflePieces()
        {
            if (currentState != GameState.Playing && currentState != GameState.Idle)
            {
                Debug.LogWarning("Cannot shuffle during this state: " + currentState);
                return;
            }

            boardManager.ShuffleAllPieces();
            Debug.Log("GameManager: Pieces shuffled!");
        }

#if UNITY_EDITOR
        // デバッグ用
        [ContextMenu("Start Game")]
        private void DebugStartGame()
        {
            StartGame();
        }

        [ContextMenu("Restart Game")]
        private void DebugRestartGame()
        {
            RestartGame();
        }

        [ContextMenu("Trigger Game Over")]
        private void DebugGameOver()
        {
            GameEvents.TriggerGameOver();
        }
#endif
    }
}
