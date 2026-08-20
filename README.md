# CardAdjust

AIで生成したカード画像のレイアウトを、共通テンプレートに合わせて統一するWPFアプリ。

開発はアジャイルのスプリント単位([SP1]、[SP2]、...)で進めています。\
開発ルールは別リポジトリで一元管理しています。

- https://github.com/yamazawa/DevGuidelines

現在進行中のスプリントの仕様書・実装方針は以下を参照してください。

- `docs/SP2/カードサイズ是正アプリ_仕様書.md`
- `docs/SP2/カードサイズ是正アプリ_実装方針.md`

クローズ済みスプリントの仕様書・実装方針(実装に反映済み)は以下を参照してください。

- `docs/archive/SP1/カードサイズ是正アプリ_仕様書.md`
- `docs/archive/SP1/カードサイズ是正アプリ_実装方針.md`

## 動作環境

- .NET 9 (Windows)

## 実行方法

```powershell
dotnet run --project CardAdjust
```

## ビルド方法

```powershell
dotnet build
```
