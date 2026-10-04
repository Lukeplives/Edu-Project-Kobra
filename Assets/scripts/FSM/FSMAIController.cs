using UnityEngine;

public class FSMAIController : MonoBehaviour
{
    public Animator anim;
    public AIController AiController;

    [Header("Detection")]

    [SerializeField] private float attackDistance = 7f;
    [SerializeField] private float detectionRadius = 15f;

    [SerializeField] private float visionCheckInterval = 0.5f;
    [SerializeField] private LayerMask visionLayer;

    private float nextVisionCheck;
    private bool playerVisible;

    public bool CanSeePlayer()
    {
        if (AiController.Target == null)
            return false;

        Vector3 direction = AiController.Target.position - transform.position;
        float distance = direction.magnitude;

        if(distance > detectionRadius)
        {
            playerVisible = false;
            return false;
        }

        if(Time.time >= nextVisionCheck)
        {
            nextVisionCheck = Time.time + visionCheckInterval;
            playerVisible = CheckLineOfSight(direction,distance);
        }

        return playerVisible;
    }

    public bool CanAttackPlayer()
    {
        if (AiController.Target == null)
            return false;

        return CanHitTarget();
    }
        public bool CanHitTarget()
    {
        if (AiController.Target == null)
            return false;

        return AiController.CalculateAngle(AiController.Target.position, true) != null;
    }

    private bool CheckLineOfSight(Vector3 direction, float distance)
    {
        direction.Normalize();

        if (Physics.Raycast(
            transform.position,
            direction,
            out RaycastHit hit,
            distance,
            visionLayer))
        {
            return hit.transform == AiController.Target ||
                hit.transform.IsChildOf(AiController.Target);
        }

        return false;
    }
}
