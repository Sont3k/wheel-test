using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _App.Scripts.Piece
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _amount;

        public Image Icon => _icon;
        public TMP_Text Label => _label;
        public TMP_Text Amount => _amount;

        public void SetData(Sprite icon, string label, string amount)
        {
            _icon.sprite = icon;
            _label.text = label;
            _amount.text = amount;
        }
    }
}