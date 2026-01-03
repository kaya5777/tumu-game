using UnityEngine;
using TsumGame.Core;

namespace TsumGame.Board
{
    /// <summary>
    /// 物理ベースのボード境界を作成
    /// 床と壁でピースの落下範囲を制限
    /// </summary>
    public class BoardBoundary : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameConfig gameConfig;

        [Header("Boundary Settings")]
        [SerializeField] private float boundaryWidth = 8f;
        [SerializeField] private float boundaryHeight = 10f;
        [SerializeField] private float wallThickness = 0.5f;

        private GameObject floor;
        private GameObject leftWall;
        private GameObject rightWall;

        private void Awake()
        {
            CreateBoundaries();
        }

        /// <summary>
        /// 床と壁を作成
        /// </summary>
        private void CreateBoundaries()
        {
            // 床を作成
            floor = CreateBoundary("Floor",
                new Vector3(0f, -boundaryHeight / 2f - wallThickness / 2f, 0f),
                new Vector2(boundaryWidth + wallThickness * 2, wallThickness));

            // 左壁を作成
            leftWall = CreateBoundary("LeftWall",
                new Vector3(-boundaryWidth / 2f - wallThickness / 2f, 0f, 0f),
                new Vector2(wallThickness, boundaryHeight));

            // 右壁を作成
            rightWall = CreateBoundary("RightWall",
                new Vector3(boundaryWidth / 2f + wallThickness / 2f, 0f, 0f),
                new Vector2(wallThickness, boundaryHeight));

            Debug.Log("BoardBoundary: Boundaries created (Floor + Walls)");
        }

        /// <summary>
        /// 境界オブジェクトを作成
        /// </summary>
        private GameObject CreateBoundary(string name, Vector3 position, Vector2 size)
        {
            GameObject boundary = new GameObject(name);
            boundary.transform.SetParent(transform);
            boundary.transform.localPosition = position;

            // BoxCollider2D を追加
            BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
            collider.size = size;

            // 静的な物理オブジェクトとして設定
            Rigidbody2D rb = boundary.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            // デバッグ用：境界を視覚化（オプション）
            if (Application.isPlaying)
            {
                SpriteRenderer sr = boundary.AddComponent<SpriteRenderer>();
                sr.sprite = CreateBoxSprite();
                sr.color = new Color(0.3f, 0.3f, 0.3f, 0.3f); // 半透明グレー
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = size;
                sr.sortingOrder = -10;
            }

            return boundary;
        }

        /// <summary>
        /// 簡単な四角形スプライトを作成
        /// </summary>
        private Sprite CreateBoxSprite()
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        /// <summary>
        /// 床の高さを取得
        /// </summary>
        public float GetFloorY()
        {
            return -boundaryHeight / 2f;
        }

        /// <summary>
        /// ボードの幅を取得
        /// </summary>
        public float GetBoardWidth()
        {
            return boundaryWidth;
        }

#if UNITY_EDITOR
        // デバッグ用：境界を描画
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            // 床
            Vector3 floorPos = new Vector3(0f, -boundaryHeight / 2f - wallThickness / 2f, 0f);
            Gizmos.DrawWireCube(floorPos, new Vector3(boundaryWidth + wallThickness * 2, wallThickness, 0.1f));

            // 左壁
            Vector3 leftWallPos = new Vector3(-boundaryWidth / 2f - wallThickness / 2f, 0f, 0f);
            Gizmos.DrawWireCube(leftWallPos, new Vector3(wallThickness, boundaryHeight, 0.1f));

            // 右壁
            Vector3 rightWallPos = new Vector3(boundaryWidth / 2f + wallThickness / 2f, 0f, 0f);
            Gizmos.DrawWireCube(rightWallPos, new Vector3(wallThickness, boundaryHeight, 0.1f));
        }
#endif
    }
}
