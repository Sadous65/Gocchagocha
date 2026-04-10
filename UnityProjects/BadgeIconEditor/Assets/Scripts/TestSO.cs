using UnityEngine;

namespace net.Dossadosa
{
    [CreateAssetMenu(fileName = "TestSO", menuName = "Scriptable Objects/TestSO")]
    public class TestSO : ScriptableObject
    {
        /// <summary>データの固有ID</summary>
        [field: Header("データの固有ID")]
        [field: SerializeField] public int Id { get; private set; }

        /// <summary>テキストデータ</summary>
        [field: Header("テキストデータ")]
        [field: Multiline(5)]
        [field: SerializeField] public string Text { get; private set; }
    }
}
