# きのこジャンプ
動作の開始は、方向キーとアクションキーのどちらでも可能にしておく。動作開始はActableを継承したMushroomActableのAction()で判定したい。これを移動に対応させるために、Actableクラスに以下の機能を追加する。

- PushAction()
  - 体当たりした時に発動する動作があれば開始する

これをStellaWalkの接触時に呼び出せるかチェックして呼び出す。まずは以下のものを作成する。

- ActableにPushAction()をvirtualの空のメソッドとして追加
- MashroomActableを作成。ActableにPushActionとCanActionを実装。PushActionは一先ず何かログ表示しておく
- StellaWalkのOnCollisionEnterとStayに、PushActionを呼び出すフローを作成

以上で動作確認。

## ジャンプ開始
ジャンプ自体はミニジャンプと同じ仕組みを利用する。PushActionでマッシュルームの頂点座標を算出して、StellaMove.targetJumpPositionに設定して、ジャンプアクションを開始する。

速度の計算はStellaMoveのStartTargetJump()に実装してあるので、targetJumpGroundに、きのこの頂点座標を設定した上で、ジャンプに移行すればよい。

## きのこ上への着地からきのこジャンプへ
StellaAirの着地時に、着地したオブジェクトにStepOnBaseを継承したクラスがあれば、それのStepOn()メソッドを呼び出す。MushroomStepOnクラスを実装してアタッチしておく。

- IStepOnインターフェースを作成して、StepOn()を定義
- MushroomStepOnを作成して、IStepOnを実装して、StepOn()を実装。とりあえず何か表示させる
- StellaAirの着地時に、何に乗っているかを調べて、IStepOnのStepOn()があれば呼び出す

以上で動作確認して、キノコに乗ったらメッセージが表示されるようにする。

## キノコジャンプ
きのこに乗ったらキノコジャンプアクションへ移る。これは専用の動作が必要なので、StellaMushroomJumpを作成する。

- StellaMoveのActionTypeに、MushroomJumpを追加
- きのこジャンプは通常のジャンプと違って、キャンセルはできない
- Init()で以下を設定
  - きのこアニメーションとジャンプアニメを開始
  - ジャンプアニメが完了したら呼び出すジャンプ実行のメソッドを登録
- UpdateActionでは、キノコの当たり判定に合わせて高さを下げる処理を実行する
- ジャンプ実行メソッドを定義して、myVelocityを算出して設定して、それ以降はAirに処理を譲る

## 不具合修正
- StellaMove.instance.Gravity()において、落下していない時でもisGroundedがtrueの場合は着地動作に入っていたので、上昇時は着地処理をしないように条件を追加
- 着地アニメに、ジャンプ開始ステータスが設定されたらすぐにジャンプ開始アニメに移行するようにトランジションを追加
