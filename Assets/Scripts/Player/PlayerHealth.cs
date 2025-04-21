using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public float invulnerabilityDuration = 1.5f;
    
    [Header("UI Elements")]
    public Image[] healthIcons;
    
    [Header("Effects")]
    public ParticleSystem damageEffect;
    public ParticleSystem healEffect;
    
    private int currentHealth;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;
    
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }
    
    void Update()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0)
            {
                isInvulnerable = false;
            }
        }
    }
    
    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;
        
        currentHealth -= amount;
        
        // Clamp health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Play damage effect
        if (damageEffect != null)
        {
            damageEffect.Play();
        }
        
        // Trigger invulnerability
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;
        
        UpdateHealthUI();
        
        // Check for game over
        if (currentHealth <= 0)
        {
            GameOver();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        
        // Clamp health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Play heal effect
        if (healEffect != null)
        {
            healEffect.Play();
        }
        
        UpdateHealthUI();
    }
    
    void UpdateHealthUI()
    {
        // Update health icons if assigned
        if (healthIcons != null && healthIcons.Length > 0)
        {
            for (int i = 0; i < healthIcons.Length; i++)
            {
                if (healthIcons[i] != null)
                {
                    healthIcons[i].enabled = i < currentHealth;
                }
            }
        }
    }
    
    void GameOver()
    {
        // Notify GameManager
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnPlayerDeath();
        }
    }
    
    // Function to check if a collision should damage the player
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if collision is with an obstacle
        if (hit.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            TakeDamage(1);
        }
    }
}