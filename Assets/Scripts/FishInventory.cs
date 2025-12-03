using UnityEngine;

public class FishInventory : MonoBehaviour
{
    public GameObject inventoryPanel;   // The UI panel
    public GameObject slotPrefab;       // Your Slot prefab
    public Sprite defaultFishSprite;    // Temporary fish icon

    private InventorySlot[] slots;

    void Start()
    {
        int totalSlots = 40;  // 5x3 grid
        slots = new InventorySlot[totalSlots];

        // Create all slot objects
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject obj = Instantiate(slotPrefab, inventoryPanel.transform);
            slots[i] = obj.GetComponent<InventorySlot>();
        }

        inventoryPanel.SetActive(false); // Hide at start
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }

    // Call this when catching a fish
    public void AddFish(Sprite fishSprite)
    {
        foreach (InventorySlot slot in slots)
        {
            // If the slot is empty
            if (slot.icon.sprite == null)
            {
                slot.SetItem(fishSprite);
                return;
            }
        }
    }
}
