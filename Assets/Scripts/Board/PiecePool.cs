using UnityEngine;
using UnityEngine.Pool;
using TsumGame.Core;

namespace TsumGame.Board
{
    /// <summary>
    /// Piece のオブジェクトプール
    /// Unity の ObjectPool を使用して GC 圧力を軽減
    /// </summary>
    public class PiecePool : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Piece piecePrefab;
        [SerializeField] private GameConfig gameConfig;

        [Header("Pool Settings")]
        [SerializeField] private Transform poolContainer;

        private ObjectPool<Piece> pool;

        private void Awake()
        {
            // プールコンテナがない場合は自動作成
            if (poolContainer == null)
            {
                poolContainer = new GameObject("PoolContainer").transform;
                poolContainer.SetParent(transform);
            }

            // ObjectPool 初期化
            pool = new ObjectPool<Piece>(
                createFunc: CreatePiece,
                actionOnGet: OnGetPiece,
                actionOnRelease: OnReleasePiece,
                actionOnDestroy: OnDestroyPiece,
                collectionCheck: true,
                defaultCapacity: gameConfig.poolInitialSize,
                maxSize: gameConfig.poolMaxSize
            );

            // 初期プリウォーム（事前生成）
            PrewarmPool(gameConfig.poolInitialSize);
        }

        /// <summary>
        /// プールから Piece を取得
        /// </summary>
        public Piece Get(PieceType type, Vector2Int gridPosition, Vector3 worldPosition)
        {
            Piece piece = pool.Get();

            // 初期化
            Sprite sprite = gameConfig.GetSpriteForPieceType(type);
            Color color = gameConfig.GetColorForPieceType(type);
            piece.Initialize(type, gridPosition, sprite, color);
            piece.transform.position = worldPosition;

            return piece;
        }

        /// <summary>
        /// プールに Piece を返却
        /// </summary>
        public void Return(Piece piece)
        {
            if (piece != null)
            {
                pool.Release(piece);
            }
        }

        /// <summary>
        /// プールをプリウォーム（事前生成）
        /// </summary>
        private void PrewarmPool(int count)
        {
            Piece[] tempPieces = new Piece[count];

            for (int i = 0; i < count; i++)
            {
                tempPieces[i] = pool.Get();
            }

            for (int i = 0; i < count; i++)
            {
                pool.Release(tempPieces[i]);
            }
        }

        // ObjectPool コールバック

        private Piece CreatePiece()
        {
            Piece piece = Instantiate(piecePrefab, poolContainer);
            piece.gameObject.SetActive(false);
            return piece;
        }

        private void OnGetPiece(Piece piece)
        {
            piece.gameObject.SetActive(true);
        }

        private void OnReleasePiece(Piece piece)
        {
            piece.ResetForPool();
            piece.gameObject.SetActive(false);
            piece.transform.SetParent(poolContainer);
        }

        private void OnDestroyPiece(Piece piece)
        {
            if (piece != null)
            {
                Destroy(piece.gameObject);
            }
        }

        private void OnDestroy()
        {
            // プールのクリーンアップ
            pool?.Clear();
        }
    }
}
