using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimationEvent : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit(animator, stateInfo, layerIndex);
        BoxCollider2D collider = animator.gameObject.GetComponent<BoxCollider2D>();
        if (animator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Open")){
                collider.enabled = false;
        } 
        else if (animator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Close")) {
                collider.enabled = true;
        }
    }
}
