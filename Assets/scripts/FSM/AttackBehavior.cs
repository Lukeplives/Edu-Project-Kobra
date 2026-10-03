using UnityEngine;

public class AttackBehavior : StateMachineBehaviour
{
    private FSMAIController controller;
    private Animator anim;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        anim = animator;
        controller = anim.GetComponentInParent<FSMAIController>();

        controller.AiController.StartAttack();

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller.AiController.UpdateAttack();

        if(!controller.CanAttackPlayer())
        {
            anim.SetTrigger("IsPursuing");
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        anim.ResetTrigger("IsPursuing");
    }
}
