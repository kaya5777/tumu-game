using UnityEngine;

namespace TsumGame.Board
{
    /// <summary>
    /// グリッド上の1つのセルを表すデータクラス
    /// BoardManager 内で使用
    /// </summary>
    public class Cell
    {
        public Vector2Int GridPosition { get; private set; }
        public Piece CurrentPiece { get; private set; }
        public bool IsEmpty => CurrentPiece == null;

        public Cell(Vector2Int gridPosition)
        {
            GridPosition = gridPosition;
            CurrentPiece = null;
        }

        /// <summary>
        /// セルにピースを設定
        /// </summary>
        public void SetPiece(Piece piece)
        {
            CurrentPiece = piece;
            if (piece != null)
            {
                piece.SetGridPosition(GridPosition);
            }
        }

        /// <summary>
        /// セルをクリア
        /// </summary>
        public void Clear()
        {
            CurrentPiece = null;
        }
    }
}
