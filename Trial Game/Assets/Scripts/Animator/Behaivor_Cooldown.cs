using System.Collections.Generic;
using UnityEngine;

public class Behaivor_Cooldown : StateMachineBehaviour
{
    private readonly int MAXCOLOR = 1;
    private readonly float MAXOPACITY = .9f;

    public GameObject Effect_Ready;
    [Tooltip("Minimum of 0 and Max of 1")] [Range(0, 1)] public float MinDarkness;

    private SpriteRenderer _sR;
    private UnitController _uC;
    private bool _startCooldown;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_startCooldown)
        {
            _uC = animator.GetComponent<UnitController>();
            _sR = animator.GetComponent<SpriteRenderer>();
            var tempColor = _sR.color;
            tempColor.r = tempColor.g = tempColor.b = MinDarkness;
            _sR.color = tempColor;
            _startCooldown = true;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float tempOpacity = (MAXCOLOR - MinDarkness) * (1 - (_uC.CooldownTimer / _uC.Cooldown));
        var tempColor = _sR.color;
        tempColor.r = tempColor.g = tempColor.b = Mathf.Min((MinDarkness + tempOpacity), MAXOPACITY);
        _sR.color = tempColor;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.GetBool("Cooldown"))
        {
            var tempColor = _sR.color;
            tempColor.r = tempColor.g = tempColor.b = MAXCOLOR;
            _sR.color = tempColor;
            Instantiate(Effect_Ready, _sR.transform.position, Quaternion.identity);
            _startCooldown = false;
        }
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
