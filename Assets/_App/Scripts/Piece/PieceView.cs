using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _App.Scripts.Piece
{
    public class PieceView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _amount;

        [Header("Parameters")]
        [SerializeField] private Color _disabledColor;

        public PieceModel Data { get; private set; }
        private Action<PieceView> _onClickCallback;

        public void SetData(PieceModel data, Action<PieceView> onClickCallback)
        {
            Data = data;
            _icon.sprite = data.Icon;
            _label.text = data.Label;
            _amount.text = data.Amount.ToString();
            _onClickCallback = onClickCallback;
            _button.onClick.AddListener(OnPieceClick);
        }
        
        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        private void OnPieceClick()
        {
            _onClickCallback?.Invoke(this);
        }

        public void DisablePiece()
        {
            _button.interactable = false;
            _icon.color = _disabledColor;
            _label.color = _disabledColor;
            _amount.color = _disabledColor;
        }
    }
}