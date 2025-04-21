using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 3f;
    public int damageAmount = 1;
    public int healthPoints = 2;

    [Header("Movement Pattern")]
    public bool moveLeftRight = true;
    public float movementRange = 3f;
    public float movementSpeed = 2f;

    [Header("Attack Pattern")]
    public bool canAttack = true;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    [Header("Effects")]
    public ParticleSystem hitEffect;
    public ParticleSystem deathEffect;
    public AudioClip hitSound;
    public AudioClip deathSound;

    private Vector3 startPosition;
    private float movementOffset;
    private bool isDead = false;
    private float lastAttackTime;
    private Transform playerTransform;
    private AudioSource audioSource;

    void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Set layer to Enemy
        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    void Update()
    {
        if (isDead) return;
        
        // Move forward at constant speed
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        
        // Left-right movement pattern
        if (moveLeftRight)
        {
            movementOffset = Mathf.Sin(Time.time * movementSpeed) * movementRange;
            transform.position = new Vector3(
                startPosition.x + movementOffset,
                transform.position.y,
                transform.position.z
            );
        }
        
        // Check if player is in attack range
        if (canAttack && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    void Attack()
    {
        // Implement attack logic here
        // For example, launch a projectile or animate an attack
    }

    public void TakeDamage(int amount)
    {
        healthPoints -= amount;
        
        // Play hit effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        
        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        if (healthPoints <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        
        // Play death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        
        // Destroy the enemy
        Destroy(gameObject, 0.2f); // Small delay to allow effects to play
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if collision is with player
        if (other.CompareTag("Player"))
        {
            // Deal damage to player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
}