using UnityEngine;

//A Patrol Robot patrols back and forth between 2 waypoints
//If it "sees" the player (periodic raycast checks) it will
//...rotate towards the player and shoot
//When it can no longer "see" the player it will return to it's patrol
public class PatrolRobot : Robot
{
    //PATROL SETTINGS
    [Header("PATROL ROBOT")]
    public float
        moveToTargetDuration = 1f; //the amount of time it should take to move to target. Used by PatrolBot & ChaseBot

    public Transform patrolStartWaypoint;
    public Transform patrolEndWaypoint;
    private int currentPatrolDir = 1;

    //VISION SETTINGS (can the robot currenlty see the agent via raycast checks)
    [Header("RAYCAST VISION SETTINGS")]
    public float lookForPlayerEveryXSec = .5f; //how often should we look for the player?

    void OnEnable()
    {
        moveToTargetDuration = Random.Range(1f, 4f); //get a random speed
    }

    private void FindEndWaypoint()
    {
        if (transform.parent.parent.GetComponentInChildren<WaypointForRobot>())
        {
            patrolEndWaypoint = transform.parent.parent.GetComponentInChildren<WaypointForRobot>().transform;
        }
    }

    public override void UpdateRobot()
    {
        if (!patrolEndWaypoint)
        {
            FindEndWaypoint();
        }

        LookForPlayer(lookForPlayerEveryXSec);

        if (canSeePlayer)
        {
            //SET TARGET POS TO THE AGENT
            robotTargetPos = agentTransform.position;

            //HANDLE SHOOTING
            if (agentTransform && shootProjectilesController)
            {
                //SHOOT AT AGENT
                shootProjectilesController.Shoot(shootProjectilesController.projectileStartingPos.transform.position,
                    agentTransform.position + Vector3.up);
            }
        }
        else
        {
            //CHOOSE OUR TARGET POS
            robotTargetPos =
                currentPatrolDir == 1 ? patrolStartWaypoint.position : patrolEndWaypoint.position; //go to end

            //MOVE (patrolbot should only translate when it can't see the player)
            Move(moveToTargetDuration);
        }

        //BODY ROTATION
        Vector3 robotDirToTarget = robotTargetPos - transform.position;
        if (robotDirToTarget != Vector3.zero)
        {
            robotDirToTarget.y = 0; //don't rotate on the y;
            robotTargetRotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(robotDirToTarget.normalized), lookAtTargetRotationDampen);
            RotateRobot();
        }

        //SWITCH DIRECTIONS WHEN CLOSE ENOUGH
        if (robotDirToTarget.magnitude < .1f)
        {
            currentPatrolDir *= -1;
        }
    }
}
