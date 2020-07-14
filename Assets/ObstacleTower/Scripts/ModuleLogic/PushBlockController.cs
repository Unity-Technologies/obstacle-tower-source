using Unity.MLAgents;
using UnityEngine;

/// <summary>
/// Responsible for determining whether block puzzle has been solved.
/// </summary>
public class PushBlockController : MonoBehaviour
{
    public Component[] doorControllers; //all locked doors in this room
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    public Material activeMaterialBlock;
    public Material activeMaterialTrigger;

    void Start()
    {
        doorControllers = transform.parent.parent.GetComponentsInChildren(typeof(DoorLogic), true);
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// Resets the block to the initial position and rotation.
    /// </summary>
    public void ResetPosition()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    void OnTriggerEnter(Collider col)
    {
        
        if (col.transform.CompareTag("pushBlockTrigger"))
        {
            GetComponent<Renderer>().material = activeMaterialBlock;
            col.gameObject.GetComponent<Renderer>().material = activeMaterialTrigger;
            
            foreach (var component in doorControllers)
            {
                var door = (DoorLogic) component;
                var agent = transform.root.GetComponent<FloorBuilder>().agent.GetComponent<ObstacleTowerAgent>();
                door.TryOpenDoor(agent);
            }
        }
    }
}
