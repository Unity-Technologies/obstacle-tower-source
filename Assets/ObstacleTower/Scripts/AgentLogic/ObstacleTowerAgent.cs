using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;


/// <summary>
/// Agent logic. Responsible for moving agent, assigning rewards, and going between floors.
/// </summary>
[RequireComponent(typeof(AgentAnimator))]
public class ObstacleTowerAgent : Agent
{
    public FloorBuilder floorBuilder;
    public KeyController keyController;
    public Transform cameraPivot; //the object that contains the camera
    public Camera cameraAgent;
    public Camera cameraPlayer;
    public Canvas canvasPlayer;
    public float cameraFollowSpeed;
    public bool denseReward;

    [Header("Episode Time Config")] 
    public int floorTimeBonus;
    public int floorTimeStart;
    public int orbBonus;

    private AgentAnimator agentAnimator; // A reference to the ThirdPersonCharacter on the object
    private Vector3 dirToGo; // the dir the char should go
    private Vector3 rotateDir; // the dir the camera should rotate
    public Rigidbody agentRb;
    private bool jumping;
    private int episodeTime;
    private bool runTimer;

    //Events
    public event Action CompletedFloorAction; //event that will fire if the agent completes the floor

    private List<Collision> _collisions = new List<Collision>();

    [HideInInspector] public UIController uIController;

    public void SetTraining()
    {
        cameraAgent.enabled = false;
        cameraPlayer.enabled = false;
        canvasPlayer.enabled = false;
    }

    public void SetInference()
    {
        cameraAgent.enabled = false;
        cameraPlayer.enabled = true;
        canvasPlayer.enabled = true;
    }

    public override void Initialize()
    {
        runTimer = true;
        agentRb = GetComponent<Rigidbody>();
        agentAnimator = GetComponent<AgentAnimator>();
        uIController = FindObjectOfType<UIController>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddOneHotObservation(keyController.currentNumberOfKeys, 6);
        sensor.AddObservation(episodeTime);
        sensor.AddObservation(floorBuilder.floorNumber);
    }

    private void PickUpKey(GameObject key)
    {
        keyController.AddKey();
        Destroy(key);
    }

    private void PickUpOrb(GameObject orb)
    {
        episodeTime += orbBonus;
        Destroy(orb);
    }

    public void AgentNewFloor()
    {
        try
        {
            floorBuilder.ResetFloor();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
#if UNITY_EDITOR
            Debug.LogError("There was an error instantiating the floor. Leaving play-mode");
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
        }
    }

    private void CompletedLevel()
    {
        CompletedFloorAction?.Invoke(); //fire the event

        AddReward(1f);
        floorBuilder.IncrementFloorNumber();
        episodeTime += floorTimeBonus;
        AgentNewFloor();
    }

    private void OnCollisionEnter(Collision col)
    {
        _collisions.Add(col);
    }

