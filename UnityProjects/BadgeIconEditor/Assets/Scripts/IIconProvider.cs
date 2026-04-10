using UnityEngine;

namespace net.Dossadosa
{
    public interface IIconProvider
    {
        /// <summary>アイコンとして表示させたいSpriteを返す</summary>
        Sprite GetPreviewSprite();

        /// <summary>
        /// アイコン上に表示するバッチ画像を返す<br/>
        /// バッチ不要であればnullを返す
        /// </summary>
        Texture2D GetBadgeIcon();
    }
}
