using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behaviour_Moving : StateMachineBehaviour
{
    public AudioClip WalkingStone;
    public AudioClip WalkingGrass;

    Enums.GridBlockType _gbT;
    AudioSource _aS;
    UnitController _uC;
    GridBlock _gB;
    

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _uC = animator.GetComponent<UnitController>();
        _gB = _uC.CurrentGridBlock;
        _aS = _uC.WalkingAudioSource;
        _gbT = _gB.Type;
        PlayProperClip();

        _uC.UnitGlance?.UnitMoving(true);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_uC.CurrentGridBlock.Type != _gbT)
        {
            _gbT = _uC.CurrentGridBlock.Type;
            _aS.Stop();
            PlayProperClip();
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _aS.Stop();
        _aS = null;
        _uC.UnitGlance?.UnitMoving(false);
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
        if (_gbT == Enums.GridBlockType.Stone)
            _aS.clip = WalkingStone;
        else
            _aS.clip = WalkingGrass;
    }
}
