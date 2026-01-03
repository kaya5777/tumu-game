using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TsumGame.Core;

namespace TsumGame.Board
{
    /// <summary>
    /// 完全物理ベースのボード管理
    /// グリッドなし、重力で自然に積み上がる
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private PiecePool piecePool;
        [SerializeField] private BoardBoundary boardBoundary;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnHeight = 5f;
        [SerializeField] private float spawnInterval = 0.3f;
        [SerializeField] private float initialSpawnInterval = 0f; // 初期生成の間隔（0で一気に）
        [SerializeField] private int initialPieceCount = 30;

        [Header("Touch Detection")]
        [SerializeField] private float touchToleranceMultiplier = 2.5f; // コライダー半径の何倍まで接触と判定するか
        [SerializeField] private float touchThreshold = 16f; // フォールバック用の固定距離

        [Header("Particle Settings")]
        [SerializeField] private bool enableParticles = true;

        // ピース管理（リストベース）
        private List<Piece> activePieces = new List<Piece>();

        // スポーン位置
        [Header("Spawn Range")]
        [SerializeField] private float spawnMinX = -8f;
        [SerializeField] private float spawnMaxX = 8f;

        // パーティクル用
        private ParticleSystem particlePrefab;

        // ランダム
        private System.Random random;

        private void Awake()
        {
            random = new System.Random();

            // パーティクルシステムを作成
            if (enableParticles)
            {
                CreateParticleSystem();
            }
        }

        /// <summary>
        /// ボードを初期化（ゲーム開始時）
        /// </summary>
        public void Initialize()
        {
            // 既存のピースをクリア
            ClearAllPieces();

            // 初期ピースを生成
            StartCoroutine(SpawnInitialPieces());
        }

        /// <summary>
        /// 初期ピースを順次生成
        /// </summary>
        private IEnumerator SpawnInitialPieces()
        {
            for (int i = 0; i < initialPieceCount; i++)
            {
                // 高さをランダムにして散らばらせる
                SpawnPieceWithRandomHeight();

                if (initialSpawnInterval > 0)
                {
                    yield return new WaitForSeconds(initialSpawnInterval);
                }
            }

            Debug.Log($"BoardManager: Spawned {initialPieceCount} initial pieces");
        }

        /// <summary>
        /// ランダムな高さでピースを生成
        /// </summary>
        private void SpawnPieceWithRandomHeight()
        {
            // ランダムな横位置
            float randomX = Random.Range(spawnMinX, spawnMaxX);

            // ランダムな高さ（spawnHeight ± 3f の範囲）
            float randomY = spawnHeight + Random.Range(-3f, 3f);

            Vector3 spawnPos = new Vector3(randomX, randomY, 0f);

            // ランダムなピースタイプ
            PieceType randomType = GetRandomPieceType();

            // ピースを生成
            Piece piece = piecePool.Get(randomType, Vector2Int.zero, spawnPos);
            piece.transform.SetParent(transform);

            // 物理を有効化（自動で落下）
            var rb = piece.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 2f;
            }

