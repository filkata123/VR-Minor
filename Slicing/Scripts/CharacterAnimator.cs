using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAnimator : MonoBehaviour
{
    public Transform playerTransform;
    public float attackSpeed = 3f;

    NavMeshAgent navMeshAgent;
    protected Animator animator;

    private float distance = 2f;
    private bool playerInCombatArea = false;

    const float locomotionAnimationSmoothTime = .1f;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        animator.SetLayerWeight(1, 1);

        StartCoroutine(WaitForAttack());
    }


    IEnumerator WaitForAttack()
    {
        if(playerInCombatArea)
        {
            animator.SetTrigger("attack");
        }
        yield return new WaitForSeconds(attackSpeed);
        StartCoroutine(WaitForAttack());

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        float speedPercent = navMeshAgent.velocity.magnitude / navMeshAgent.speed;
        animator.SetFloat("Speed", speedPercent, locomotionAnimationSmoothTime, Time.deltaTime);

        playerInCombatArea = (Vector3.Distance(playerTransform.position, this.transform.position) < distance);
        animator.SetBool("inCombat", playerInCombatArea);
    }
}
