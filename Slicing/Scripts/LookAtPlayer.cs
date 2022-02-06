using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LookAtPlayer : MonoBehaviour
{
    protected Animator animator;

    private float lookWeight = 1;
    private float headWeight = 1;
    private float bodyWeight = 0;
    private float eyesWeight = 0;

    public float clampWeight = 1;
    public Transform lookAt;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK()
    {

        // Set the look target position, if one has been assigned
        if (lookAt != null)
        {
            animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
            animator.SetLookAtPosition(lookAt.position);
        }


    }
}
