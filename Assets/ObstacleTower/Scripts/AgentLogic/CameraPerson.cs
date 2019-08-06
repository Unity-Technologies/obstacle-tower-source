using UnityEngine;

/// <summary>
/// Used to enable switching between first and third person perspective programatically.
/// </summary>
public class CameraPerson : MonoBehaviour
{
    public AgentPerspective perspective;
    private Camera attachedCamera;
    private const int NoAgentMask = 32567;

    [Header("First Person")]
    public Vector3 firstPersonPosition;
    public Vector3 firstPersonRotation;
    
    [Header("Third Person")]
    public Vector3 thirdPersonPosition;
    public Vector3 thirdPersonRotation;

    private void Start()
    {
        attachedCamera = GetComponent<Camera>();
        UpdatePerspective(perspective);
    }

    public void UpdatePerspective(AgentPerspective firstPerson)
    {
        perspective = firstPerson;
        
        if (perspective == AgentPerspective.FirstPerson)
        {
            transform.localPosition = firstPersonPosition;
            transform.localRotation = Quaternion.Euler(firstPersonRotation);
            attachedCamera.cullingMask = NoAgentMask;
        }
        else
        {
            transform.localPosition = thirdPersonPosition;
            transform.localRotation = Quaternion.Euler(thirdPersonRotation);
            attachedCamera.cullingMask = -1;
        }

    }
}
