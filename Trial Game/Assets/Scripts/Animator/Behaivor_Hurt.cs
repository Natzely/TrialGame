using System;
using System.Collections.Generic;
using UnityEngine;

public class Behaivor_Hurt : StateMachineBehaviour
{
    public List<AudioClip> HitSounds;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var uC = animator.GetComponent<UnitController>();
        var hurtSoundIndex = (int)(Time.time % HitSounds.Count);
        var hurtClip = HitSounds[hurtSoundIndex];
        if (uC)
        {
            uC.HurtAudioSource.Play(hurtClip);
            uC.EnterHurtState();
        }

        var deathID = Animator.StringToHash("Death");
        if (animator.HasState(0, deathID) && uC.CurrentHealth <= 0)
            animator.SetBool("Death", true);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Console.WriteLine("");
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var uC = animator.GetComponent<UnitController>();
        if(uC && uC.CurrentHealth > 0)
            uC.ExitHurtState();
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
}
