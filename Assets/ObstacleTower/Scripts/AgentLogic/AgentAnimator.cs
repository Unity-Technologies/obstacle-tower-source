using UnityEngine;

/// <summary>
/// Contains logic for controlling the movement and animation of the agent within the environment.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class AgentAnimator : MonoBehaviour
{
    [Header("Body Rotation")] [SerializeField]
    float m_MovingTurnSpeed = 360;

    [SerializeField] float m_StationaryTurnSpeed = 180;

    [Header("Jumping")] public bool canJump; //can this player jump
    public bool currentlyJumping; //are we currently jumping
    public float jumpPauseGroundCheckTime = .35f; //pause gc for this amount of time

    private float jumpTimePausedGroundCheckElapsed; //The time elapsed since the ground check

    // was disabled for the jumping
    public float m_JumpPower = 12f;

    [Header("Ground Check")] public float m_GroundCheckDistance = 0.35f;
    public float groundCheckSherecastRadius;
    public bool pauseGroundCheck; //are we currently pausing gc

    public bool m_IsGrounded;

    //Increase gravity using this multiplier
    [Range(1f, 10f)] [SerializeField] float m_GravityMultiplier = 2f;


    [SerializeField] float
        m_RunCycleLegOffset =
            0.2f; //specific to the character in sample assets, will need to be modified to work with others

    [Header("Running")] [SerializeField] float m_MoveSpeedMultiplier = 1f;
    [SerializeField] float m_AnimSpeedMultiplier = 1f;


    Rigidbody m_Rigidbody;
    Animator m_Animator;
    const float k_Half = 0.5f;
    float m_TurnAmount;
    float m_ForwardAmount;
    Vector3 m_GroundNormal;


    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY |
                                  RigidbodyConstraints.FreezeRotationZ;
    }

    public void ResetAnimator()
    {
        m_IsGrounded = true;
        m_ForwardAmount = 0f;
        m_TurnAmount = 0f;
        canJump = true;
        currentlyJumping = false;
        pauseGroundCheck = false;

        m_Animator.SetBool("OnGround", true);
        m_Animator.SetFloat("Forward", 0);
        m_Animator.SetFloat("Jump", 0);
        m_Animator.SetFloat("JumpLeg", 0);
        m_Animator.Play("Grounded", -1, 0f);
    }


    public void Move(Vector3 move)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction.
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);

        if (!m_IsGrounded)
        {
            AddExtraGravityForce();
        }

        CheckGroundStatus();

        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        // send input and other state parameters to the animator
        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        // update the animator parameters
        m_Animator.SetFloat("Forward", m_ForwardAmount, 0.05f, Time.deltaTime);
        m_Animator.SetBool("OnGround", m_IsGrounded);
        m_Animator.speed = m_AnimSpeedMultiplier;

        if (canJump)
        {
            if (currentlyJumping)
            {
                m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
            }
            else
            {
                m_Animator.SetFloat("Jump", 0);
            }

            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (m_IsGrounded)
            {
                m_Animator.SetFloat("JumpLeg", jumpLeg);
            }
        }
    }

    public bool CanJump()
    {
        bool can = canJump && !currentlyJumping && m_IsGrounded && m_Animator.GetFloat("Jump") == 0;

        return can;
    }

    public void Jump()
    {
        //We are now Jumping
        currentlyJumping = true;

        //Add the jump force
        m_Rigidbody.AddForce(Vector3.up * m_JumpPower, ForceMode.VelocityChange);

        //We're gonna pause ground check so player doesn't 
        //raycast the ground shortly after jump is pressed
        pauseGroundCheck = true;
    }

    private void UpdateJumpLogic()
    {
        // If the agent is currently jumping, we must check if the agent is still in the air.
        // If it is not the case, the agent is no longer jumping.
        if (currentlyJumping)
        {
            // The ground check is disabled right after the agent jumps for jumpPauseGroundCheckTime
            // seconds. This is to ensure the agent will not double jump. If the ground check is
            // paused, we must un-pause it if it has been paused for more than
            // jumpPauseGroundCheckTime seconds
            if (pauseGroundCheck)
            {
                jumpTimePausedGroundCheckElapsed += Time.fixedDeltaTime;
                if (jumpTimePausedGroundCheckElapsed >= jumpPauseGroundCheckTime)
                {
                    jumpTimePausedGroundCheckElapsed = 0;
                    pauseGroundCheck = false;
                }
            }

            // If we no longer have the ground check paused, we check if the agent is grounded,
            // if it is true, then the agent is no longer jumping.
            if (!pauseGroundCheck)
            {
                if (m_IsGrounded)
                {
                    currentlyJumping = false;
                }
            }
        }
    }

    void FixedUpdate()
    {
        UpdateJumpLogic();
    }

    //Make jumping less floaty
    void AddExtraGravityForce()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        m_Rigidbody.AddForce(extraGravityForce);
    }

    void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
        transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
    }

    public void OnAnimatorMove()
    {
        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (Time.deltaTime > 0)
        {
            //if we're on the ground we should match the animators deltaPos and zero out the y vel
            if (m_IsGrounded)
            {
                Vector3 movementVectorFromTheAnimator =
                    (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
                movementVectorFromTheAnimator.y = 0;
                m_Rigidbody.velocity =
                    Vector3.Lerp(m_Rigidbody.velocity, movementVectorFromTheAnimator,
                        .9f); //lerp to smooth it out a bit
            }
        }
    }


    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        var origen = (transform.position + (Vector3.up * 0.5f));
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(origen, .1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(origen + (Vector3.down * m_GroundCheckDistance), groundCheckSherecastRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origen, Vector3.down * m_GroundCheckDistance);
    }


    public void CheckGroundStatus()
    {
        if (pauseGroundCheck)
        {
            m_IsGrounded = false;
            return;
        }

        RaycastHit hit;
        if (Physics.SphereCast(transform.position + Vector3.up * 0.5f, 
            groundCheckSherecastRadius, Vector3.down,
            out hit, m_GroundCheckDistance))
        {
            m_IsGrounded = false;
            if ((hit.collider.CompareTag("floor") ||
                 hit.collider.CompareTag("block") ||
                 hit.collider.CompareTag("pushBlockTrigger")))
            {
                if (hit.normal.y > 0.95f)
                {
                    m_GroundNormal = hit.normal;
                    m_IsGrounded = true;
                }
            }
        }
        else
        {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
        }
    }
}
