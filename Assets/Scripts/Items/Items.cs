using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    public ItemType type;
    public float effectValue;
    public float duration;

    public enum ItemType
    {
        Heal,
        SpeedBoost,
        Shield,
        SpecialAbility
    }
}