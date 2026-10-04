using UnityEngine;

public class SearchBehavior : StateMachineBehaviour
{
    private FSMAIController controller;
    private Animator anim;

    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        anim = animator;
        controller = animator.GetComponentInParent<FSMAIController>();

        controller.AiController.StartSearch();
    }

    public override void OnStateUpdate(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        if (controller.CanSeePlayer())
        {
            anim.SetTrigger("IsPursuing");
            return;
        }

        if (controller.AiController.HasFinishedSearch())
        {
            anim.SetTrigger("IsPatrolling");
        }
    }

    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        anim.ResetTrigger("IsPursuing");
        anim.ResetTrigger("IsPatrolling");
    }
}