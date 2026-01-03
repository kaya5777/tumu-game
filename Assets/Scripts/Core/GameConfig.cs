using UnityEngine;

namespace TsumGame.Core
{
    /// <summary>
    /// ゲーム設定を管理する ScriptableObject
    /// Inspector から簡単に調整可能
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "TsumGame/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("ボード設定")]
        [Tooltip("ボードの幅")]
        public int boardWidth = 8;

        [Tooltip("ボードの高さ")]
        public int boardHeight = 8;

        [Tooltip("セル間のスペース（ワールド単位）")]
        public float cellSpacing = 1.0f;

        [Header("ピース設定")]
        [Tooltip("各ピースタイプのスプライト")]
        public Sprite redSprite;      // piece_0
        public Sprite blueSprite;     // piece_1
        public Sprite greenSprite;    // piece_2
        public Sprite yellowSprite;   // piece_3
        public Sprite purpleSprite;   // piece_4

        [Tooltip("各ピースタイプの色（パーティクル用）")]
        public Color redColor = new Color(1f, 0f, 0f);      // #FF0000
        public Color blueColor = new Color(0f, 0f, 1f);     // #0000FF
        public Color greenColor = new Color(0f, 1f, 0f);    // #00FF00
        public Color yellowColor = new Color(1f, 1f, 0f);   // #FFFF00
        public Color purpleColor = new Color(1f, 0f, 1f);   // #FF00FF

        [Tooltip("ハイライト時の色（透明度調整）")]
        public Color highlightColor = new Color(1f, 1f, 1f, 0.7f);

        [Header("ゲームルール")]
        [Tooltip("消去に必要な最小ピース数")]
        public int minPiecesToErase = 3;

        [Tooltip("ゲーム時間（秒）")]
        public float gameTime = 60f;

        [Header("スコア設定")]
        [Tooltip("ピース1個あたりの基本スコア")]
        public int baseScorePerPiece = 10;

        [Tooltip("コンボ倍率（配列インデックス = コンボ数 - 1）")]
        public float[] comboMultipliers = { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 4.0f, 5.0f, 6.0f, 8.0f, 10.0f };

        [Tooltip("コンボがリセットされるまでの時間（秒）")]
        public float comboResetTime = 2.0f;

        [Header("アニメーション設定")]
        [Tooltip("ピース消去アニメーション時間")]
        public float pieceEraseAnimationDuration = 0.2f;

        [Tooltip("ピース落下アニメーション時間")]
        public float pieceFallAnimationDuration = 0.3f;

        [Header("オブジェクトプール")]
        [Tooltip("ピースプールの初期サイズ")]
        public int poolInitialSize = 100;

        [Tooltip("プールの最大サイズ")]
        public int poolMaxSize = 150;

        /// <summary>
        /// ピースタイプに対応するスプライトを取得
        /// </summary>
        public Sprite GetSpriteForPieceType(PieceType type)
        {
            switch (type)
            {
                case PieceType.Red:
                    return redSprite;
                case PieceType.Blue:
                    return blueSprite;
                case PieceType.Green:
                    return greenSprite;
                case PieceType.Yellow:
                    return yellowSprite;
                case PieceType.Purple:
                    return purpleSprite;
                default:
                    return null;
            }
        }

        /// <summary>
        /// ピースタイプに対応する色を取得（パーティクル用）
        /// </summary>
        public Color GetColorForPieceType(PieceType type)
        {
            switch (type)
            {
                case PieceType.Red:
                    return redColor;
                case PieceType.Blue:
                    return blueColor;
                case PieceType.Green:
                    return greenColor;
                case PieceType.Yellow:
                    return yellowColor;
                case PieceType.Purple:
                    return purpleColor;
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// コンボ倍率を取得（範囲外の場合は最大倍率を返す）
        /// </summary>
        public float GetComboMultiplier(int comboCount)
        {
            if (comboCount <= 0)
                return 1.0f;

            int index = comboCount - 1;
            if (index >= comboMultipliers.Length)
                return comboMultipliers[comboMultipliers.Length - 1];

            return comboMultipliers[index];
        }
    }
}
