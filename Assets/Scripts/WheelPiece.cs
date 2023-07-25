using UnityEngine;

[System.Serializable]
public class WheelPiece
{
    public Sprite Icon;
    public string Label;

    [Tooltip("Reward amount")] 
    public int Amount;

    [Tooltip("Probability in %")]
    [Range(0f, 100f)]
    public float Chance = 100f;

    public int Index { get; set; }
    public double Weight { get; set; }
}