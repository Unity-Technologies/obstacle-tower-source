using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for opening the door this object references once certain conditions are met.
/// </summary>
public class DoorLogic : MonoBehaviour
{
    public bool open;
    public Material closedDoor;
    public Material openDoor;
    public GameObject door;
    private Animation animator;
    private List<string> clips;

    private void Start()
    {
        open = false;
        animator = door.GetComponent<Animation>();
        clips = new List<string>(2);
        foreach (AnimationState clip in animator) {
            clips.Add(clip.name);
        }
    }

    public void TryOpenDoor(ObstacleTowerAgent agent)
    {
        if (!open)
        {
            if (door.CompareTag("lockedDoorRegular")
            || door.CompareTag("lockedDoorPuzzle") ||
            door.CompareTag("lockedDoorLever"))
            {
                OpenDoor(agent);
            }

            if (door.CompareTag("lockedDoorKey"))
            {
                if (agent.keyController)
                {
                    if (agent.keyController.currentNumberOfKeys > 0)
                    {
                        agent.keyController.UseKey();
                        OpenDoor(agent);
                    }
                }
            }
        }
    }

    public void TryCloseDoor(ObstacleTowerAgent agent)
    {
        if (open && door.CompareTag("lockedDoorLever"))
        {
            CloseDoor(agent);
        }
    }

    private void CloseDoor(ObstacleTowerAgent agent)
    {
        open = false;
        animator.Play(clips[1]);
        agent.AddReward(-0.1f);
    }

    private void OpenDoor(ObstacleTowerAgent agent)
    {
        animator.Play(clips[0]);
        open = true;
        agent.AddReward(0.1f);
    }
}
