# Kohei Utilities
This package is various utilities for unity.

このパッケージには以下の機能が含まれれます:

- Localization
- CsvConverter
- GSPlugin
- その他 Unity のユーティリティ

## Localization

Unity で言語ローカライズをするためのモジュール.
現段階でサポートしている言語は以下の通り:
- en
- ja

Format で SmarFormat を使う場合は下記シンボルをつけて、さらに Unity の Localization パッケージをインポートする.

KU_SMART_FORMAT

https://docs.unity3d.com/Packages/com.unity.localization@1.3/manual/Smart/SmartStrings.html

## Requirements
- TMPro
- Odin


## How To Use
1. シーンに LocalizationManager をアタッチしたゲームオブジェクトを配置する.
2. LocalizationTable スクリプタブルオブジェクトをプロジェクト内に作成する.
3. LocalizationManager.tables に 2 で作成したアセットをセットする.
4. ゲーム開始時に LocalizationManager.SetLanguage を呼び出す.


## CsvConverter

### Getting Started

1. Create > CsvConverter > GlobalSettings.asset を作成
2. Google Cloud Platform で API key を作成
   1. 詳細は https://qiita.com/suisuina/items/a41932088acacea4835e を参照する
3. 1で作成した設定ファイルに 2で作成した apiKey をセットする.
4. Create > CsvConverter > CsvConverterSettings を作成する
   1. 取り込みたいシートの情報をセットする
5. Window > CsvConverter を開く
6. 取り込みたいシートで Import を実行する.

### Join List

あるマスターテーブルの各行がさらにリスト（テーブル）を持っている場合に利用できる.
例えば、モンスターマスターがあって、各種モンスターはレベルマスター（レベルごとのパラメータ）を
持っている場合など。

結合するベースとなるテーブルのマスタークラスには以下のようなフィールドをあらかじめ追加しておく:

```csharp
List<*LevelMaster*> *levels*;
```

配列では駄目な点に注意.

また結合するテーブルの各行は上から順番に levels に追加される点に注意.
例えば、以下ようなレベルテーブルがあったとする.
(このとき monsterId を使ってベーステーブルと結合するとする.)

| monsterId | level | hp | atk | 備考 |
| ------- | ---- | --- | --------- | --- |
| int | | int | int |     |
| 1 | 1 | 10 | 5 |  |
| 1 | 2 | 12 | 6 |  |
| 1 | 3 | 14 | 7 |  |
| 2 | 1 | 30 | 3 |  |
| 2 | 2 | 40 | 4 |  |
| 2 | 3 | 50 | 5 |  |

上記で level という列は無視されベースのマスターの levels にはこの表で、 monsterId ごとに
上から順に Add される.


## GSPlugin
