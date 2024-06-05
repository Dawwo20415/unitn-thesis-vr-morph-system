using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AddDirectionsToSkeletonTransform : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            GameObject obj = animator.GetBoneTransform((HumanBodyBones)i).gameObject;
            if (obj)
            {
                DebugDisplayDirections ddd = obj.AddComponent<DebugDisplayDirections>();
                ddd.length = 0.1f;
            }
        }
        //Debug.Log("Nome del root del avatar [" + animator.avatar.name + "] è  [" + animator.avatarRoot.name + "]");
        //DebugDisplayDirections ddd2 = animator.avatarRoot.gameObject.AddComponent<DebugDisplayDirections>();
        //ddd2.length = 0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
