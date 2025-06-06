using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public int damageAmount = 1;
    public bool isBreakable = false;
    public int health = 1;
    public bool canBeJumpedOver = true;
    public bool canBeSlidUnder = false;

    [Header("Effects")]
    public ParticleSystem breakEffect;
    public AudioClip breakSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Controlla se il layer esiste prima di assegnarlo
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        if (obstacleLayer != -1)
        {
            gameObject.layer = obstacleLayer;
        }
        else
        {
            Debug.LogWarning("Layer 'Obstacle' not found! Using default layer instead.");
            // Potresti usare un altro layer esistente o semplicemente non fare nulla
        }
    }

    public void TakeDamage(int amount)
    {
        if (!isBreakable) return;
        
        health -= amount;
        if (health <= 0)
        {
            Break();
        }
    }

    void Break()
    {
        // Play break effect
        if (breakEffect != null)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }
        
        // Play break sound
        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound);
        }
        
        // Destroy the obstacle
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if collision is with player
        if (collision.gameObject.CompareTag("Player"))
        {
            // If player has dash ability active, the obstacle breaks
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null && isBreakable && playerController.IsDashing())
            {
                Break();
                return;
            }
            
            // Otherwise, player takes damage
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
}