    private bool ProcessCollision(Collision col)
    {
        if (col.gameObject.CompareTag("exit"))
        {
            CompletedLevel();
            return true;
        }

        if (col.gameObject.CompareTag("hazard"))
        {
            if (uIController)
            {
                uIController.ShowKillScreen();
            }
            EndEpisode();
            return true;
        }

        if (col.gameObject.CompareTag("enemy"))
        {
            if (uIController)
            {
                uIController.ShowKillScreen();
            }

            EndEpisode();
            return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("key"))
        {
            PickUpKey(col.gameObject);
            if (denseReward) AddReward(0.1f);
            Destroy(col.gameObject);
        }

        if (col.gameObject.CompareTag("orb"))
        {
            PickUpOrb(col.gameObject);
        }

        if (col.gameObject.CompareTag("fake"))
        {
            Destroy(col.gameObject);
        }

        if (col.gameObject.CompareTag("doorZone"))
        {
            DoorLogic doorController = col.transform.GetComponent<DoorLogic>();
            if (doorController)
            {
                doorController.TryOpenDoor();
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("doorZone"))
        {
            DoorLogic doorController = col.transform.GetComponent<DoorLogic>();
            if (doorController)
            {
                doorController.TryCloseDoor();
            }
        }
    }

    private void MoveAgent(float[] act)
    {
        dirToGo = Vector3.zero;
        rotateDir = Vector3.zero;

        var forwardAction = Mathf.FloorToInt(act[0]);
        var rotateAction = Mathf.FloorToInt(act[1]);
        var jumpAction = Mathf.FloorToInt(act[2]);
        var lateralAction = Mathf.FloorToInt(act[3]);

        switch (rotateAction) //THIS ROTATES THE CAMERA, NOT THE PLAYER
        {
            case 1:
                rotateDir = -Vector3.up;
                break;
            case 2:
                rotateDir = Vector3.up;
                break;
        }

        //ROTATE CAM
        cameraPivot.transform.position =
            Vector3.Lerp(cameraPivot.transform.position, agentRb.position, cameraFollowSpeed);
        cameraPivot.Rotate(180f * Time.deltaTime * rotateDir);

        var camForward = Vector3.Scale(cameraPivot.forward, new Vector3(1, 0, 1)).normalized;
        var camRight = Vector3.Scale(cameraPivot.right, new Vector3(1, 0, 1)).normalized;
        switch (forwardAction)
        {
            case 1:
                dirToGo = camForward * 1f;
                break;
            case 2:
                dirToGo = -camForward * 1f;
                break;
        }

        switch (lateralAction)
        {
            case 1:
                dirToGo += camRight * 1f;
                break;
            case 2:
                dirToGo += -camRight * 1f;
                break;
        }

        if (jumpAction == 1 && agentAnimator.m_IsGrounded)
        {
            if (agentAnimator.CanJump())
            {
                agentAnimator.Jump();
            }
        }

        if (!agentAnimator.m_IsGrounded)
        {
            dirToGo *= 0.8f;
        }

        dirToGo *= 6f;
        agentRb.velocity =
            Vector3.Lerp(agentRb.velocity, new Vector3(dirToGo.x, agentRb.velocity.y, dirToGo.z), .2f);
        agentAnimator.Move(dirToGo);
    }

    public override void Heuristic(float[] action)
    {
        action[0] = 0f;
        action[1] = 0f;
        action[2] = 0f;
        action[3] = 0f;
        if (Input.GetKey(KeyCode.S))
        {
            action[0] = 2f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            action[0] = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            action[3] = 2f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            action[3] = 1f;
        }
        if (Input.GetKey(KeyCode.K))
        {
            action[1] = 1f;
        }
        if (Input.GetKey(KeyCode.L))
        {
            action[1] = 2f;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            action[2] = 1f;
        }
    }

    private void CheckOutOfBounds()
    {
        if (transform.position.y < -3f)
        {
            if (uIController)
            {
                uIController.ShowKillScreen();
            }
            EndEpisode();
        }
    }

    private void CheckTimeout()
    {
        if (episodeTime <= 0)
        {
            if (uIController)
            {
                uIController.ShowKillScreen();
            }

            EndEpisode();
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        foreach (var col in _collisions)
        {
            if (col != null && col.collider != null && col.gameObject != null)
                if (ProcessCollision(col))
                {
                    break;
                }
        }

        _collisions.Clear();

        CheckOutOfBounds();
        CheckTimeout();

        MoveAgent(vectorAction);
        if (runTimer)
        {
            episodeTime -= 1;
        }

        uIController.floorText.text = floorBuilder.floorNumber.ToString();
        uIController.timeText.text = episodeTime.ToString();
    }
    
    public void ReparentAgent()
    {
        if (transform.parent != floorBuilder.transform)
        {
            transform.SetParent(floorBuilder.transform); //in case parented to something else
        }
    }

    public override void OnEpisodeBegin()
    {
        _collisions.Clear();

        if (!floorBuilder.hasInitialized)
        {
            floorBuilder.Initialize();
        }

        if (floorBuilder.floorNumber != 0)
        {
            Debug.Log("You reached floor: " + floorBuilder.floorNumber);
        }
        
        ReparentAgent();
        episodeTime = floorTimeStart;
        var perspective = floorBuilder.environmentParameters.agentPerspective;
        cameraAgent.GetComponent<CameraPerson>().UpdatePerspective(perspective);
        cameraPlayer.GetComponent<CameraPerson>().UpdatePerspective(perspective);
        floorBuilder.Reset();
        AgentNewFloor();
        uIController.seedText.text = floorBuilder.towerNumber.ToString();
    }

    public void ToggleTimer()
    {
        runTimer = !runTimer;
    }

    public int GetEpisodeTime()
    {
        return episodeTime;
    }
}
