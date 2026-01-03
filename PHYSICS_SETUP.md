# 物理ベースツムツムゲーム セットアップガイド

完全物理演算ベースのゲームシステムへの移行手順です。

## 概要

従来のグリッドベースシステムから、完全物理演算ベースのシステムに移行しました。

**主な変更点:**
- グリッド配置 → 自由落下で積み上がる
- グリッド座標での隣接判定 → 物理的な距離/接触判定
- 固定配置 → Dynamic Rigidbody2D による重力シミュレーション
- 床と壁で範囲を制限

## 新規作成されたファイル

### コアスクリプト
1. `Assets/Scripts/Board/PhysicsBoardManager.cs` - 物理ベースのボード管理
2. `Assets/Scripts/Board/BoardBoundary.cs` - 床と壁の境界作成
3. `Assets/Scripts/Input/PhysicsInputManager.cs` - 物理ベースの入力管理
4. `Assets/Scripts/Core/PhysicsGameManager.cs` - 物理ベースのゲーム統括

### 既存ファイルとの比較

| 機能 | グリッド版 | 物理版 |
|------|-----------|--------|
| ボード管理 | `BoardManager.cs` | `PhysicsBoardManager.cs` |
| 入力管理 | `InputManager.cs` | `PhysicsInputManager.cs` |
| ゲーム管理 | `GameManager.cs` | `PhysicsGameManager.cs` |
| ピース配置 | 2D配列 `Piece[8,8]` | リスト `List<Piece>` |
| 連結判定 | マンハッタン距離 | 物理的距離 |
| 補充方法 | グリッド上部から整列 | ランダムX位置から落下 |

## Unity エディタでのセットアップ手順

### 1. 境界（Boundary）の設定

新しいGameObjectを作成します。

```
Hierarchy:
  └─ BoardBoundary (Empty GameObject)
      └─ BoardBoundary.cs をアタッチ
```

**Inspector設定:**
- `Game Config`: GameConfig アセットを参照
- `Boundary Width`: 8.0 (ボードの幅)
- `Boundary Height`: 10.0 (ボードの高さ)
- `Wall Thickness`: 0.5 (壁の厚さ)

### 2. 物理ボード管理（PhysicsBoardManager）の設定

新しいGameObjectを作成します。

```
Hierarchy:
  └─ PhysicsBoardManager (Empty GameObject)
      └─ PhysicsBoardManager.cs をアタッチ
```

**Inspector設定:**
- `Game Config`: GameConfig アセットを参照
- `Piece Pool`: PiecePool オブジェクトを参照
- `Board Boundary`: BoardBoundary オブジェクトを参照
- `Spawn Height`: 5.0 (ピースの出現高さ)
- `Spawn Interval`: 0.3 (補充間隔)
- `Initial Piece Count`: 30 (初期ピース数)
- `Enable Particles`: チェック

### 3. 物理入力管理（PhysicsInputManager）の設定

新しいGameObjectを作成します。

```
Hierarchy:
  └─ PhysicsInputManager (Empty GameObject)
      └─ PhysicsInputManager.cs をアタッチ
```

**Inspector設定:**
- `Game Config`: GameConfig アセットを参照
- `Physics Board Manager`: PhysicsBoardManager を参照
- `Main Camera`: Main Camera を参照
- `Piece Layer Mask`: "Piece" レイヤーを選択
- `Touch Distance`: 1.2 (接触判定距離)
- `Line Width`: 0.2
- `Line Color`: 黄色 (R:1, G:1, B:0, A:0.8)

### 4. 物理ゲーム管理（PhysicsGameManager）の設定

新しいGameObjectを作成します。

```
Hierarchy:
  └─ PhysicsGameManager (Empty GameObject)
      └─ PhysicsGameManager.cs をアタッチ
```

**Inspector設定:**
- `Game Config`: GameConfig アセットを参照
- `Physics Board Manager`: PhysicsBoardManager を参照
- `Physics Input Manager`: PhysicsInputManager を参照
- `Time Manager`: TimeManager を参照
- `Score Manager`: ScoreManager を参照
- `Auto Start Game`: チェック

### 5. Piece プレハブの物理設定

既存の `Piece.prefab` を開いて確認します。

**必要なコンポーネント:**
- ✅ `SpriteRenderer` - 既存
- ✅ `CircleCollider2D` - 既存（radius: 0.45）
- ✅ `Rigidbody2D` - 既存

**Rigidbody2D 設定（重要）:**
- `Body Type`: Kinematic（初期値、スクリプトで動的に変更）
- `Gravity Scale`: 0（初期値、落下時に 2.0 に変更）
- `Collision Detection`: Continuous
- `Constraints`: Freeze Rotation Z （回転を防ぐ場合）

**レイヤー設定:**
- Layer: "Piece"（まだ作成していない場合は作成）

### 6. レイヤー設定

Unity エディタで新しいレイヤーを作成します。

1. **Edit > Project Settings > Tags and Layers**
2. Layers に "Piece" を追加
3. Piece.prefab の Layer を "Piece" に設定

### 7. 物理設定の最適化（推奨）

**Edit > Project Settings > Physics 2D**

推奨設定:
- `Gravity`: (0, -20) - 少し強めの重力
- `Default Contact Offset`: 0.01
- `Queries Hit Triggers`: チェックを外す
- `Queries Start In Colliders`: チェック

