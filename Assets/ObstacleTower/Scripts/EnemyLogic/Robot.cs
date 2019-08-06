using UnityEngine;

[RequireComponent(typeof(ShootProjectiles))]
//Robot Base Class to be used by all robots
public abstract class Robot : MonoBehaviour
{
    //AGENT INFO
    private bool initialized; //has this robot been initialized
    [HideInInspector] public Transform agentTransform;
    [HideInInspector] public Collider agentCollider; //capsule collider on the agent. used for collision detection
    [HideInInspector] public Vector3 dirToAgent; //the enemy's dir to the player
    [HideInInspector] public Vector3 startingPos; //starting position of the robot
    [HideInInspector] public Quaternion robotTargetRotation;


    [Header("GENERAL MOVEMENT")] public float idleBodyRotationSpeed = 15f;
    public float lookAtTargetRotationDampen = .1f;
    [HideInInspector] public Vector3 robotMoveVelocity = Vector3.zero; //velocity used for smoothdamp
    [HideInInspector] public Vector3 robotTargetPos; //target pos for the robot to go to


    //VISION SETTINGS (can the robot currenlty see the agent via raycast checks)
    [HideInInspector] public bool canSeePlayer; //can currently see player?
    private float raycastVisionTimer; //timer used for raycast vision


    //PROJECTILE SETTINGS
    [HideInInspector] public ShootProjectiles shootProjectilesController; //handles logic or shooting projectiles
    public float dotProductRelativeDirToAgent; //the dot of the robot's forward dir and dir to the agent


    void Start()
    {
        if (!initialized)
        {
            InitializeRobot();
        }
    }


    void OnEnable()
    {
        if (!initialized)
        {
            InitializeRobot();
        }
    }


    void OnDisable()
    {
        initialized = false;
    }


    //General Setup
    public virtual void InitializeRobot()
    {
        //GET STARTING POS
        startingPos = transform.position;

        //RANDOMIZE STARTING ROT
        transform.rotation *= Quaternion.Euler(0, Random.Range(0, 180), 0);

        //FIND THE AGENT
        agentTransform = GameObject.FindWithTag("agent").transform;

        //GET REF
        shootProjectilesController = GetComponent<ShootProjectiles>();

        //INITIALIZATION DONE
        initialized = true;
    }


    void FixedUpdate()
    {
        //GET CURRENT DIR TO AGENT
        dirToAgent = (agentTransform.position + new Vector3(0, .5f, 0)) - transform.position;

        //UPDATE ROBOT
        UpdateRobot();
    }


    //The child robot's logic should be implemented in the child class
    //This function will be called every FixedUpdate();
    public virtual void UpdateRobot()
    {
    }


    //Translate to robotTargetPos with moveTiming speed
    public void Move(float moveTiming)
    {
        if (robotTargetPos != transform.position)
        {
            transform.position =
                Vector3.SmoothDamp(transform.position, robotTargetPos, ref robotMoveVelocity,
                    moveTiming); //move towards target pos
        }
    }


    //Handle body rotation
    public void RotateRobot()
    {
        if (robotTargetRotation != transform.rotation)
        {
            transform.rotation = robotTargetRotation;
        }
    }


    //Raycast periodically to determine if we can "see" player
    public void LookForPlayer(float freq)
    {
        if (raycastVisionTimer > freq)
        {
            //RESET TIMER
            raycastVisionTimer = 0;

            if (agentTransform)
            {
                canSeePlayer = false; //default

                //if the robot is facing away from the agent then we don't need to raycast
                dotProductRelativeDirToAgent = Vector3.Dot(dirToAgent.normalized, transform.forward);
                if (dotProductRelativeDirToAgent < 0)
                {
                    return;
                }


                RaycastHit hit;
                if (Physics.Raycast(transform.position, dirToAgent.normalized, out hit, 20))
                {
                    //RAYCAST HIT THE PLAYER. WE CAN SEE IT
                    if (hit.transform.CompareTag("agent"))
                    {
                        canSeePlayer = true;
                    }
                }
            }
            else
            {
                //FIND THE AGENT
                agentTransform = GameObject.FindWithTag("agent").transform;
            }
        }

        //UPDATE TIMER
        raycastVisionTimer += Time.fixedDeltaTime;
    }
}