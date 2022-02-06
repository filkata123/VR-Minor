using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeCollision : MonoBehaviour
{
    private Animator anim;
    private GameObject rig;
    private GameObject teleportLoc;
    // Start is called before the first frame update
    void Start()
    {
        anim = this.gameObject.transform.root.GetComponent<Animator>();
        rig = GameObject.Find("PlayerRig");
        teleportLoc = GameObject.Find("StartPosition");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 7)
        {
            if(anim.GetCurrentAnimatorStateInfo(0).IsName("Armature|Axe_attack"))
            {
                if(InvincibilityButton.invincibility == false)
                {
                    if(rig)
                    {
                        rig.transform.position = teleportLoc.transform.position;
                    }
                    //Debug.Log("Hit");
                }
            }
        }
    }
}