**Layer Collision Matrix:**
- "Piece" ↔ "Piece": チェック（ピース同士が衝突）
- "Piece" ↔ "Default": チェック（境界と衝突）

## シーン構成例

完全な物理ベースシーン構成:

```
SampleScene
├─ Main Camera
│   └─ Position: (0, 0, -10)
│   └─ Orthographic Size: 5
│
├─ PhysicsGameManager
│   └─ PhysicsGameManager.cs
│
├─ PhysicsBoardManager
│   └─ PhysicsBoardManager.cs
│
├─ BoardBoundary
│   └─ BoardBoundary.cs
│
├─ PhysicsInputManager
│   └─ PhysicsInputManager.cs
│
├─ TimeManager
│   └─ TimeManager.cs
│
├─ ScoreManager
│   └─ ScoreManager.cs
│
├─ PiecePool
│   └─ PiecePool.cs
│   └─ Piece Prefab 参照
│
└─ Canvas (Screen Space - Camera)
    ├─ ScoreText (TextMeshProUGUI)
    │   └─ ScoreView.cs
    ├─ TimerText (TextMeshProUGUI)
    │   └─ TimerView.cs
    └─ ComboText (TextMeshProUGUI)
```

## 移行手順（既存プロジェクトから）

既にグリッド版が動いている場合:

### オプション A: 新規シーンで試す（推奨）

1. 新しいシーン `PhysicsScene` を作成
2. 上記のセットアップ手順に従って構築
3. 動作確認後、既存シーンを置き換え

### オプション B: 既存シーンを更新

1. **古いコンポーネントを無効化:**
   - GameManager を無効化
   - BoardManager を無効化
   - InputManager を無効化

2. **新しいコンポーネントを追加:**
   - 上記セットアップ手順に従う

3. **テスト後、古いコンポーネントを削除**

## 動作確認

ゲームを実行して以下を確認:

### 初期状態
- [ ] ピースが上から落ちてくる
- [ ] 床と壁が表示される（半透明グレー）
- [ ] ピースが床に積み上がる
- [ ] 30個のピースが生成される

### ドラッグ操作
- [ ] ピースをドラッグで選択できる
- [ ] 接触しているピースだけが連結される
- [ ] 連結線が表示される
- [ ] 3個以上で消去可能

### 消去アニメーション
- [ ] ピースが回転しながら消える
- [ ] パーティクルが表示される
- [ ] スコアが加算される

### 補充
- [ ] 消去後、新しいピースが上から落ちてくる
- [ ] 既存のピースは物理的に落下しない（すでに積み上がっている）
- [ ] 補充されたピースは自然に積み重なる

## トラブルシューティング

### ピースが表示されない
- PiecePool の Piece Prefab 参照を確認
- PhysicsBoardManager の Initialize() が呼ばれているか確認
- Console でエラーログを確認

### ピースが落ちない
- Rigidbody2D の Body Type が Kinematic になっているか確認
- PhysicsBoardManager.SpawnPiece() で Dynamic に変更されているか確認
- Gravity Scale が設定されているか確認（2.0）

### ピースが壁を突き抜ける
- BoardBoundary の Rigidbody2D が Static になっているか確認
- Collision Detection が Continuous になっているか確認
- Layer Collision Matrix を確認

### 連結できない
- Piece の Layer が "Piece" になっているか確認
- PhysicsInputManager の Piece Layer Mask が "Piece" になっているか確認
- CircleCollider2D が有効になっているか確認

### パーティクルが表示されない
- PhysicsBoardManager の Enable Particles がチェックされているか確認
- Console でパーティクル関連のログを確認
- Particle System の Renderer Material を確認

## パフォーマンス最適化

物理演算を使用するため、以下に注意:

1. **Fixed Timestep 設定**
   - Edit > Project Settings > Time
   - Fixed Timestep: 0.02（デフォルト）または 0.01（より滑らか）

2. **ピース数の制限**
   - Initial Piece Count を 50 以下に
   - 画面外のピースを削除する仕組み（将来実装）

3. **Collision Detection**
   - 高速移動しない場合は Discrete でも可

## 次のステップ

物理ベースシステムが動作したら:

1. **バランス調整**
   - Spawn Height（高すぎると散らばる）
   - Gravity Scale（重力の強さ）
   - Touch Distance（接触判定の厳しさ）

2. **視覚改善**
   - ピースのバウンス感を調整
   - パーティクルエフェクトの強化
   - 連結線のアニメーション

3. **ゲームプレイ改善**
   - 特殊ピース（物理的な爆発など）
   - チェーンボーナスの視覚化
   - コンボエフェクト

## 参考: グリッド版との違い

| 特徴 | グリッド版 | 物理版 |
|------|-----------|--------|
| 見た目 | 整然と配置 | 自然に積み重なる |
| 補充 | 上から整列して落下 | ランダムX位置から落下 |
| 連結判定 | 厳密な隣接（4方向） | 物理的な距離/接触 |
| パフォーマンス | 高速（計算少） | やや重い（物理計算） |
| ツムツム感 | 低い | 高い |

---

## まとめ

物理ベースシステムは、よりツムツムらしい見た目と挙動を実現します。
セットアップは少し複雑ですが、このガイドに従えば問題なく動作するはずです。

何か問題があれば、Console のログを確認してください。
