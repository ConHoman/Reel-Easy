using UnityEngine;

public class FishInventory : MonoBehaviour
{
    public static FishInventory Instance;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public Sprite defaultFishSprite;

    public Sprite[] fishSprites; // list of available fish

    private InventorySlot[] slots;

    void Start()
    {
        Instance = this;

        int totalSlots = 40;
        slots = new InventorySlot[totalSlots];

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, inventoryPanel.transform);
            slots[i] = obj.GetComponent<InventorySlot>();
        }

        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }

    public void AddFish(Sprite fishSprite)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.icon.sprite == null)
            {
                slot.SetItem(fishSprite);
                return;
            }
        }
    }
}
