# LiveTalkZoomSample
LiveTalk 常時ファイル出力で出力したテキストを、Zoom の third-party Closed captioning service API を使って字幕表示するサンプルです。  
本サンプルコードは、.NET Core 3.1 で作成しています。コードレベルでは .NET Framework 4.6 と互換性があります。

# Zoom の closed caption とは
Zoom の字幕 (closed caption) は、Zoom ミーティングの主催者（ホスト）のカメラ映像や画面共有に字幕を表示させる機能です。
字幕の入力は、主催者端末から行うことが基本ですが、[参加者をタイプに割り当てる]で参加者の１人に字幕入力の権限を渡すこともできます。

[Zoom 公式ページ（英語）](https://support.zoom.us/hc/en-us/articles/207279736-Getting-Started-with-Closed-Captioning) 

# 手順
## Zoom で字幕の有効化を行う
[マイ設定 - Zoom](https://zoom.us/profile/setting) にアクセスして、自分がホストするミーティングで字幕表示を許可します。

![ZoomSetting](https://github.com/FujitsuSSL-LiveTalk/LiveTalkZoomSample/blob/images/Zoom01.png)

## Zoom ミーティングで API トークンを取得する
Zoomでミーティングを開始します。コントロールバーに [字幕] が増えています。

![ZoomSetting](https://github.com/FujitsuSSL-LiveTalk/LiveTalkZoomSample/blob/images/Zoom02.png)

[字幕] をクリックすると字幕関連のメニューが表示されるので、[API トークンを取得する] をクリックして、API トークンをコピーします。

![ZoomSetting](https://github.com/FujitsuSSL-LiveTalk/LiveTalkZoomSample/blob/images/Zoom03.png)

## API トークンを設定する
コピーした API トークンを設定します。

![ZoomSetting](https://github.com/FujitsuSSL-LiveTalk/LiveTalkZoomSample/blob/images/Zoom04.png)

## Zoom ミーティングで字幕を開始する
[私が入力します] をクリックすると、コントロールバーの [字幕] の横に [^] が表示されるので、[サブタイトルを表示] をクリックします。

![ZoomSetting](https://github.com/FujitsuSSL-LiveTalk/LiveTalkZoomSample/blob/images/Zoom05.png)

Zoom の手入力での字幕入力を試して動作を確認します。

![ZoomSetting](https://github.com/FujitsuSSL-LiveTalk/LiveTalkZoomSample/blob/images/Zoom06.png)

## LiveTalkをファイル連携を開始
LiveTalkの常時ファイル出力のファイル名を指定して [スタート] ボタンをクリックします。

![LiveTalkと連携したZoom字幕表示のデモムービー (YouTube)](https://youtu.be/kxqhoF8xMI0)

# 利用時の工夫
- 複数の参加者の発話をすべて字幕表示したいときは、LiveTalk の発話共有機能により複数端末の発話を全端末で共有し、いずれか１台で本サンプルを動作させて、Zoom と連携しましょう。
- [発話者表示]チェックボックスをチェックしておくと発話者名も同時に字幕に表示可能です。
- リアルタイム字幕（発話途中の表示）が必要な場合は、本サンプルの方法ではなく、LiveTalk のサブスクリーン字幕表示を活用しての字幕付き配信をお勧めします。

# 注意点
1. third-party Closed captioning service API に依頼してから字幕が表示されるまで、PROXY環境などでは、若干タイムラグが発生します。
2. 発話途中の表示（同一行の順次書換）は、third-party Closed captioning service API の仕様的には実現可能な様でしたが、実際に試してみるとタイムラグの問題で実用的ではありませんでした。そのため、本サンプルのように発話単位での字幕表示が必要となります。
3. １回の字幕で表示できる文字数は144文字前後となります。
4. 字幕表示時間の調整などはできませんので、長いと読み切れません。１文１文を簡素に話すようにしましょう。
5. Zoom 側で字幕が表示されないときは、Zoom 側の [サブタイトル表示]操作で字幕のオフ/オンをすることで再開できる場合があります。

# 連絡事項
本ソースコードは、LiveTalk の保守サポート範囲に含まれません。  
頂いた issue については、必ずしも返信できない場合があります。  
LiveTalk そのものに関するご質問は、公式WEBサイトのお問い合わせ窓口からご連絡ください。