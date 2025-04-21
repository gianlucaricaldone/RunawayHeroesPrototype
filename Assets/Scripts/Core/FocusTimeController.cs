using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FocusTimeController : MonoBehaviour
{
    [Header("Focus Time Settings")]
    public float timeSlowFactor = 0.3f;
    public float maxFocusDuration = 10f;
    public float cooldownDuration = 25f;
    public float drainRate = 1f;

    [Header("UI Elements")]
    public Image focusMeterFill;
    public Image cooldownFill;
    public GameObject itemWheelParent;
    public Transform[] itemSlots;

    [Header("Effects")]
    public ParticleSystem focusActivateEffect;

    private bool isFocusActive = false;
    private float focusTimeRemaining;
    private bool isCooldown = false;
    private float cooldownRemaining;
    private List<Item> availableItems = new List<Item>();
    private int selectedItemIndex = -1;

    // Reference to the original time scale
    private float originalTimeScale;
    private InventoryManager inventoryManager;

    void Start()
    {
        // Store original time scale
        originalTimeScale = Time.timeScale;
        // Initialize focus time
        focusTimeRemaining = maxFocusDuration;
        // Find inventory manager
        inventoryManager = FindFirstObjectByType<InventoryManager>();
        // Initially hide item wheel
        if (itemWheelParent != null)
        {
            itemWheelParent.SetActive(false);
        }
        // Update UI
        UpdateFocusUI();
    }

    void Update()
    {
        // Check for input only if not in cooldown
        if (!isCooldown && Input.GetKeyDown(KeyCode.F) && !isFocusActive)
        {
            ActivateFocusTime();
        }
        else if (Input.GetKeyUp(KeyCode.F) && isFocusActive)
        {
            DeactivateFocusTime();
        }

        // Handle focus time duration
        if (isFocusActive)
        {
            focusTimeRemaining -= drainRate * Time.unscaledDeltaTime;
            if (focusTimeRemaining <= 0)
            {
                focusTimeRemaining = 0;
                DeactivateFocusTime();
                StartCooldown();
            }
        }

        // Handle cooldown
        if (isCooldown)
        {
            cooldownRemaining -= Time.deltaTime;
            if (cooldownRemaining <= 0)
            {
                isCooldown = false;
                focusTimeRemaining = maxFocusDuration;
            }
        }

        // Update UI
        UpdateFocusUI();
    }

    void ActivateFocusTime()
    {
        isFocusActive = true;
        // Slow down time
        Time.timeScale = timeSlowFactor;
        // Ensure fixed delta time is adjusted as well
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        
        // Get available items from inventory
        if (inventoryManager != null)
        {
            availableItems = inventoryManager.GetAvailableItems();
        }
        
        // Show item wheel
        if (itemWheelParent != null)
        {
            itemWheelParent.SetActive(true);
            UpdateItemWheel();
        }
        
        // Play activation effect
        if (focusActivateEffect != null)
        {
            focusActivateEffect.Play();
        }
    }

    void DeactivateFocusTime()
    {
        isFocusActive = false;
        // Return to normal time
        Time.timeScale = originalTimeScale;
        // Reset fixed delta time
        Time.fixedDeltaTime = 0.02f;
        
        // Hide item wheel
        if (itemWheelParent != null)
        {
            itemWheelParent.SetActive(false);
        }
        
        // Use selected item if any
        if (selectedItemIndex >= 0 && selectedItemIndex < availableItems.Count)
        {
            UseItem(availableItems[selectedItemIndex]);
        }
        
        // Reset selection
        selectedItemIndex = -1;
    }

    void StartCooldown()
    {
        isCooldown = true;
        cooldownRemaining = cooldownDuration;
    }

    void UpdateFocusUI()
    {
        // Update focus meter
        if (focusMeterFill != null)
        {
            focusMeterFill.fillAmount = focusTimeRemaining / maxFocusDuration;
        }
        
        // Update cooldown indicator
        if (cooldownFill != null)
        {
            cooldownFill.gameObject.SetActive(isCooldown);
            if (isCooldown)
            {
                cooldownFill.fillAmount = cooldownRemaining / cooldownDuration;
            }
        }
    }

    void UpdateItemWheel()
    {
        // Update item slots with available items
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < availableItems.Count)
            {
                // Item is available
                Item item = availableItems[i];
                // Update item slot
                Transform slot = itemSlots[i];
                
                // Find and update image
                Image itemImage = slot.Find("ItemIcon")?.GetComponent<Image>();
                if (itemImage != null)
                {
                    itemImage.sprite = item.icon;
                    itemImage.enabled = true;
                }
                
                // Find and update text
                Text itemText = slot.Find("ItemName")?.GetComponent<Text>();
                if (itemText != null)
                {
                    itemText.text = item.itemName;
                }
                
                // Make slot visible
                slot.gameObject.SetActive(true);
            }
            else
            {
                // No item for this slot
                itemSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectItem(int index)
    {
        selectedItemIndex = index;
    }

    void UseItem(Item item)
    {
        if (inventoryManager != null)
        {
            inventoryManager.UseItem(item);
        }
    }
}