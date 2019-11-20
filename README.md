# GreeningEx2019
 Greeningの2019年度生の改良プロジェクト用リポジトリー

# プロジェクトの準備
プロジェクトを作業できるように準備する。

1. Unityは閉じておく
1. Webブラウザーで github.com を開いて、自分のアカウントで*Sign in*する
1. 指定のURLを開く
1. リポジトリーを*Fork*する
1. GitHub Desktopを起動
1. *File*メニュー > *Options*を開いて、自分のアカウントで*Sign in*していることを確認する。自分のアカウント以外の時、自分のアカウントで*Sign in*する
1. *File*メニューから*Clone repository*を選択
1. フォークしたリポジトリーを選択して、クローンする。クローン先は、ドキュメントフォルダー内の自分の名前のフォルダーの中。すべて半角英数であること。自分の名前のフォルダーがない場合は、新しいフォルダーを作成して、半角英数で自分の名前を設定して、そこを選ぶ
1. クローンしたUnityのプロジェクトフォルダー内の`CopyPrivateResources.bat`をダブルクリックして、ネットドライブから必要なリソースをコピーする
1. Unityを起動して、クローンしたプロジェクトを読み込む
1. *Window*メニューから、*TextMeshPro* > *Import TMP Essential Resources*を選択

以上で、プロジェクトが実行可能になる。

# 命名規約
- フォルダー名とスクリプトファイル名はパスカルケース
- それ以外のリソースやレイヤー名は小文字のスネークケース
  - 参考は[こちら](https://docs.google.com/document/d/10DWSrp2QcdawOtBvM67lr8Sjv1disyUshkcp0mc_B5U/)
- ソースコードは、マイクロソフトの規約を元にしたUnityのプロジェクトで利用されているエディター設定(.editorconfig)をプロジェクトに加えてある。**エラー一覧**の**メッセージ**に警告が表示されるので、問題がなくなるように気を配ること
  - [Microsoft. 識別子名、名前付け規則](https://docs.microsoft.com/ja-jp/dotnet/csharp/programming-guide/inside-a-program/identifier-names)
  - [Microsoft. C#のコーディング規約](https://docs.microsoft.com/ja-jp/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)

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


# 使用アセット
- Bgm H/MIX Gallary http://www.hmix.net/
- Se 効果音ラボ https://soundeffect-lab.info/
