using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxInventorySlots = 4;
    
    [Header("Starting Items")]
    public string[] startingItemIds;
    
    [Header("Available Items")]
    public List<Item> allItems = new List<Item>();
    
    [Header("Effects")]
    public ParticleSystem collectEffect;
    public AudioClip collectSound;
    public AudioClip useItemSound;
    
    private List<Item> inventory = new List<Item>();
    private PlayerController playerController;
    private PlayerHealth playerHealth;
    private AudioSource audioSource;
    
    void Start()
    {
        // Get references
        playerController = FindFirstObjectByType<PlayerController>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        audioSource = GetComponent<AudioSource>();
        
        // Add starting items to inventory
        foreach (string itemId in startingItemIds)
        {
            Item item = GetItemById(itemId);
            if (item != null)
            {
                AddItem(item);
            }
        }
    }
    
    public void AddItem(Item item)
    {
        if (inventory.Count < maxInventorySlots)
        {
            inventory.Add(item);
            // Play collect effect
            if (collectEffect != null)
            {
                collectEffect.Play();
            }
            // Play collect sound
            if (audioSource != null && collectSound != null)
            {
                audioSource.PlayOneShot(collectSound);
            }
        }
    }
    
    public void UseItem(Item item)
    {
        // Apply item effect
        switch (item.type)
        {
            case Item.ItemType.Heal:
                if (playerHealth != null)
                {
                    int healAmount = Mathf.RoundToInt(item.effectValue);
                    playerHealth.Heal(healAmount);
                }
                break;
            case Item.ItemType.SpeedBoost:
                if (playerController != null)
                {
                    StartCoroutine(ApplySpeedBoost(item.effectValue, item.duration));
                }
                break;
            case Item.ItemType.Shield:
                // Implement shield logic
                break;
            case Item.ItemType.SpecialAbility:
                // Implement special ability logic
                break;
        }
        
        // Play use sound
        if (audioSource != null && useItemSound != null)
        {
            audioSource.PlayOneShot(useItemSound);
        }
        
        // Remove item from inventory
        inventory.Remove(item);
    }
    
    public List<Item> GetAvailableItems()
    {
        return inventory;
    }
    
    public Item GetItemById(string id)
    {
        return allItems.Find(item => item.itemId == id);
    }
    
    // Coroutine for temporary speed boost
    private IEnumerator ApplySpeedBoost(float multiplier, float duration)
    {
        float originalSpeed = playerController.forwardSpeed;
        playerController.forwardSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        playerController.forwardSpeed = originalSpeed;
    }
}