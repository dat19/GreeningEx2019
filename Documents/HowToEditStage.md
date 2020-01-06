# ステージの作り方
ステージは、簡易ステージエディタで作成します。

- ProjectウィンドウのScenesフォルダーから、StageEditorシーンをダブルクリックして開く


## ステージが完成したら
保存機能はプレハブ化で代用する。

- Playは停止せずに、HierarchyウィンドウのMapオブジェクトをドラッグして、ProjectウィンドウのPrefabs/Stagesフォルダーにドロップしてプレハブ化する
- プレハブ化ができたらPlayを停止
- Scenes/StagesフォルダーのStage1をクリックして選択
- [Ctrl]+[D]キーを押して、複製する
- 複製されたStageシーンの名前を、作成したマップのステージ数のものに変更する
- 複製して名前を変更したシーンをダブルクリックして開く
- HierarchyウィンドウのTestMap、或いは、Mapオブジェクトを削除する
- Projectウィンドウから、プレハブ化したMapプレハブをドラッグして、Hierarchyウィンドウにドロップする
- ドロップしたMapオブジェクトを右クリックして、Unpack Prefabを選択して、プレハブを解除する

以上で、マップの設定が完了する。不要になったMapプレハブは、Projectウィンドウから削除してよい。

ステージとして正しく動かすための設定を行う。

- HierarchyウィンドウのMapオブジェクトをクリックして選択
- Inspectorウィンドウで、Stage Editorスクリプトのチェックを外して、エディット機能を無効化
- Windowメニューから、Rendering > Lighting Settingsを選択
- 下の方にあるAudo Generateのチェックを外して、Generate Lightingをクリックして、ライトをベイクする
- [Ctrl]+[S]キーを押して、シーンを保存する
- Fileメニューから、Build Settingsを選択
- Add Open Scenesボタンをクリックして、新規に作成したステージシーンをビルドターゲットに追加する

以上で完了。Systemシーンをダブルクリックして開いたら、Playしてゲームを開始する。
