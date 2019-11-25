# システム
## シーンの構成
- Systemシーンを永続化させて、ゲーム全体を通して利用するパラメーター、カメラ、フェード、BGM管理を行う
- ゲームのパラメーターや全体を統括する処理は、`GameParams`
- シーン切り替えは、`SceneChanger.ChangeScene()`で行う
- フェードは、`Fade`
- 各シーンには、`シーン名Manager`というオブジェクトとクラスを作成して、それで管理

## キー操作
以下の仮実装をしてある。

- タイトルでは、Spaceキーでステージ選択へ
- ステージ選択では、Spaceキーでゲームへ
- ゲームでは、Sキーでステージ選択、Eキーでエンディングへ
- エンディングでは、Spaceキーでタイトルヘ

Inputの設定をゲームに合わせて行ったら、Input.GetButton()に変更すること。

## デバッグについて
Player SettingsのOther SettingsのScripting Define Symbolsで`DEBUG`を定義している。これを削除すると、`#if DEBUG`～`#endif`間のデバッグコードをまとめて無効にできる。ビルドする時に消す。

## Warning
- `Your multi-scene setup may be improved by tending to ...`という警告は、マルチシーンでライティングをオートにしていると表示される警告。ライトが決まったあとにベイクすれば治るので、それまでは気にしなくてよい
