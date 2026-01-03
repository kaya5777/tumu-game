# Unity エディタ設定手順

このファイルには、ツムツムライクゲームをUnityエディタで動作させるための設定手順が記載されています。

---

## 1. GameConfig ScriptableObject の作成

### 手順
1. Unity エディタのプロジェクトウィンドウで、`Assets` フォルダを右クリック
2. `Create > TsumGame > Game Config` を選択
3. 作成された `GameConfig` アセットの名前を `GameConfig` に変更
4. Inspector で以下の値を確認（デフォルト値が設定されているはずです）:
   - **Board Width**: 8
   - **Board Height**: 8
   - **Cell Spacing**: 1.0
   - **Min Pieces To Erase**: 3
   - **Game Time**: 60
   - **Base Score Per Piece**: 10
   - **Pool Initial Size**: 100

---

## 2. Piece Prefab の作成

### 手順
1. **空の GameObject を作成**
   - Hierarchy で右クリック → `Create Empty`
   - 名前を `Piece` に変更

2. **必要なコンポーネントを追加**
   - `Piece` GameObject を選択
   - Inspector で `Add Component` ボタンをクリック
   - 以下のコンポーネントを追加:
     - `Sprite Renderer`
     - `Box Collider 2D`
     - `Scripts > Board > Piece` (作成したスクリプト)

3. **Sprite Renderer の設定**
   - **Sprite**: Unity のデフォルトスプライト `Circle` を選択
     - (Assets を右クリック → Create → Sprites → Circle)
   - **Color**: 白 (デフォルト)
   - **Sorting Layer**: Default
   - **Order in Layer**: 0

4. **Box Collider 2D の設定**
   - **Size**: (0.9, 0.9) に設定（スプライトよりわずかに小さく）

5. **Layer の作成と設定**
   - Unity メニューバーで `Edit > Project Settings > Tags and Layers`
   - `Layers` セクションで空いているスロット（例: User Layer 6）に `Piece` を追加
   - Hierarchy の `Piece` GameObject を選択
   - Inspector の `Layer` ドロップダウンで `Piece` を選択

6. **Prefab として保存**
   - Hierarchy の `Piece` GameObject を `Assets/Prefabs` フォルダにドラッグ
   - Hierarchy から `Piece` GameObject を削除

---

## 3. SampleScene のセットアップ

### A. Manager GameObjects の作成

#### 3-1. GameManager
1. Hierarchy で右クリック → `Create Empty`
2. 名前を `GameManager` に変更
3. Inspector で以下のコンポーネントを追加:
   - `Scripts > Core > Game Manager`
   - `Scripts > Core > Time Manager`
   - `Scripts > Core > Score Manager`
4. `Game Manager` コンポーネントの Inspector で参照を設定:
   - **Game Config**: 作成した `GameConfig` アセットをドラッグ
   - **Board Manager**: (後で設定)
   - **Input Manager**: (後で設定)
   - **Time Manager**: 同じ GameObject の `Time Manager` をドラッグ
   - **Score Manager**: 同じ GameObject の `Score Manager` をドラッグ
   - **Auto Start Game**: チェックを入れる

#### 3-2. BoardManager
1. Hierarchy で右クリック → `Create Empty`
2. 名前を `BoardManager` に変更
3. Inspector で以下のコンポーネントを追加:
   - `Scripts > Board > Board Manager`
   - `Scripts > Board > Piece Pool`
4. `Board Manager` コンポーネントの Inspector で参照を設定:
   - **Game Config**: `GameConfig` アセットをドラッグ
   - **Piece Pool**: 同じ GameObject の `Piece Pool` をドラッグ
5. `Piece Pool` コンポーネントの Inspector で参照を設定:
   - **Piece Prefab**: `Assets/Prefabs/Piece.prefab` をドラッグ
   - **Game Config**: `GameConfig` アセットをドラッグ

#### 3-3. InputManager
1. Hierarchy で右クリック → `Create Empty`
2. 名前を `InputManager` に変更
3. Inspector でコンポーネントを追加:
   - `Scripts > Input > Input Manager`
4. `Input Manager` コンポーネントの Inspector で参照を設定:
   - **Game Config**: `GameConfig` アセットをドラッグ
   - **Board Manager**: `BoardManager` GameObject をドラッグ
   - **Main Camera**: Hierarchy の `Main Camera` をドラッグ
   - **Piece Layer Mask**: `Piece` レイヤーを選択

#### 3-4. GameManager の参照を完成させる
1. Hierarchy の `GameManager` を選択
2. Inspector の `Game Manager` コンポーネントで:
   - **Board Manager**: `BoardManager` GameObject をドラッグ
   - **Input Manager**: `InputManager` GameObject をドラッグ

### B. Main Camera の設定

1. Hierarchy の `Main Camera` を選択
2. Inspector で以下を設定:
   - **Projection**: Orthographic
   - **Size**: 5
   - **Position**: (0, 0, -10)
   - **Background**: お好みの色（黒推奨）

### C. UI Canvas の作成

#### 3-5. Canvas
1. Hierarchy で右クリック → `UI > Canvas`
2. `Canvas` コンポーネントの Inspector で:
   - **Render Mode**: Screen Space - Camera
   - **Render Camera**: `Main Camera` をドラッグ
   - **Plane Distance**: 10
3. `Canvas Scaler` コンポーネントで:
   - **UI Scale Mode**: Scale With Screen Size
   - **Reference Resolution**: (1920, 1080)

