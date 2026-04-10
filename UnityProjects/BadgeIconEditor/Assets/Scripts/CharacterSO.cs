using UnityEngine;

namespace net.Dossadosa
{
    [CreateAssetMenu(fileName = "CharacterSO", menuName = "Scriptable Objects/CharacterSO")]
    public class CharacterSO : ScriptableObject, IIconProvider
    {
        /// <summary>固有ID</summary>
        [field: Header("固有ID")]
        [field: SerializeField] public int Id { get; private set; }

        /// <summary>キャラクター名</summary>
        [field: Header("キャラクター名")]
        [field: SerializeField] public string Name { get; private set; }

        /// <summary>顔のスプライト</summary>
        [field: Header("顔のスプライト")]
        [field: SerializeField] public Sprite SpriteFace { get; private set; }

        public Sprite GetPreviewSprite() => SpriteFace;
        public Texture2D GetBadgeIcon() => Resources.Load<Texture2D>("タイトルなし____");
    }
}
