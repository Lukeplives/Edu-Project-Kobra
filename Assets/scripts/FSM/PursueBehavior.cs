using UnityEngine;

public class PursueBehavior : StateMachineBehaviour
{
    private FSMAIController controller;
    private Animator anim;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        anim = animator;
        controller = anim.GetComponentInParent<FSMAIController>();

        controller.AiController.StartPursue();

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        controller.AiController.UpdatePursue();
        
        if (controller.CanAttackPlayer())
        {
            anim.SetTrigger("IsAttacking");
            return;
        }

        if (!controller.CanSeePlayer())
        {
            anim.SetTrigger("IsPatrolling");
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        anim.ResetTrigger("IsAttacking");
        anim.ResetTrigger("IsPatrolling");
        controller.AiController.StopMovement();
    }
}
