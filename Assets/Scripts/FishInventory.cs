using UnityEngine;

public class FishInventory : MonoBehaviour
{
    public static FishInventory Instance;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;

    private InventorySlot[] slots;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
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
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void AddFish(FishData fish)
    {
        if (slots == null) return;
        foreach (InventorySlot slot in slots)
        {
            if (slot.icon.color.a == 0f)
            {
                slot.SetItem(fish.fishSprite);
                return;
            }
        }
    }

    public void ResetInventory()
    {
        if (slots == null) return;
        foreach (InventorySlot slot in slots)
            slot.Clear();
    }
}
