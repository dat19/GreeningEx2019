# ステージ選択シーン
ステージ選択シーンを作成する。ステージ選択シーンは、以下のような流れである。

- GameParamsにシーン選択直後の演出が設定されていれば再生開始
  - オープニングムービー
  - クリア演出
  - カットインムービー
  - エンディングムービー
- 演出があれば、終了まで待機
- 演出がないか(ユーザー操作で戻った)、終了したら、操作可能状態
  - 左右キーで選択ステージの変更
  - 何かのキーでゲーム開始
  - 選択したステージを手前に表示する処理を常に実行
  - ステージ名は、StageSelectManagerに文字オブジェクトのインスタンスを渡して表示管理

## 文字レイアウトを作成
画面には、以下の文字が必要なのでレイアウトする。

- 現在選択中のステージとステージ名。上方に中央揃え
- 左右キーでステージ変更が可能と分かるように、左右矢印か、<>をアニメ表示
- キーを押したらゲームが開始するという表示

## シーン開始
どのようにシーンを開始したかは、GameParamsのtoStageSelect変数で指定されているので、この値を読み取って初期化を実行する。開始の種類は、StageSelectManagerのToStageSelectType列挙子で宣言されている。

## 選択している島を手前に表示(WIP)
BaseStarオブジェクトにBaseStarスクリプトをアタッチして制御する。現在のステージをGameParamsから読み取って、常時、選択されたステージを手前に表示するように動かす。

### 考え方
1. 星の中心から、選択したステージへの方向ベクトルを求める
1. 方向ベクトル、星の中心、Vector3.backの平面の外積を取って、法線を求める
1. 求めた方向ベクトルと手前向きのベクトルVector3.backが、2の法線を軸として為す角度をVector3.SignedAngle()で求める
1. BaseStarを中心に、求めた法線を軸として、2で求めた角度に減衰率を掛けた角度で、`Transform.RotateAround()`で回転させる
  - https://docs.unity3d.com/ja/2017.4/ScriptReference/Transform.RotateAround.html

## クリア済みのステージの上空に星を表示する
ステージ選択シーンのStart()で、クリア済みのステージの数だけ星をInstantiateして、BaseStarから既定の半径の位置に配置する。配置したら、BaseStarの子にする。

- 未クリアステージでは、初期状態の星を表示
- クリアしている場合、星を回転させる

## ステージの星の向きを調整
- StageStarにスクリプトを設定
- 常に、カメラに相対させる
- 選択中のステージの星は、Vector3.up軸で回転させる

## 最初の準備

### クリア
- 2面をクリアした時(ClearedStageCount = 2)
  - 奇麗な海、島、星: 1面まで
  - 未クリア状態の星を表示: 2面の星
  - 演出: 2面
  - 演出後、星を表示: 3面 starCount
- 10面をクリアした時(ClearedStageCount = 10)
  - 奇麗な海、島、星: 9面まで
  - 未クリア状態の星を表示: 10面の星
  - 演出: 10面
  - 演出後、星の表示はなし

### 戻る
- 2面までクリアで、3面から戻った時(ClearedStageCount = 2)
  - 奇麗な海、島、星: 2面まで
  - 未クリア状態の星を表示: 3面の星
- 9面までクリアで、10面から戻った時(ClearedStageCount = 9)
  - 奇麗な海、島、星: 9面まで
  - 未クリア状態の星を表示: 10面の星
- 10面クリアして、10面から戻った時(ClearedStageCount = 10)
  - 奇麗な海、島、星: 10面まで
  - 未クリア状態の星を表示: なし

### 変数
- 星の作成個数
  - starCount = ClearedStageCount+1 or StageMax
- 事前に表示しておく星の数
  - displayCount = クリアの時、ClearedStageCount : starCountと一致
- 奇麗にする海、島、星
  - cleanedCount = クリアの時、ClearedStageCount-1 : 戻った時、ClearedStageCount
- 奇麗にする前に表示しておく星
  - nextTargetStar = クリアの時、ClearedStageCount : 戻った時、ClearedStageCount+1。Stagemaxを越えていたらなし
- 表示する星
  - クリアのみ、ClearedStageCount+1
