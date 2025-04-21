using UnityEngine;
using System.Collections.Generic;

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
    private Dictionary<int, float> touchStartTimes = new Dictionary<int, float>();

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;
        originalCenter = controller.center;
        // Initialize movement direction
        movementDirection = Vector3.forward * forwardSpeed;
    }

    void Update()
    {
        CheckGrounded();
        HandleInput();
        ApplyGravity();
        HandleLaneMovement();
        HandleSliding();
        HandleDash();
        // Apply final movement
        Vector3 movement = new Vector3(0, verticalVelocity, forwardSpeed) * Time.deltaTime;
        controller.Move(movement);
    }

    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        // Reset vertical velocity when grounded
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Small negative value to keep grounded
            isJumping = false;
        }
    }

    void HandleInput()
    {
        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded && !isSliding)
        {
            Jump();
        }
        
        // Slide
        if (Input.GetKeyDown(KeyCode.S) && isGrounded && !isSliding)
        {
            StartSlide();
        }
        
        // Lane movement
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveLane(1);
        }
        
        // Dash (special ability)
        if (Input.GetKeyDown(KeyCode.E) && !isDashing)
        {
            StartDash();
        }
        
        // Mobile touch controls
        if (Input.touchCount > 0) {

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                int fingerId = touch.fingerId;
                
                // Registra il tempo di inizio quando un tocco inizia
                if (touch.phase == TouchPhase.Began)
                {
                    touchStartTimes[fingerId] = Time.time;
                    
                    // Tap to jump
                    if (isGrounded && !isSliding)
                    {
                        Jump();
                    }
                }
                
                // Gestisci i movimenti di swipe
                else if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 deltaPosition = touch.deltaPosition;
                    
                    if (deltaPosition.y > 50) // Swipe up
                    {
                        Jump();
                    }
                    else if (deltaPosition.y < -50) // Swipe down
                    {
                        StartSlide();
                    }
                    else if (deltaPosition.x < -50) // Swipe left
                    {
                        MoveLane(-1);
                    }
                    else if (deltaPosition.x > 50) // Swipe right
                    {
                        MoveLane(1);
                    }
                }
                
                // Gestisci il tocco prolungato per Focus Time
                else if (touch.phase == TouchPhase.Stationary)
                {
                    // Verifica se abbiamo registrato questo tocco
                    if (touchStartTimes.ContainsKey(fingerId))
                    {
                        // Calcola quanto tempo è passato dall'inizio del tocco
                        float touchDuration = Time.time - touchStartTimes[fingerId];
                        
                        // Se il tocco è stato mantenuto abbastanza a lungo
                        if (touchDuration > 0.5f)
                        {
                            // Attiva Focus Time (gestito da FocusTimeController)
                            // Qui potresti inviare un evento o chiamare un metodo su FocusTimeController
                            
                            // Opzionale: rimuovi questo fingerId per non attivare continuamente
                            // touchStartTimes.Remove(fingerId);
                        }
                    }
                }
                
                // Pulisci il dizionario quando un tocco termina
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    if (touchStartTimes.ContainsKey(fingerId))
                    {
                        touchStartTimes.Remove(fingerId);
                    }
                }
            }
        }
    }

    void Jump()
    {
        verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        isJumping = true;
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        // Adjust collider for sliding
        controller.height = originalHeight / 2;
        controller.center = originalCenter / 2;
    }

    void EndSlide()
    {
        isSliding = false;
        // Restore original collider
        controller.height = originalHeight;
        controller.center = originalCenter;
    }

    void HandleSliding()
    {
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                EndSlide();
            }
        }
    }

    void MoveLane(int direction)
    {
        int targetLaneIndex = currentLaneIndex + direction;
        // Clamp to valid lanes
        targetLaneIndex = Mathf.Clamp(targetLaneIndex, -maxLaneIndex, maxLaneIndex);
        if (targetLaneIndex != currentLaneIndex)
        {
            currentLaneIndex = targetLaneIndex;
            targetLanePosition = currentLaneIndex * laneDistance;
        }
    }

    void HandleLaneMovement()
    {
        // Smoothly move toward target lane
        Vector3 targetPosition = new Vector3(targetLanePosition, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(targetPosition.x, transform.position.y, transform.position.z),
            laneChangeSpeed * Time.deltaTime
        );
    }

    void ApplyGravity()
    {
        verticalVelocity += gravity * Time.deltaTime;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        // Temporarily increase speed
        forwardSpeed *= 1.5f;
        // Play dash effect if available
        if (dashEffect != null)
        {
            dashEffect.Play();
        }
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
        // Reset speed to normal
        forwardSpeed /= 1.5f;
        // Stop dash effect
        if (dashEffect != null)
        {
            dashEffect.Stop();
        }
    }

    // Public accessor for other scripts to check dash state
    public bool IsDashing()
    {
        return isDashing;
    }
}