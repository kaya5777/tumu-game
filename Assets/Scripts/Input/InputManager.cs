using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TsumGame.Core;
using TsumGame.Board;

namespace TsumGame.Input
{
    /// <summary>
    /// 物理ベースのプレイヤー入力管理
    /// Raycastでピースを検出し、物理的な接触で連結判定
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private Camera mainCamera;

        [Header("Input Settings")]
        [SerializeField] private LayerMask pieceLayerMask;
        [SerializeField] private float touchDistance = 1.2f; // ピース間の接触判定距離

        [Header("Visual Feedback")]
        [SerializeField] private LineRenderer connectionLine;
        [SerializeField] private float lineWidth = 0.2f;
        [SerializeField] private Color lineColor = new Color(1f, 1f, 0f, 0.8f);

        // Input Actions
        private GameInput gameInput;
        private InputAction touchAction;
        private InputAction positionAction;

        // ドラッグ状態
        private bool isDragging = false;
        private List<Piece> currentPieces = new List<Piece>();
        private PieceType? currentPieceType = null;

        // イベント
        public System.Action<List<Piece>> OnDragEnd;

        // 入力有効化フラグ
        private bool inputEnabled = true;

        private void Awake()
        {
            // メインカメラの自動取得
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            // Input System セットアップ
            gameInput = new GameInput();
            touchAction = gameInput.Puzzle.Touch;
            positionAction = gameInput.Puzzle.Position;

            // LineRenderer セットアップ
            SetupLineRenderer();
        }

        /// <summary>
        /// LineRenderer の初期設定
        /// </summary>
        private void SetupLineRenderer()
        {
            if (connectionLine == null)
            {
                // LineRenderer が未設定の場合は自動作成
                GameObject lineObj = new GameObject("ConnectionLine");
                lineObj.transform.SetParent(transform);
                connectionLine = lineObj.AddComponent<LineRenderer>();
            }

            // LineRenderer 設定
            connectionLine.startWidth = lineWidth;
            connectionLine.endWidth = lineWidth;
            connectionLine.material = new Material(Shader.Find("Sprites/Default"));
            connectionLine.startColor = lineColor;
            connectionLine.endColor = lineColor;
            connectionLine.positionCount = 0;
            connectionLine.sortingOrder = 10;
            connectionLine.useWorldSpace = true;
            connectionLine.numCornerVertices = 5;
            connectionLine.numCapVertices = 5;
        }

        private void OnEnable()
        {
            gameInput.Enable();

            // コールバック登録
            touchAction.started += OnTouchStarted;
            touchAction.canceled += OnTouchCanceled;
        }

        private void OnDisable()
        {
            touchAction.started -= OnTouchStarted;
            touchAction.canceled -= OnTouchCanceled;

            gameInput.Disable();
        }

        private void Update()
        {
            if (isDragging && inputEnabled)
            {
                UpdateDrag();
            }
        }

        /// <summary>
        /// タッチ/クリック開始
        /// </summary>
        private void OnTouchStarted(InputAction.CallbackContext context)
        {
            if (!inputEnabled) return;

            Vector2 screenPos = positionAction.ReadValue<Vector2>();
            Piece piece = GetPieceAtScreenPosition(screenPos);

            if (piece != null)
            {
                // ドラッグ開始
                isDragging = true;
                currentPieces.Clear();

                currentPieces.Add(piece);
                currentPieceType = piece.PieceType;

                piece.SetHighlight(true);

                // 連結線を更新
                UpdateConnectionLine();
            }
        }

        /// <summary>
        /// ドラッグ中の更新
        /// </summary>
        private void UpdateDrag()
        {
            Vector2 screenPos = positionAction.ReadValue<Vector2>();
            Piece piece = GetPieceAtScreenPosition(screenPos);

            if (piece == null) return;

            // 既にパスに含まれている場合
            if (currentPieces.Contains(piece))
            {
                // 1つ前に戻る処理
                int index = currentPieces.IndexOf(piece);
                if (index < currentPieces.Count - 1)
                {
                    // 後続のピースを削除
                    for (int i = currentPieces.Count - 1; i > index; i--)
                    {
                        currentPieces[i].SetHighlight(false);
                        currentPieces.RemoveAt(i);
                    }

                    // 連結線を更新
                    UpdateConnectionLine();
                }
                return;
            }

            // 新しいピースを追加
            if (IsValidConnection(piece))
            {
                currentPieces.Add(piece);
                piece.SetHighlight(true);

                // 連結線を更新
                UpdateConnectionLine();
            }
        }

