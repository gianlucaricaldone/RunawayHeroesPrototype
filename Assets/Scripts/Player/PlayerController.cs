using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float jumpForce = 10f;
    public float laneChangeSpeed = 5f;
    public float slideDuration = 1f;
    public float laneDistance = 3f;
    public int maxLaneIndex = 1;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Dash Effect")]
    public ParticleSystem dashEffect;

    private CharacterController controller;
    private Vector3 movementDirection;
    private int currentLaneIndex = 0;
    private float targetLanePosition = 0f;
    private bool isJumping = false;
    private bool isSliding = false;
    private float slideTimer = 0f;
    private float originalHeight;
    private Vector3 originalCenter;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashDuration = 2f;
    private bool isGrounded = false;
    private float verticalVelocity = 0f;
    private float gravity = -20f;

    void Start()
    {
        // Ottieni il controller
        controller = GetComponent<CharacterController>();
        
        // Salva le dimensioni originali del controller
        originalHeight = controller.height;
        originalCenter = controller.center;
        
        // Inizializza la posizione della corsia
        currentLaneIndex = 0;
        targetLanePosition = 0f;
        
        // Inizializza la direzione di movimento
        movementDirection = Vector3.forward * forwardSpeed;
        
        Debug.Log("PlayerController inizializzato - posizione: " + transform.position);
    }

    void Update()
    {
        // Controlla se il giocatore è a terra
        CheckGrounded();
        
        // Gestisce l'input del giocatore
        HandleInput();
        
        // Applica la gravità
        ApplyGravity();
        
        // Gestisce il movimento laterale
        HandleLaneMovement();
        
        // Gestisce lo scivolamento
        HandleSliding();
        
        // Gestisce il dash
        HandleDash();
        
        // Movimento finale = vertical velocity + forward speed
        Vector3 movement = new Vector3(0, verticalVelocity, forwardSpeed) * Time.deltaTime;
        controller.Move(movement);
    }

    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Reset vertical velocity when grounded
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Piccolo valore negativo per mantenere il contatto col terreno
            isJumping = false;
        }
    }

    void HandleInput()
    {
        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isSliding)
        {
            Jump();
        }
        
        // Scivolata
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && isGrounded && !isSliding)
        {
            StartSlide();
        }
        
        // Movimento laterale
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("Left movement detected");
            MoveLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("Right movement detected");
            MoveLane(1);
        }
        
        // Dash (abilità speciale)
        if (Input.GetKeyDown(KeyCode.E) && !isDashing)
        {
            StartDash();
        }
    }

    void Jump()
    {
        verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        isJumping = true;
        Debug.Log("Jump executed: " + verticalVelocity);
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        
        // Modifica il collider per la scivolata
        controller.height = originalHeight / 2;
        controller.center = originalCenter / 2;
        
        Debug.Log("Slide started");
    }

    void EndSlide()
    {
        isSliding = false;
        
        // Ripristina le dimensioni originali del collider
        controller.height = originalHeight;
        controller.center = originalCenter;
        
        Debug.Log("Slide ended");
    }

    void MoveLane(int direction)
    {
        // Calcola l'indice di corsia target
        int targetLaneIndex = currentLaneIndex + direction;
        
        // Limita l'indice alle corsie valide
        targetLaneIndex = Mathf.Clamp(targetLaneIndex, -maxLaneIndex, maxLaneIndex);
        
        // Se la corsia target è diversa da quella corrente
        if (targetLaneIndex != currentLaneIndex)
        {
            Debug.Log($"Moving from lane {currentLaneIndex} to lane {targetLaneIndex}");
            currentLaneIndex = targetLaneIndex;
            targetLanePosition = currentLaneIndex * laneDistance;
        }
    }

    void HandleLaneMovement()
    {
        // Applica il movimento laterale con controller.Move
        if (!Mathf.Approximately(transform.position.x, targetLanePosition))
        {
            // Calcola quanto spostamento è necessario in questo frame
            float xDelta = Mathf.MoveTowards(transform.position.x, targetLanePosition, laneChangeSpeed * Time.deltaTime) - transform.position.x;
            
            // Applica solo lo spostamento laterale
            Vector3 lateralMove = new Vector3(xDelta, 0, 0);
            controller.Move(lateralMove);
            
            Debug.Log($"Lane movement: current={transform.position.x}, target={targetLanePosition}, delta={xDelta}");
        }
    }

    void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        
        // Aumenta temporaneamente la velocità
        forwardSpeed *= 1.5f;
        
        // Attiva l'effetto dash se disponibile
        if (dashEffect != null)
        {
            dashEffect.Play();
        }
        
        Debug.Log("Dash started");
    }

    void HandleDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
        }
    }

    void EndDash()
    {
        isDashing = false;
        
        // Ripristina la velocità normale
        forwardSpeed /= 1.5f;
        
        // Ferma l'effetto dash
        if (dashEffect != null)
        {
            dashEffect.Stop();
        }
        
        Debug.Log("Dash ended");
    }
    
    void HandleSliding()
    {
        if (isSliding)
        {
            // Riduci il timer della scivolata
            slideTimer -= Time.deltaTime;
            
            // Se il timer è scaduto, termina la scivolata
            if (slideTimer <= 0)
            {
                EndSlide();
            }
        }
    }
    
    // Utile per altri script per verificare se il player sta facendo dash
    public bool IsDashing()
    {
        return isDashing;
    }
}