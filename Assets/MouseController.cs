using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public float jetpackForce = 75.0f;

    public float forwardMovementSpeed = 3.0f;


    public float colliderRadius = 0.5f;

    public float groundCheckDistance = 0.2f;

    public float nearGroundDistance = 1.2f;

    public float landedHoldTime = 0.3f;


    private const int StateIdle = 0;   
    private const int StateTakeOff = 1;  
    private const int StateFly = 2;      
    private const int StateLand = 3;   
    private const int StateLanded = 4;   

    private Rigidbody2D playerRigidbody;
    private Animator animator;

    private bool wasGrounded;    
    private float landedTimer;   

    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        bool jetpackActive = Input.GetButton("Fire1");
        if (jetpackActive)
        {
            playerRigidbody.AddForce(new Vector2(0, jetpackForce));
        }

        // Keep the mouse moving forward. Only the x-component is changed so the
        // jetpack force keeps full control over vertical (y) movement.
        Vector2 newVelocity = playerRigidbody.velocity;
        newVelocity.x = forwardMovementSpeed;
        playerRigidbody.velocity = newVelocity;

        UpdateAnimation(jetpackActive);
    }
    
    void UpdateAnimation(bool jetpackActive)
    {
        if (animator == null)
        {
            return;
        }

        float distance = DistanceToGround();
        bool grounded = distance >= 0f && distance <= groundCheckDistance;
        bool nearGround = distance >= 0f && distance <= nearGroundDistance;
        float vSpeed = playerRigidbody.velocity.y;

      
        if (grounded && !wasGrounded)
        {
            landedTimer = landedHoldTime;
        }
        if (!grounded)
        {
            landedTimer = 0f; 
        }
        else if (landedTimer > 0f)
        {
            landedTimer -= Time.fixedDeltaTime;
        }
        wasGrounded = grounded;

        int state;
        if (grounded)
        {
            if (jetpackActive)
            {
                state = StateTakeOff;         
            }
            else if (landedTimer > 0f)
            {
                state = StateLanded;           
            }
            else
            {
                state = StateIdle;             
            }
        }
        else if (nearGround && vSpeed < 0f && !jetpackActive)
        {
            state = StateLand;                 
        }
        else
        {
            state = StateFly;                
        }

        animator.SetInteger("state", state);
    }


    float DistanceToGround()
    {
        
        Vector2 origin = (Vector2)transform.position + Vector2.down * (colliderRadius + 0.01f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, nearGroundDistance);
        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            return hit.distance;
        }
        return -1f;
    }
}
