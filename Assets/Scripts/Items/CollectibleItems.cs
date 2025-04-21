using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public string itemId = "coin";
    public int value = 1;
    public bool rotateObject = true;
    public float rotationSpeed = 100f;
    public bool bobUpAndDown = true;
    public float bobHeight = 0.5f;
    public float bobSpeed = 1f;
    public ParticleSystem collectEffect;
    public AudioClip collectSound;

    private Vector3 startPosition;
    private float bobTime;

    void Start()
    {
        startPosition = transform.position;
        bobTime = Random.Range(0f, 2f * Mathf.PI); // Random start position in the bob cycle
        
        // Set layer to Collectible
        gameObject.layer = LayerMask.NameToLayer("Collectible");
    }

    void Update()
    {
        // Rotate the object
        if (rotateObject)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        // Bob the object up and down
        if (bobUpAndDown)
        {
            bobTime += bobSpeed * Time.deltaTime;
            float bobOffset = Mathf.Sin(bobTime) * bobHeight;
            transform.position = startPosition + new Vector3(0f, bobOffset, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if player entered the trigger
        if (other.CompareTag("Player"))
        {
            // Find inventory manager
            InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();
            
            // Add item to inventory if possible
            if (inventoryManager != null)
            {
                Item item = inventoryManager.GetItemById(itemId);
                if (item != null)
                {
                    inventoryManager.AddItem(item);
                }
            }
            
            // Play collect effect
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }
            
            // Play collect sound
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
            
            // Destroy the collectible
            Destroy(gameObject);
        }
    }
}