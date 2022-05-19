# Changelog
All notable changes to this package will be documented in this file. The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)

## [0.3.2] - 2022-05-19

- [feat] LocalizationData に ko を追加
- [feat] CsvConverter で AudioClip をインポートできるようにした

## [0.3.1] - 2021-12-26

- [feat] LocalizationManager の Setup のタイミングを指定できるようにした

## [0.3.0] - 2021-11-02

- [feat] CsvConverterSettings を１ファイル＝１設定にするようにした
  - 古い設定ファイルは動かなくなるので、KoheiUtils > CsvConverter > Convert Settings を動かす必要あり
- [fix] Join 後の自動保存設定
- [fix] Join 自動実行設定 

## [0.2.7] - 2021-[0.2.7] - 2021-10-24

- [fix] ログの出力設定を可能にした

## 10-24

- [fix] ログの出力設定を可能にした

## [0.2.6] - 2021-10-13

- [fix] Text Mesh Pro のエラー修正

## [0.2.5] - 2021-10-09

- [fix] 細かい調整

## [0.2.4] - 2021-10-07

- [fix] ビルドエラー対応

## [0.2.3] - 2021-09-25

- [feat] Searchable 対応
- [feat] DefaultDictionary 追加
- [refactor] LocalizationManager を LookUpTable と分離

## [0.2.2] - 2021-09-20

- [fix] CsvConverter 改善

## [0.2.1] - 2021-09-14

- [feat] StateMachineViewer の改善

## [0.2.0] - 2021-08-20

- [feat] Google SpreadSheet api v4 に対応

## [0.1.9] - 2021-08-14

- [fix] Windows 対応や Odin 周りのエラー修正

## [0.1.8] - 2021-06-17

- Play onEnd が先にコールされるようにした

## [0.1.7] - 2021-06-17
- FlipAnimator の onEnd コール中の Play による無限ループを改善

## [0.1.6] - 2021-06-16
- StateMachine.State のパラメータの渡し方を変更
- StateMachine の各種メソッドの名前を変更

## [0.1.5] - 2021-06-02
- Fix FlipAnimator loop logic miss

## [0.1.4] - 2021-06-01
- FlipAnimator HasAnimation added
- FlipAnimator TrackEntry impl

## [0.1.3] - 2021-05-11
- CsvConverter join table

## [0.1.2] - 2021-04-14
- FlipAnimator.OnEnd added
- CsvConverter import button added
- CsvConverter edit/duplicate settings button added
- CsvConverter code destination property added

## [0.1.1] - 2021-04-09
- Add FlipAnimator dictionary version

## [0.0.11] - 2021-04-02
- Add playOnStart to FlipAnimator

## [0.0.10] - 2021-03-16
- Improve fsm viewer.


## [0.0.9] - 2021-03-15

### Fixed
- Fix -1 default loop index

## [0.0.8] - 2021-03-12

### Fixed
- Fix FlipAnimator.OnComplete null exception

## [0.0.7] - 2021-03-12

### Features
- Handle complete event in FlipAnimator

## [0.0.6] - 2021-03-10

### Features
- Handle events in FlipAnimator

## [0.0.5] - 2021-02-20

### Features
- Add StateMachineL
- Add ObjectPool
- Add FlipAnimator
- Refine flip animation
- Csv Converter 改行を含む csv に対応

## [0.0.4] - 2021-02-20

### Fixed
- Add Localization

## [0.0.3] - 2021-02-14

### Fixed
- Add GSPlugin and CsvConverter


## [0.0.2] - 2021-02-13

### Fixed
- Duplicant GUID Changed

## [0.0.1] - 2021-02-13

### Fixed
- First Changed
