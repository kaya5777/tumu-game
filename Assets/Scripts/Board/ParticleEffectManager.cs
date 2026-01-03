using UnityEngine;
using TsumGame.Core;

namespace TsumGame.Board
{
    /// <summary>
    /// パーティクルエフェクトを管理するクラス
    /// ピース消去時のキラキラエフェクトなど
    /// </summary>
    public class ParticleEffectManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameConfig gameConfig;

        [Header("Particle Prefab")]
        [SerializeField] private ParticleSystem particlePrefab;

        [Header("Auto-Create Particle")]
        [SerializeField] private bool autoCreateParticle = true;

        private void Awake()
        {
            // パーティクルが未設定の場合は自動作成
            if (particlePrefab == null && autoCreateParticle)
            {
                CreateDefaultParticle();
            }
        }

        /// <summary>
        /// デフォルトのパーティクルシステムを作成
        /// </summary>
        private void CreateDefaultParticle()
        {
            Debug.Log("ParticleEffectManager: Creating default particle system...");

            // パーティクルプレハブ用の GameObject 作成
            GameObject particleObj = new GameObject("DefaultParticle");
            particleObj.transform.SetParent(transform);

            // ParticleSystem 追加
            ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

            // メインモジュール設定
            var main = ps.main;
            main.startLifetime = 1.0f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            main.startColor = Color.yellow;
            main.maxParticles = 100;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Emission モジュール設定
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 15)
            });

            // Shape モジュール設定
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            // Renderer 設定
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            // より確実なマテリアル設定
            Material particleMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
            if (particleMaterial.shader == null)
            {
                particleMaterial = new Material(Shader.Find("Sprites/Default"));
            }
            particleMaterial.color = Color.white;
            renderer.material = particleMaterial;
            renderer.sortingOrder = 100;

            // Size over Lifetime 設定（大きさを変化）
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0.0f, 1.0f);
            sizeCurve.AddKey(0.5f, 1.2f);
            sizeCurve.AddKey(1.0f, 0.0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Color over Lifetime 設定
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0.0f),
                    new GradientColorKey(Color.yellow, 0.5f),
                    new GradientColorKey(Color.white, 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );

            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            // プレハブとして保存
            particlePrefab = ps;

            Debug.Log("ParticleEffectManager: Default particle system created successfully!");
        }

        /// <summary>
        /// 指定位置でパーティクルを再生
        /// </summary>
        public void PlayParticleAt(Vector3 position, Color color)
        {
            if (particlePrefab == null)
            {
                Debug.LogWarning("ParticleEffectManager: particlePrefab is null!");
                return;
            }

            // Z位置を調整（カメラより手前）
            position.z = -2f;

            // パーティクルをインスタンス化
            ParticleSystem particleInstance = Instantiate(particlePrefab, position, Quaternion.identity);
            particleInstance.gameObject.SetActive(true);

            Debug.Log($"ParticleEffectManager: Playing particle at {position} with color {color}");

            // 色を設定
            var main = particleInstance.main;
            main.startColor = color;

            // Renderer の色も設定
            var renderer = particleInstance.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = color;
            }

            // 再生
            particleInstance.Play();

            // パーティクル数を確認
            Debug.Log($"ParticleEffectManager: Particle count = {particleInstance.particleCount}");

            // 自動削除
            float lifetime = main.startLifetime.constant;
            Destroy(particleInstance.gameObject, lifetime + 1.0f);
        }

        /// <summary>
        /// ピース消去時のエフェクト
        /// </summary>
        public void PlayEraseEffect(Piece piece)
        {
            if (piece == null)
            {
                Debug.LogWarning("ParticleEffectManager: Piece is null!");
                return;
            }

            if (gameConfig == null)
            {
                Debug.LogWarning("ParticleEffectManager: GameConfig is null!");
                return;
            }

            Color particleColor = gameConfig.GetColorForPieceType(piece.PieceType);
            Debug.Log($"ParticleEffectManager: Erasing piece of type {piece.PieceType} with color {particleColor}");
            PlayParticleAt(piece.transform.position, particleColor);
        }
    }
}
