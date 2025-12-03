using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon;

    public void SetItem(Sprite sprite)
    {
        icon.sprite = sprite;
        icon.color = Color.white;
    }

    public void Clear()
    {
        icon.sprite = null;
        icon.color = new Color(1, 1, 1, 0);
    }
}
