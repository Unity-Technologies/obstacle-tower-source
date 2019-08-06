using UnityEngine;

//A Chasing Robot rotates in place until it sees the player
//If it "sees" the player (periodic ray-cast checks) it will
//...rotate & move towards the player while shooting
//When it can no longer "see" the player it will return to it's starting position
public class ChasingRobot : Robot
{
    //CHASING SETTINGS
    [Header("CHASING ROBOT")]
    public float
        moveToTargetDuration = 1f; //the amount of time it should take to move to target. Used by PatrolBot & ChaseBot

    public AnimationCurve animCurveChaseBotHoverEffect; //hoverOscillation
    private float robotHoverEffectCurveTimer;

    //VISION SETTINGS (can the robot currently see the agent via raycast checks)
    [Header("RAYCAST VISION SETTINGS")]
    public float lookForPlayerEveryXSec = .5f; //how often should we look for the player?


    public override void UpdateRobot()
    {
        LookForPlayer(lookForPlayerEveryXSec);

        if (canSeePlayer)
        {
            //SET TARGET POS TO THE AGENT
            robotTargetPos = agentTransform.TransformPoint(Vector3.forward * 2.5f);

            dirToAgent.y = 0; //don't rotate on the y

            //SET TARGET ROT (ROTATE TOWARDS AGENT)
            robotTargetRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirToAgent),
                lookAtTargetRotationDampen);

            //HANDLE SHOOTING
            if (agentTransform && shootProjectilesController)
            {
                //SHOOT AT AGENT
                shootProjectilesController.Shoot(shootProjectilesController.projectileStartingPos.transform.position,
                    agentTransform.position + Vector3.up);
            }
        }
        else //can't see player
        {
            //SET TARGET POS TO CURRENT LOCATION
            robotTargetPos = startingPos;

            //SET TARGET ROT (ROTATE IN PLACE)
            robotTargetRotation = Quaternion.Slerp(transform.rotation,
                transform.rotation * Quaternion.AngleAxis(idleBodyRotationSpeed, Vector3.up),
                lookAtTargetRotationDampen);
        }

        //ADD BOUNCE EFFECT TO THE TARGET POSITION
        Vector3 hoverEffectVector =
            new Vector3(0, animCurveChaseBotHoverEffect.Evaluate(robotHoverEffectCurveTimer), 0);
        robotTargetPos += hoverEffectVector;

        //MOVE
        Move(moveToTargetDuration);

        //BODY ROTATION
        RotateRobot();

        //UPDATE TIMER
        robotHoverEffectCurveTimer += Time.fixedDeltaTime;
    }
}