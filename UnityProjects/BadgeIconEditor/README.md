# ScriptableObjectのプレビューアイコン表示拡張

クロードに全任せした結果

- IIconProviderを実装したScriptableObjectは問答無用でプレビュー表示が変わる
- Texture2DはRead/WriteがTrueでもFalseでも問題ない
- Texture2Dは圧縮してても問題ない
- リフレクションは使ってない

ってかんじになった。よくわかんないです。

## 参考
- [ライフハック：UnityでScriptableObjectsの表示画像を変更 - x.com](https://x.com/Phantom_TheGame/status/2041837556052975677)
- [Tutorials/CustomScriptableObjectIcons/Editor/RuneEditor.cs](https://github.com/Vinark117/Tutorials/blob/main/CustomScriptableObjectIcons/Editor/RuneEditor.cs)