#### 3-6. ScoreText
1. `Canvas` を右クリック → `UI > Text - TextMeshPro`
   - ※初回は TMP Importer が表示されるので `Import TMP Essentials` をクリック
2. 名前を `ScoreText` に変更
3. Inspector で設定:
   - **Text**: "SCORE: 0"
   - **Font Size**: 48
   - **Alignment**: 左上
   - **Color**: 白
   - **Rect Transform**:
     - **Anchors**: Top-Left
     - **Pos X**: 50, **Pos Y**: -50
     - **Width**: 400, **Height**: 80
4. `Add Component` → `Scripts > UI > Score View`
5. `Score View` コンポーネントで:
   - **Score Text**: 自分自身の `TextMeshProUGUI` をドラッグ
   - **Combo Text**: (後で設定)

#### 3-7. ComboText
1. `Canvas` を右クリック → `UI > Text - TextMeshPro`
2. 名前を `ComboText` に変更
3. Inspector で設定:
   - **Text**: "COMBO x2!"
   - **Font Size**: 72
   - **Alignment**: 中央
   - **Color**: 黄色
   - **Rect Transform**:
     - **Anchors**: Center
     - **Pos X**: 0, **Pos Y**: 100
     - **Width**: 600, **Height**: 120
4. **初期状態で非表示**: GameObject のチェックを外す

#### 3-8. TimerText
1. `Canvas` を右クリック → `UI > Text - TextMeshPro`
2. 名前を `TimerText` に変更
3. Inspector で設定:
   - **Text**: "TIME: 01:00"
   - **Font Size**: 48
   - **Alignment**: 右上
   - **Color**: 白
   - **Rect Transform**:
     - **Anchors**: Top-Right
     - **Pos X**: -50, **Pos Y**: -50
     - **Width**: 400, **Height**: 80
4. `Add Component` → `Scripts > UI > Timer View`
5. `Timer View` コンポーネントで:
   - **Timer Text**: 自分自身の `TextMeshProUGUI` をドラッグ
   - **Warning Threshold**: 10
   - **Warning Color**: 赤

#### 3-9. ScoreText に ComboText を設定
1. Hierarchy の `ScoreText` を選択
2. Inspector の `Score View` コンポーネントで:
   - **Combo Text**: `ComboText` の `TextMeshProUGUI` をドラッグ

#### 3-10. GameOverPanel（オプション）
1. `Canvas` を右クリック → `UI > Panel`
2. 名前を `GameOverPanel` に変更
3. Inspector で設定:
   - **Color**: 半透明の黒 (R:0, G:0, B:0, A:0.8)
4. `GameOverPanel` を右クリック → `UI > Text - TextMeshPro`
5. 名前を `FinalScoreText` に変更
   - **Text**: "FINAL SCORE\n0"
   - **Font Size**: 72
   - **Alignment**: 中央
   - **Color**: 白
6. `GameOverPanel` を右クリック → `UI > Button - TextMeshPro`
7. 名前を `RestartButton` に変更
   - ボタン内のテキストを "RESTART" に変更
8. `GameOverPanel` に `Add Component` → `Scripts > UI > Game Over View`
9. `Game Over View` コンポーネントで:
   - **Game Over Panel**: `GameOverPanel` GameObject をドラッグ
   - **Final Score Text**: `FinalScoreText` の `TextMeshProUGUI` をドラッグ
   - **Restart Button**: `RestartButton` の `Button` をドラッグ
10. **初期状態で非表示**: `GameOverPanel` のチェックを外す

---

## 4. Time Manager と Score Manager の GameConfig 参照設定

### Time Manager
1. Hierarchy の `GameManager` を選択
2. Inspector の `Time Manager` コンポーネントで:
   - **Game Config**: `GameConfig` アセットをドラッグ

### Score Manager
1. 同じく `GameManager` の `Score Manager` コンポーネントで:
   - **Game Config**: `GameConfig` アセットをドラッグ

---

## 5. シーンの保存

1. `File > Save` (Ctrl+S / Cmd+S)
2. シーン名を `GameScene` に変更（オプション）

---

## 6. ゲームの実行

1. **Play ボタンをクリック**
2. **期待される動作**:
   - 8x8 のグリッドに5色のピースが配置される
   - タイマーが60秒からカウントダウン
   - マウスドラッグで同色ピースを連結
   - 3個以上で消去、スコア加算
   - ピースが落下・補充される
   - タイマーが0秒でゲームオーバー

---

## トラブルシューティング

### ピースが表示されない
- `BoardManager` の `Game Config` と `Piece Pool` の参照を確認
- `Piece Pool` の `Piece Prefab` の参照を確認

### 入力が反応しない
- `InputManager` の `Board Manager` と `Main Camera` の参照を確認
- `Piece` Layer が正しく設定されているか確認

### スコア・タイマーが表示されない
- UI の `ScoreView` / `TimerView` の参照を確認
- Canvas の Render Camera 設定を確認

### NullReferenceException が発生
- 各 Manager の GameConfig 参照を確認
- Inspector で赤色になっている参照があれば、正しいアセット/GameObject を設定

---

## 次のステップ

すべての設定が完了したら:
1. ゲームをテストプレイ
2. GameConfig の値を調整して難易度を変更
3. Piece の色やサイズを変更
4. WebGL ビルドを試す (`File > Build Settings > WebGL`)

---

以上で基本的なセットアップは完了です！
