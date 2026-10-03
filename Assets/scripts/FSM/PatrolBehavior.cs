using UnityEngine;
using UnityEngine.AI;

public class PatrolBehavior : StateMachineBehaviour
{
    private FSMAIController controller;
    private Animator anim;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        anim = animator;
        controller = anim.GetComponentInParent<FSMAIController>();
        

        controller.AiController.StartPatrol();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(controller.AiController.HasReachedPatrolPoint())
        {
            controller.AiController.NextPatrolPoint();
        }
        if(controller.CanSeePlayer())
        {
            anim.SetTrigger("IsPursuing");
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        anim.ResetTrigger("IsPursuing");
        controller.AiController.StopMovement();

    }

}
