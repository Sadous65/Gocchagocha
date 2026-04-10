using UnityEditor;
using UnityEngine;

namespace net.Dossadosa
{
    /// <summary>
    /// ScriptableObject のプレビューにバッチアイコンを合成するエディタ拡張
    /// </summary>
    [CustomEditor(typeof(ScriptableObject), true)]
    public class BadgeIconEditor : Editor
    {
        // バッチアイコンのサイズと位置を調整するための定数
        private const float BadgeScale = 0.45f;
        private const float BadgeMargin = 0.03f;

        /// <summary>
        /// ScriptableObject のプレビューを描画する際に呼び出されるメソッドをオーバーライド<br/>
        /// IIconProvider を実装している場合は、プレビューにバッチアイコンを合成して表示する
        /// </summary>
        public override Texture2D RenderStaticPreview(
            string assetPath, Object[] subAssets, int width, int height)
        {
            if (target is not IIconProvider provider)
                return base.RenderStaticPreview(assetPath, subAssets, width, height);

            Texture2D badge = provider.GetBadgeIcon();

            Sprite sprite = provider.GetPreviewSprite();
            if (sprite == null)
            {
                // Sprite未セット時はデフォルトプレビューにバッジを合成
                Texture2D defaultPreview = base.RenderStaticPreview(assetPath, subAssets, width, height);
                if (badge == null) return defaultPreview;

                // base が null を返す場合は無地テクスチャをベースにする
                if (defaultPreview == null)
                {
                    defaultPreview = new Texture2D(width, height, TextureFormat.ARGB32, false);
                    Color[] pixels = new Color[width * height];
                    System.Array.Fill(pixels, new Color(0.2f, 0.2f, 0.2f, 1f));
                    defaultPreview.SetPixels(pixels);
                    defaultPreview.Apply();
                }

                return CompositeBadge(defaultPreview, badge, width, height);
            }

            Texture2D basePreview = RenderSpritePreview(sprite, width, height);
            if (basePreview == null)
                return base.RenderStaticPreview(assetPath, subAssets, width, height);

            return badge != null
                ? CompositeBadge(basePreview, badge, width, height)
                : basePreview;
        }

        /// <summary>
        /// ベースのプレビューとバッチアイコンを合成して新しいテクスチャを生成する
        /// </summary>
        private static Texture2D CompositeBadge(
            Texture2D baseTexture, Texture2D badge, int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, width, height, 0);
            Graphics.DrawTexture(new Rect(0, 0, width, height), baseTexture);

            int badgeSize = Mathf.RoundToInt(width * BadgeScale);
            int margin = Mathf.RoundToInt(width * BadgeMargin);
            Graphics.DrawTexture(
                new Rect(width - badgeSize - margin, height - badgeSize - margin, badgeSize, badgeSize), //右下
                badge, new Rect(0, 0, 1, 1), 0, 0, 0, 0);
            // new Rect(margin, height - badgeSize - margin, badgeSize, badgeSize), //左下

            GL.PopMatrix();

            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            RenderTexture.active = prev;
            Object.DestroyImmediate(rt);
            return result;
        }

        /// <summary>
        /// Sprite を指定されたサイズのテクスチャにレンダリングする<br/>
        /// テクスチャが読み取り可能な場合は CPU で、そうでない場合は GPU でレンダリングする
        /// </summary>
        private static Texture2D RenderSpritePreview(Sprite sprite, int width, int height)
        {
            // isReadable=true かつ非圧縮 → CPUパス
            if (sprite.texture.isReadable && !IsCompressed(sprite.texture))
                return RenderSpritePreviewCPU(sprite, width, height);

            // isReadable=false は警告（本来は Read/Write Enabled にすべき）
            if (!sprite.texture.isReadable)
                Debug.LogWarning(
                    $"[BadgeIconEditor] {sprite.texture.name} は Read/Write が無効です。" +
                    $"Texture Import Settings で Read/Write Enabled を有効にすることを推奨します。");

            // 圧縮テクスチャ または isReadable=false → GPUパス
            return RenderSpritePreviewGPU(sprite, width, height);
        }

        private static bool IsCompressed(Texture2D texture)
        {
            switch (texture.format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT5:
                case TextureFormat.DXT1Crunched:  // 追加
                case TextureFormat.DXT5Crunched:  // 追加
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ETC_RGB4Crunched:   // 追加
                case TextureFormat.ETC2_RGBA8Crunched: // 追加
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGBA4:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// isReadable=true かつ非圧縮テクスチャ用
        /// </summary>
        private static Texture2D RenderSpritePreviewCPU(Sprite sprite, int width, int height)
        {
            Rect srcRect = sprite.textureRect;
            Texture2D src = sprite.texture;
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float u = srcRect.x + (x / (float)width) * srcRect.width;
                    float v = srcRect.y + (y / (float)height) * srcRect.height;
                    result.SetPixel(x, y, src.GetPixelBilinear(u / src.width, v / src.height));
                }

            result.Apply();
            return result;
        }

        /// <summary>
        /// 圧縮テクスチャ または isReadable=false 用。
        /// Graphics.Blit の座標系は Y 軸下原点のため ReadPixels で上下反転するが、
        /// CompositeBadge の GL.LoadPixelMatrix(0, width, height, 0) が上原点で統一されているため
        /// baseTexture として渡した時点で再反転され、最終出力は正立する。
        /// </summary>
        private static Texture2D RenderSpritePreviewGPU(Sprite sprite, int width, int height)
        {
            Rect srcRect = sprite.textureRect;
            Texture2D src = sprite.texture;

            Vector2 scale = new Vector2(srcRect.width / src.width,
                                         srcRect.height / src.height);
            Vector2 offset = new Vector2(srcRect.x / src.width,
                                         srcRect.y / src.height);

            RenderTexture spriteRT = new RenderTexture(
                (int)srcRect.width, (int)srcRect.height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previewRT = new RenderTexture(
                width, height, 0, RenderTextureFormat.ARGB32);

            Graphics.Blit(src, spriteRT, scale, offset);
            Graphics.Blit(spriteRT, previewRT);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = previewRT;
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = prev;

            Object.DestroyImmediate(spriteRT);
            Object.DestroyImmediate(previewRT);
            return result;
        }
    }
}