using UnityEngine;

/// <summary>
/// Responsible for physically moving platforms around according to various parameters.
/// </summary>
public class PlatformMover : MonoBehaviour
{
    public bool needsTarget;
    public float meanSpeed = 1f;
    
    [Header("Object Translation")]
    public bool translateObject = true;
    public Vector3 startPosition;
    public Vector3 endPosition;
    private bool goingForward;
    private float speed;
    
    private GameObject floor;
    private GameObject agent;
    private bool hasTarget;

    [Header("Object Rotation")]
    public bool rotateObject;
	public Vector3 rotationSpeed;
	public Space objectRotationSpace; //should we rotate in world or local space?


    private void Start()
    {
        goingForward = true;
        speed = Random.Range(meanSpeed * 0.75f, meanSpeed *1.25f);

        agent = FindObjectOfType<ObstacleTowerAgent>().gameObject;
        floor = agent.transform.parent.gameObject;

        if(translateObject)
        {
            transform.localPosition = Vector3.Lerp(
                startPosition, endPosition, Random.Range(0f, 1f));
        }
    }

    private void FindTarget()
    {
        var globalEndPosition =
            transform.parent.parent.GetComponentInChildren<PlatformTarget>().gameObject.transform.position;
        endPosition = (transform.InverseTransformPoint(globalEndPosition) * transform.localScale.x) + startPosition;
        hasTarget = true;
        transform.localPosition = Vector3.Lerp(
            startPosition, endPosition, Random.Range(0f, 1f));
    }

    private void FixedUpdate()
    {
        if (needsTarget && !hasTarget)
        {
            FindTarget();
        }
        
        if(translateObject)
        {
            MovePlatform();
        }
        
        if(rotateObject)
        {
		    transform.Rotate(rotationSpeed * Time.deltaTime, objectRotationSpace);
        }
    }

    private void MovePlatform()
    {
        var directionPosition = goingForward ? endPosition : startPosition;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition, directionPosition, speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.localPosition, directionPosition) < 0.1f)
        {
            goingForward = !goingForward;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("agent") && other.contacts[0].normal.y < -.5f)
        {
            agent.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.transform.CompareTag("agent"))
        {
            agent.GetComponent<ObstacleTowerAgent>().ReparentAgent();
        }
    }

    private void OnDestroy()
    {
        agent.GetComponent<ObstacleTowerAgent>().ReparentAgent();
    }
}
