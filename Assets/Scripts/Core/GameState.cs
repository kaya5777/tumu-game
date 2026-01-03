namespace TsumGame.Core
{
    /// <summary>
    /// ゲームの状態を定義する列挙型
    /// </summary>
    public enum GameState
    {
        /// <summary>待機中（入力受付可能）</summary>
        Idle,

        /// <summary>ゲーム中</summary>
        Playing,

        /// <summary>ピース連結中</summary>
        Connecting,

        /// <summary>ピース消去中</summary>
        Erasing,

        /// <summary>ピース落下中</summary>
        Falling,

        /// <summary>ゲーム終了</summary>
        GameOver
    }
}
