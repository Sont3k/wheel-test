using UnityEngine;

namespace _App.Scripts.Piece
{
    [System.Serializable]
    public class PieceModel
    {
        public Sprite Icon;
        public string Label;
        public AudioClip Audio;

        [Tooltip("Reward amount")] 
        public int Amount;

        public int Index { get; set; }
    }
}