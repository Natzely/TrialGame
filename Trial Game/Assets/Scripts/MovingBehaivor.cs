using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBehaivor : StateMachineBehaviour
{
    public AudioClip WalkingStone;
    public AudioClip WalkingGrass;

    Enums.GridBlockType gbT;
    AudioSource aS;
    UnitController uC;
    GridBlock gB;
    

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        uC = animator.GetComponent<UnitController>();
        gB = uC.CurrentGridBlock;
        aS = uC.WalkingAudioSource;
        gbT = gB.Type;
        PlayProperClip();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(uC.CurrentGridBlock.Type != gbT)
        {
            gbT = uC.CurrentGridBlock.Type;
            aS.Stop();
            PlayProperClip();
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        aS.Stop();
        aS = null;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}

    private void PlayProperClip()
    {
        if (gbT == Enums.GridBlockType.Stone)
            aS.clip = WalkingStone;
        else
            aS.clip = WalkingGrass;
        //aS.Play();
    }
}
