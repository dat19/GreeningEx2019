# クリア
クリアの流れは以下の通り。

- ステラが星に触れる StellaMove.OnTriggerEnter
  - StageManagerのStartClear()を呼び出して、ClearSequenceを開始
- ClearSequence
  - クリアの文字表示
  - ステラをClearアクションに切り替え
    - 旋回、星から見てよい位置に移動、移動方向に旋回(Turn, ToTarget, Turn, Wait)
  - 星の回転アニメ
  - 回転アニメの完了待ち
  - フェードアウト
  - 背景を切り替える
  - フェードイン
- フェードイン完了
  - ステラのつかまりアニメ再生(StellaClear.HoldStar)。左移動の時は、アニメのBackパラメーターをtrueにして開始

---

- StellaClear.HoldStar
  - アニメイベント設定 StellaClear.AttachStarを設定
  - ステラの向きを設定
  - 星に捕まるためのジャンプ開始
- StellaClear.AttackStar
  - 星とステラの手の位置関係をOffsetFromStarに保存
  - Goal.FollowStella()を呼び出して、星をステラにくっつける
  - StellaClear.FlyAwayをアニメイベントに登録
  - ステラの状態をWaitにして、イベントがアニメから呼ばれるのを待機
- つかまったイベント
  - ステラと星の位置関係を記録(StellaClear.AttachStar)
  - ステラの動きに星を合わせる(Goal.FollowStella)
- 捕まりアニメ終了イベント
  - 上昇アニメ
  - 微速X速度(Goal.FlyStart)
  - 星の移動を開始
  - ステラを星にくっつけて移動(StellaClear.HoldStar)
- スピードアップにアニメ切り替わり
  - X速度アップ(Goal.Fly)
- クリア処理
- フェードアウトして、ステージ選択シーンへ

以上の処理中、操作はできないので、CanMoveに条件を加える。処理の流れはStageManagerにコルーチンで実装する。

## スクリプトのアタッチ先
- 星に触れた時の判定
  - StellaのStellaMoveスクリプト
- クリアの流れを制御する処理
  - StageManagerオブジェクト
- 星のアニメ
  - Goalオブジェクト

## ステラの星からの相対座標
Unity上で位置を調整して確認する。

- `-0.25`, `-1.95`
