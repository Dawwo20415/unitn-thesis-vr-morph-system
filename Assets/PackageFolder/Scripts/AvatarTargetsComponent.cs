using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarTargetsComponent : MonoBehaviour
{
    private struct DispCorrespondance
    {
        public int bone;
        public IDisplacementOperation op;

        public DispCorrespondance(int b, IDisplacementOperation o)
        {
            bone = b;
            op = o;
        }
    }

    private List<Vector3> m_targets;
    private List<DispCorrespondance> m_standard_indexes;
    private List<DispCorrespondance> m_egocentric_indexes;

    public void InstanceTargets()
    {
        m_standard_indexes = new List<DispCorrespondance>();
        m_egocentric_indexes = new List<DispCorrespondance>();
        m_targets = new List<Vector3>((int)HumanBodyBones.LastBone);
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            m_targets.Add(Vector3.zero);
        }
    }

    public void SetTargets(Animator animator)
    {
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            if (animator.GetBoneTransform((HumanBodyBones)i))
            {
                Transform trn = animator.GetBoneTransform((HumanBodyBones)i);
                m_targets[i] = trn.position;
            }
        }
    }

    public void RegisterBone(int bone, IDisplacementOperation op)
    {
        m_standard_indexes.Add(new DispCorrespondance(bone, op));
    }

    public void RegisterEgocentricBone(int bone, IDisplacementOperation op)
    {
        m_egocentric_indexes.Add(new DispCorrespondance(bone, op));
    }

    public void CompoundOperations()
    {
        foreach (DispCorrespondance i in m_standard_indexes)
        {
            m_targets[i.bone] = i.op.Operation(m_targets[i.bone]);
        }

        foreach (DispCorrespondance i in m_egocentric_indexes)
        {
            m_targets[i.bone] = i.op.Operation(m_targets[i.bone]);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        foreach (DispCorrespondance i in m_standard_indexes)
        {
            Gizmos.DrawWireSphere(m_targets[i.bone], 0.025f);
        }

        Gizmos.color = Color.red;
        foreach (DispCorrespondance i in m_egocentric_indexes)
        {
            Gizmos.DrawWireSphere(m_targets[i.bone], 0.025f);
        }
    }

}
