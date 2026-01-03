using System.Collections;
using UnityEngine;
using TsumGame.Core;

namespace TsumGame.Board
{
    /// <summary>
    /// 個々のピースを表す MonoBehaviour
    /// オブジェクトプールで再利用される
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Piece : MonoBehaviour
    {
        // プロパティ
        public PieceType PieceType { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public bool IsConnected { get; private set; }

        // コンポーネント参照
        private SpriteRenderer spriteRenderer;
        private CircleCollider2D circleCollider;
        private Rigidbody2D rb;

        // 元の色（ハイライト解除時に戻す）
        private Color originalColor;

        // 元のスケール（プレハブのスケールを保持）
        private Vector3 originalScale;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // プレハブのスケールを保存
            originalScale = transform.localScale;

            // CircleCollider2D を取得（なければ追加）
            circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider == null)
            {
                circleCollider = gameObject.AddComponent<CircleCollider2D>();
            }
            // circleCollider.radius = 0.45f;

            // Rigidbody2D を取得（なければ追加）
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }

            // 物理設定（段階的アプローチ：まずはKinematic）
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        /// <summary>
        /// ピースを初期化（プールから取得時に呼ばれる）
        /// </summary>
        public void Initialize(PieceType type, Vector2Int gridPosition, Sprite sprite, Color color)
        {
            PieceType = type;
            GridPosition = gridPosition;
            IsConnected = false;

            // 見た目の設定
            spriteRenderer.sprite = sprite;
            originalColor = Color.white; // スプライトを使うので白にする
            spriteRenderer.color = Color.white;

            // コライダーを有効化
            circleCollider.enabled = true;

            // 物理をリセット
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // スケールとアルファを初期値に戻す
            transform.localScale = originalScale;
            transform.rotation = Quaternion.identity;
            SetAlpha(1f);
        }

        /// <summary>
        /// グリッド位置を更新
        /// </summary>
        public void SetGridPosition(Vector2Int newPosition)
        {
            GridPosition = newPosition;
        }

        /// <summary>
        /// ハイライト表示の切り替え
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            IsConnected = highlighted;

            if (highlighted)
            {
                // ハイライト時は明るく＆大きく（ツムツムらしく！）
                spriteRenderer.color = originalColor * 1.3f;
                transform.localScale = originalScale * 1.15f;

                // 少し回転させる
                transform.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
            }
            else
            {
                // 元に戻す
                spriteRenderer.color = originalColor;
                transform.localScale = originalScale;
                transform.rotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// 消去アニメーション再生（ツムツムらしく回転＆縮小）
        /// </summary>
        public IEnumerator PlayDestroyAnimation(float duration)
        {
            // コライダーを無効化（アニメーション中は選択不可）
            circleCollider.enabled = false;

            // 物理を無効化
            rb.bodyType = RigidbodyType2D.Kinematic;

            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            float startRotation = transform.rotation.eulerAngles.z;
            float targetRotation = startRotation + 360f; // 1回転

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // スケールを0に縮小（少し弾む）
                float scale = Mathf.Lerp(1f, 0f, t);
                if (t < 0.3f)
                {
                    // 最初少し大きくなる
                    scale = Mathf.Lerp(1f, 1.2f, t / 0.3f);
                }
                else
                {
                    scale = Mathf.Lerp(1.2f, 0f, (t - 0.3f) / 0.7f);
                }
                transform.localScale = startScale * scale;

                // 回転しながら消える
                float rotation = Mathf.Lerp(startRotation, targetRotation, t);
                transform.rotation = Quaternion.Euler(0, 0, rotation);

                // 透明度も下げる
                SetAlpha(1f - t);

                yield return null;
            }

            // 最終値を設定
            transform.localScale = Vector3.zero;
            SetAlpha(0f);
        }

        /// <summary>
        /// 落下アニメーション（物理演算版）
        /// </summary>
        public IEnumerator PlayFallAnimation(Vector3 targetPosition, float duration)
        {
            // 物理演算を有効化
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;

            // ターゲット位置まで物理的に落下
            Vector3 startPosition = transform.position;
            float elapsed = 0f;
            bool reachedTarget = false;

            while (elapsed < duration && !reachedTarget)
            {
                elapsed += Time.deltaTime;

                // ターゲット位置に近づいたら停止
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    reachedTarget = true;
                    transform.position = targetPosition;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                }

                yield return null;
            }

            // 最終位置を設定
            if (!reachedTarget)
            {
                transform.position = targetPosition;
            }

            // 物理を無効化
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        /// <summary>
        /// プールに戻す前のクリーンアップ
        /// </summary>
        public void ResetForPool()
        {
            IsConnected = false;
            circleCollider.enabled = false;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.localScale = originalScale;
            transform.rotation = Quaternion.identity;
            SetAlpha(1f);
        }

        /// <summary>
        /// 着地時の揺れエフェクト（物理っぽさ）
        /// </summary>
        public void PlayLandEffect()
        {
            StartCoroutine(LandBounceEffect());
        }

        private IEnumerator LandBounceEffect()
        {
            Vector3 originalScale = transform.localScale;
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 少し潰れて戻る
                float scaleX = Mathf.Lerp(1.1f, 1f, t);
                float scaleY = Mathf.Lerp(0.9f, 1f, t);

                transform.localScale = new Vector3(
                    originalScale.x * scaleX,
                    originalScale.y * scaleY,
                    1f
                );

                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// 透明度を設定
        /// </summary>
        private void SetAlpha(float alpha)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }

        /// <summary>
        /// イージング関数（バウンス）
        /// </summary>
        private float EaseOutBounce(float t)
        {
            if (t < 1f / 2.75f)
            {
                return 7.5625f * t * t;
            }
            else if (t < 2f / 2.75f)
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < 2.5f / 2.75f)
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            }
        }
    }
}
