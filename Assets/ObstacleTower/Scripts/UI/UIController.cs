using UnityEngine;
using TMPro;

/// <summary>
/// Responsible for rendering and managing contents of onscreen UI.
/// </summary>
public class UIController : MonoBehaviour
{
    FloorBuilder floorBuilder;
    private ObstacleTowerAcademy academy;
    public GameObject killScreenPanel;
    public GameObject devUIPanel;
    public TMP_Text floorText;
    public TMP_Text timeText;
    public TMP_Text seedText;
    private const float KillScreenDuration = 1;
    private bool killScreenOn = false;
    private float killScreenElapsed;

    // Use this for initialization
    void Awake()
    {
        floorBuilder = FindObjectOfType<FloorBuilder>();
        academy = FindObjectOfType<ObstacleTowerAcademy>();
        killScreenPanel.SetActive(false);
    }

    void FixedUpdate()
    {
        if (killScreenOn)
        {
            killScreenElapsed += Time.fixedDeltaTime;
            if (killScreenElapsed >= KillScreenDuration)
            {
                killScreenElapsed = 0;
                killScreenOn = false;
                killScreenPanel.SetActive(false);
            }
        }
    }

    public void ShowKillScreen()
    {
        if (!killScreenPanel.activeInHierarchy)
        {
            killScreenPanel.SetActive(true);
            killScreenOn = true;
        }
    }

    public void GoToNextFloor()
    {
        floorBuilder.IncrementFloorNumber();
        floorBuilder.agent.GetComponent<ObstacleTowerAgent>().AgentNewFloor();
    }

    public void GoToPrevFloor()
    {
        if (floorBuilder.floorNumber == 0)
        {
            floorBuilder.floorNumber = floorBuilder.totalFloors - 1;
        }
        else
        {
            floorBuilder.floorNumber -= 1;
        }
        floorBuilder.agent.GetComponent<ObstacleTowerAgent>().AgentNewFloor();
    }

    public void StartOver()
    {
        floorBuilder.floorNumber = 0;
        var agent = floorBuilder.agent.GetComponent<ObstacleTowerAgent>();
        agent.Done();
    }

    public void ResetAnimation()
    {
        floorBuilder.agent.GetComponent<AgentAnimator>().ResetAnimator();
    }

    public void ToggleTimer()
    {
        floorBuilder.agent.GetComponent<ObstacleTowerAgent>().ToggleTimer();
    }
}
