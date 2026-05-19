# Kohei Utilities

Unity 向けの汎用ユーティリティをまとめたパッケージです。

- **対応 Unity バージョン**: 2019.4 以降
- **パッケージ名**: `com.kc-works.kohei-utils`
- **ライセンス**: MIT

## インストール

Unity Package Manager の "Add package from git URL..." から以下を指定します。

```
https://github.com/akagik/KoheiUtils.git
```

CsvConverter を含まない版を使う場合:

```
https://github.com/akagik/KoheiUtils.git#no-csvconverter
```

## 依存パッケージ

- TextMesh Pro
- Odin Inspector

## 含まれる機能

### Localization

Unity でアプリを多言語対応させるためのモジュールです。

現在サポートしている言語:

- `en` 英語
- `ja` 日本語
- `ko` 韓国語
- `zh_cn` 中国語（簡体字）
- `zh_tw` 中国語（繁体字）
- `th` タイ語
- `vi` ベトナム語

#### 使い方

1. シーンに `LocalizationManager` をアタッチしたゲームオブジェクトを配置する。
2. `LocalizationTable` スクリプタブルオブジェクトをプロジェクト内に作成する。
3. `LocalizationManager.tables` に 2 で作成したアセットをセットする。
4. ゲーム開始時に `LocalizationManager.SetLanguage` を呼び出す。

UI への適用には `LocalizedText` / `TMP_LocalizedText` / `LocalizedImage` / `LocalizedSprite` を利用します。
文字列の書式整形には [SmartFormat](https://github.com/axuno/SmartFormat) を同梱しています。

### CsvConverter

Google スプレッドシートや CSV からマスターデータのクラス・アセットを生成するエディタ拡張です。

#### 使い方

1. `Create > CsvConverter > GlobalSettings.asset` を作成する。
2. Google Cloud Platform で API key を作成する。
   - 詳細は https://qiita.com/suisuina/items/a41932088acacea4835e を参照する。
3. 1 で作成した設定ファイルに 2 で作成した apiKey をセットする。
4. `Create > CsvConverter > CsvConverterSettings` を作成し、取り込みたいシートの情報をセットする。
5. `Window > CsvConverter` を開く。
6. 取り込みたいシートで Import を実行する。

#### Join List

あるマスターテーブルの各行がさらにリスト（テーブル）を持っている場合に利用できます。
例えば、モンスターマスターがあって、各種モンスターがレベルマスター（レベルごとのパラメータ）を
持っている場合などです。

結合するベースとなるテーブルのマスタークラスには、あらかじめ以下のようなフィールドを追加しておきます。

```csharp
List<LevelMaster> levels;
```

配列では動作しない点に注意してください。

また、結合するテーブルの各行は上から順番に `levels` に追加されます。
例えば、以下のようなレベルテーブルがあったとします
（このとき `monsterId` を使ってベーステーブルと結合するとします）。

| monsterId | level | hp | atk | 備考 |
| --------- | ----- | --- | --- | --- |
| int | | int | int | |
| 1 | 1 | 10 | 5 | |
| 1 | 2 | 12 | 6 | |
| 1 | 3 | 14 | 7 | |
| 2 | 1 | 30 | 3 | |
| 2 | 2 | 40 | 4 | |
| 2 | 3 | 50 | 5 | |

上記で `level` という列は無視され、ベースのマスターの `levels` に対し、
`monsterId` ごとに上から順に Add されます。

### GSPlugin

Google スプレッドシート API v4 を利用して、スプレッドシートのデータを取得するためのモジュールです。
CsvConverter のデータ取得基盤としても利用されます。

### WAM (Weighted Alias Method)

重みに応じた抽選を行うためのクラスです。Alias Method を用いているため、
事前計算（`Setup`）後の 1 回の抽選は要素数によらず O(1) で行えます。

```csharp
// 重みの配列を渡して初期化
var wam = new WAM(new float[] { 1f, 3f, 6f });

// 重みに応じてインデックスを 1 つ抽選（この例なら 0:10%, 1:30%, 2:60%）
int index = wam.SelectOne();

// System.Random を渡してシード固定の抽選も可能
int index2 = wam.SelectOne(new System.Random(seed));

// 重みは後から更新できる（autoSetup でテーブルを再構築）
wam.SetWeight(0, 0f);
```

重みが 0 の要素は抽選されません。

### ObjectPool

`GameObject` の生成・破棄コストを抑えるためのオブジェクトプールです。
プールしたい要素には `PoolElement` をアタッチして `template` に指定します。

```csharp
// 要素を借りる（プール内の非アクティブな要素が返る）
var element = objectPool.Rent();
var bullet  = objectPool.Rent<Bullet>(); // 型を指定して取得

// 使い終わったら返却（PoolElement.Return を呼ぶ）
element.Return();

// すべての要素を一括で返却
objectPool.ReturnAll();
```

- `dynamicScale` を有効にすると、空き要素が無いときに自動でプールを拡張します。
- `inactiveOnReturn` で返却時に `GameObject` を非アクティブ化するかを切り替えられます。
- `First` / `ForEach` で貸出中の要素を走査できます。

### TouchUtils

タッチ入力とマウス入力の差異を吸収するためのユーティリティです。
実機ではタッチ入力を、エディタ / スタンドアロン / WebGL ではマウス入力を
`Touch` として扱えるため、入力処理を分岐なしで書けます。

```csharp
if (TouchUtils.touchCount > 0)
{
    Touch touch = TouchUtils.GetTouch(0);
    // touch.position / touch.deltaPosition / touch.phase などを利用
}
```

### その他のユーティリティ

`Runtime` 以下には上記以外にも各種ユーティリティが含まれています。

- **Animation**: `FlipAnimator` などのパラパラアニメーション機能
- **Extensions**: `Transform` / `GameObject` / `string` などへの拡張メソッド群
- **Save**: JSON によるセーブデータのシリアライズ機能
- **StateMachineL**: シンプルなステートマシン
- **UI**: `ToggleButton`、`RainbowUIEffect` などの UI コンポーネント
- **Utils**: `SingletonMonoBehaviour`、`SceneUtils`、`ScreenUtils` などの汎用ユーティリティ
- `DefaultDictionary` / `MyRandom` / `LogDebugger` などの汎用クラス

## ライセンス

MIT License. 詳細は [LICENSE.md](LICENSE.md) を参照してください。

## 更新履歴

[CHANGELOG.md](CHANGELOG.md) を参照してください。
