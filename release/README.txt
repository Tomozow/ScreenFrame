ScreenFrame - 配布版

【ファイル構成】
- ScreenFrame.exe           : メインアプリケーション
- ScreenFrame.dll           : アプリケーション本体
- Hardcodet.NotifyIcon.Wpf.dll : タスクトレイアイコン用ライブラリ
- System.Text.Json.dll      : 設定保存用ライブラリ
- System.Text.Encodings.Web.dll : JSON処理用ライブラリ
- ScreenFrame.deps.json     : 依存関係情報
- ScreenFrame.runtimeconfig.json : 実行時設定

【システム要件】
- Windows 10以降
- .NET 6.0 Runtime以降（インストールされていない場合は自動的にダウンロードされます）

【使用方法】
1. ScreenFrame.exeをダブルクリックして起動
2. タスクトレイにアイコンが表示されます
3. タスクトレイアイコンを右クリックまたはダブルクリックで設定モード切替
4. 設定モードでは枠のドラッグ移動、リサイズ、色変更が可能
5. ESCキーで設定モード終了

【注意事項】
- 初回起動時に.NET Runtimeのダウンロードが必要な場合があります
- すべてのファイルを同じフォルダに保存してください
- 設定は自動的に保存されます