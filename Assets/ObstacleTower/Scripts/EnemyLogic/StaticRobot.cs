using UnityEngine;

//A Static robot spins in place and shoots
//It does not look for or try to target the player
public class StaticRobot : Robot
{
    [Header("STATIC ROBOT")]
    public float shootDistance = 2; //distance projectiles will shoot in a transform.forward dir

    public override void UpdateRobot()
    {
        //GET ROTATION TARGET
        robotTargetRotation = Quaternion.Slerp(transform.rotation,
            transform.rotation * Quaternion.AngleAxis(idleBodyRotationSpeed, Vector3.up), lookAtTargetRotationDampen);

        //ROTATE
        RotateRobot();

        //HANDLE SHOOTING
        if (shootProjectilesController)
        {
            //SHOOT LOCAL FORWARD
            shootProjectilesController.Shoot(shootProjectilesController.projectileStartingPos.transform.position,
                transform.TransformPoint(Vector3.forward * shootDistance));
        }
    }
}