            // リストに追加
            activePieces.Add(piece);
        }

        /// <summary>
        /// ピースを1つ生成（物理的に落下）
        /// </summary>
        public void SpawnPiece()
        {
            // ランダムな横位置
            float randomX = Random.Range(spawnMinX, spawnMaxX);
            Vector3 spawnPos = new Vector3(randomX, spawnHeight, 0f);

            // ランダムなピースタイプ
            PieceType randomType = GetRandomPieceType();

            // ピースを生成
            Piece piece = piecePool.Get(randomType, Vector2Int.zero, spawnPos);
            piece.transform.SetParent(transform);

            // 物理を有効化（自動で落下）
            var rb = piece.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 2f;
            }

            // リストに追加
            activePieces.Add(piece);
        }

        /// <summary>
        /// ピースを消去
        /// </summary>
        public IEnumerator ErasePieces(List<Piece> pieces, float animationDuration)
        {
            foreach (Piece piece in pieces)
            {
                // リストから削除
                activePieces.Remove(piece);

                // パーティクルエフェクト再生
                if (enableParticles && particlePrefab != null)
                {
                    PlayParticleAt(piece.transform.position, gameConfig.GetColorForPieceType(piece.PieceType));
                }

                // 消去アニメーション
                StartCoroutine(piece.PlayDestroyAnimation(animationDuration));
            }

            // アニメーション完了まで待機
            yield return new WaitForSeconds(animationDuration);

            // プールに返却
            foreach (Piece piece in pieces)
            {
                piecePool.Return(piece);
            }

            // 消去した分を補充
            StartCoroutine(RefillPieces(pieces.Count));
        }

        /// <summary>
        /// 消去した分のピースを補充
        /// </summary>
        private IEnumerator RefillPieces(int count)
        {
            // 少し待ってから補充
            yield return new WaitForSeconds(0.3f);

            for (int i = 0; i < count; i++)
            {
                SpawnPiece();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        /// <summary>
        /// 全ピースをシャッフル（ピースタイプをランダム化）
        /// </summary>
        public void ShuffleAllPieces()
        {
            Debug.Log("BoardManager: Shuffling all pieces");

            foreach (Piece piece in activePieces)
            {
                if (piece == null) continue;

                // ランダムな新しいピースタイプを取得
                PieceType newType = GetRandomPieceType();

                // 新しいスプライトと色を取得
                Sprite newSprite = gameConfig.GetSpriteForPieceType(newType);
                Color newColor = gameConfig.GetColorForPieceType(newType);

                // ピースを再初期化（位置はそのまま）
                piece.Initialize(newType, piece.GridPosition, newSprite, newColor);

                // 物理を再度有効化（Initialize で無効化されるため）
                var rb = piece.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.gravityScale = 2f;
                }
            }

            Debug.Log($"BoardManager: Shuffled {activePieces.Count} pieces");
        }

        /// <summary>
        /// 指定位置のピースを取得（物理的な範囲チェック）
        /// </summary>
        public Piece GetPieceAt(Vector3 worldPos)
        {
            // 一番近いピースを探す
            Piece closestPiece = null;
            float closestDistance = 0.5f; // 検出範囲

            foreach (Piece piece in activePieces)
            {
                float distance = Vector3.Distance(piece.transform.position, worldPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPiece = piece;
                }
            }

            return closestPiece;
        }

        /// <summary>
        /// 2つのピースが接触しているかチェック
        /// </summary>
        public bool ArePiecesTouching(Piece piece1, Piece piece2)
        {
            if (piece1 == null || piece2 == null) return false;

            float distance = Vector3.Distance(piece1.transform.position, piece2.transform.position);

            // ピースのスケールとコライダーサイズを考慮した判定
            CircleCollider2D collider1 = piece1.GetComponent<CircleCollider2D>();
            CircleCollider2D collider2 = piece2.GetComponent<CircleCollider2D>();

            if (collider1 != null && collider2 != null)
            {
                // 実際のコライダー半径（スケール考慮）
                float radius1 = collider1.radius * piece1.transform.localScale.x;
                float radius2 = collider2.radius * piece2.transform.localScale.x;

                // 2つのピースが接触する距離 + 余裕（物理演算の隙間を考慮）
                float maxDistance = (radius1 + radius2) * touchToleranceMultiplier;

                return distance < maxDistance;
            }

            // フォールバック
            return distance < touchThreshold;
        }

        /// <summary>
        /// すべてのピースをクリア
        /// </summary>
        private void ClearAllPieces()
        {
            foreach (Piece piece in activePieces)
            {
                if (piece != null)
                {
                    piecePool.Return(piece);
                }
            }

            activePieces.Clear();
        }

        /// <summary>
        /// ランダムなピースタイプを取得
        /// </summary>
        private PieceType GetRandomPieceType()
        {
            int typeCount = System.Enum.GetValues(typeof(PieceType)).Length;
            int randomIndex = random.Next(typeCount);
            return (PieceType)randomIndex;
        }

        /// <summary>
        /// パーティクルシステムを作成
        /// </summary>
        private void CreateParticleSystem()
        {
            GameObject particleObj = new GameObject("PieceParticle");
            particleObj.transform.SetParent(transform);

            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 0.8f;
            main.startSpeed = 3f;
            main.startSize = 0.3f;
            main.maxParticles = 100;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 100;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

            particlePrefab = ps;
        }

        /// <summary>
        /// 指定位置でパーティクルを再生
        /// </summary>
        private void PlayParticleAt(Vector3 position, Color color)
        {
            if (particlePrefab == null) return;

            position.z = -2f;
            ParticleSystem ps = Instantiate(particlePrefab, position, Quaternion.identity);
            ps.gameObject.SetActive(true);

            var main = ps.main;
            main.startColor = color;

            ps.Play();
            Destroy(ps.gameObject, 2f);
        }

        private void OnDestroy()
        {
            ClearAllPieces();
        }
    }
}
