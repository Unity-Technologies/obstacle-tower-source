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
    KeyController keyController;
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

    public void TryOpenDoor()
    {
        if (!open)
        {
            if (door.CompareTag("lockedDoorRegular")
            || door.CompareTag("lockedDoorPuzzle") ||
            door.CompareTag("lockedDoorLever"))
            {
                OpenDoor();
            }

            if (door.CompareTag("lockedDoorKey"))
            {
                keyController = FindObjectOfType<KeyController>();
                if (keyController)
                {
                    if (keyController.currentNumberOfKeys > 0)
                    {
                        keyController.UseKey();
                        OpenDoor();
                    }
                }
            }
        }
    }

    public void TryCloseDoor()
    {
        if (open && door.CompareTag("lockedDoorLever"))
        {
            CloseDoor();
        }
    }

    private void CloseDoor()
    {
        open = false;
        animator.Play(clips[1]);
        GameObject.FindWithTag("agent").GetComponent<ObstacleTowerAgent>().AddReward(-0.1f);
    }

    private void OpenDoor()
    {
        animator.Play(clips[0]);
        open = true;
        GameObject.FindWithTag("agent").GetComponent<ObstacleTowerAgent>().AddReward(0.1f);
    }
}
