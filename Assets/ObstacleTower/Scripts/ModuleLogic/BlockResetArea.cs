using UnityEngine;

/// <summary>
/// Responsible for resetting the puzzle block to its initial position.
/// </summary>
public class BlockResetArea : MonoBehaviour
{

	public PushBlockController controller;
	public bool needsBlock;
	public bool hasBlock;


	private void Start()
	{
		needsBlock = true;
		hasBlock = false;
	}
	
	private void FixedUpdate()
	{
		if (needsBlock && !hasBlock)
		{
			FindBlock();
		}        
	}

	private void FindBlock()
	{
		controller =
			transform.parent.parent.GetComponentInChildren<PushBlockController>();
		if (controller != null)
		{
			hasBlock = true;
			needsBlock = false;
		}
	}
	

	/// <summary>
	/// Resets the position of the block when the agent enters the button trigger.
	/// </summary>
	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.CompareTag("agent") && hasBlock)
		{
			controller.ResetPosition();
		}
	}
}