        /// <summary>
        /// タッチ/クリック終了
        /// </summary>
        private void OnTouchCanceled(InputAction.CallbackContext context)
        {
            if (!isDragging) return;

            isDragging = false;

            // 最小ピース数チェック
            if (currentPieces.Count >= gameConfig.minPiecesToErase)
            {
                // 消去イベント発火
                OnDragEnd?.Invoke(new List<Piece>(currentPieces));
            }
            else
            {
                // 不十分な場合はハイライト解除
                foreach (Piece piece in currentPieces)
                {
                    piece.SetHighlight(false);
                }
            }

            // クリア
            currentPieces.Clear();
            currentPieceType = null;

            // 連結線をクリア
            ClearConnectionLine();
        }

        /// <summary>
        /// スクリーン座標のピースを取得（Raycast使用）
        /// </summary>
        private Piece GetPieceAtScreenPosition(Vector2 screenPos)
        {
            // スクリーン座標 → ワールド座標
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z));
            worldPos.z = 0f;

            // Raycast2D でピースを検出
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, pieceLayerMask);

            if (hit.collider != null)
            {
                return hit.collider.GetComponent<Piece>();
            }

            // Raycast で見つからない場合は近接検索
            return boardManager.GetPieceAt(worldPos);
        }

        /// <summary>
        /// 接続が有効かチェック（物理的な距離ベース）
        /// </summary>
        private bool IsValidConnection(Piece newPiece)
        {
            if (currentPieces.Count == 0)
                return false;

            // 最後に追加されたピースとの距離チェック
            Piece lastPiece = currentPieces[currentPieces.Count - 1];

            // 物理的に接触しているかチェック
            if (!boardManager.ArePiecesTouching(lastPiece, newPiece))
                return false;

            // 同じピースタイプかチェック
            if (newPiece.PieceType != currentPieceType)
                return false;

            return true;
        }

        /// <summary>
        /// 入力を有効/無効化
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;

            if (!enabled && isDragging)
            {
                // ドラッグ中の場合は強制終了
                foreach (Piece piece in currentPieces)
                {
                    piece.SetHighlight(false);
                }

                isDragging = false;
                currentPieces.Clear();
                currentPieceType = null;

                // 連結線をクリア
                ClearConnectionLine();
            }
        }

        /// <summary>
        /// 連結線を更新（物理位置ベース）
        /// </summary>
        private void UpdateConnectionLine()
        {
            if (connectionLine == null || currentPieces.Count == 0)
            {
                ClearConnectionLine();
                return;
            }

            // LineRenderer の位置を更新
            connectionLine.positionCount = currentPieces.Count;

            for (int i = 0; i < currentPieces.Count; i++)
            {
                Vector3 worldPos = currentPieces[i].transform.position;
                worldPos.z = -1; // カメラより手前に表示
                connectionLine.SetPosition(i, worldPos);
            }

            connectionLine.enabled = true;
        }

        /// <summary>
        /// 連結線をクリア
        /// </summary>
        private void ClearConnectionLine()
        {
            if (connectionLine != null)
            {
                connectionLine.positionCount = 0;
                connectionLine.enabled = false;
            }
        }

        /// <summary>
        /// デバッグ用: 現在のパスを描画
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || currentPieces.Count == 0) return;

            Gizmos.color = Color.cyan;

            for (int i = 0; i < currentPieces.Count - 1; i++)
            {
                Vector3 from = currentPieces[i].transform.position;
                Vector3 to = currentPieces[i + 1].transform.position;

                Gizmos.DrawLine(from, to);
            }
        }
    }
}
