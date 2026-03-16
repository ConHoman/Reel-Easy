using UnityEngine;

[CreateAssetMenu(fileName = "NewFish", menuName = "Reel Easy/Fish Data")]
public class FishData : ScriptableObject
{
    public string fishName;
    public Sprite fishSprite;
    [Range(1, 5)] public int difficulty = 1;
    [Range(1, 4)] public int rarity = 1;
    public int scoreValue = 100;
    [TextArea] public string flavorText;
}
