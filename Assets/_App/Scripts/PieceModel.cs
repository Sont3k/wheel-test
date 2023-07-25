using UnityEngine;

namespace _App.Scripts
{
    [System.Serializable]
    public class PieceModel
    {
        public Sprite Icon;
        public string Label;
        public AudioClip Audio;

        [Tooltip("Reward amount")] 
        public int Amount;

        [Tooltip("Probability in %")]
        [Range(0f, 100f)]
        public float Chance = 100f;

        public int Index { get; set; }
        public double Weight { get; set; }
    }
}