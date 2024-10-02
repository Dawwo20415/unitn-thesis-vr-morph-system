using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class CCDIKComponent : MonoBehaviour
{
    private List<Transform> m_Bones;

    private float m_SqrDistError = 0.01f;
    private int m_MaxIterationCount = 10;
    private int m_ChainLength;

    public void Init(Animator animator, List<HumanBodyBones> bones)
    {
        m_Bones = new List<Transform>(bones.Count);

        for (int i = 0; i < bones.Count; i++)
        {
            m_Bones[i] = animator.GetBoneTransform(bones[i]);
        }

        m_ChainLength = bones.Count;
    }

    public void Init(Animator animator, List<HumanBodyBones> bones, int max_iterations, float square_distance_error)
    {
        Init(animator, bones);
        m_SqrDistError = square_distance_error;
        m_MaxIterationCount = max_iterations;
    }

    public void IKSolver(List<Vector3> targets)
    {
        if (targets.Count != m_Bones.Count)
            throw new UnityException("From IK Solver: provided targets are not the same length as the bones");

        Vector3 goal = targets[0];

        ROTATE_CROSS(targets);
        PRE_2TARGETS(targets);
        CCD_IK(goal);
    }

    private void ROTATE_CROSS(List<Vector3> targets)
    {
        Quaternion rot;
        float angle = 0.0f;
        for (int i = 1; i < m_ChainLength - 1; i++)
        {
            angle = BetweenNormals(m_Bones[i].position, m_Bones[i + 1].position, targets[0], targets[i]);
            Vector3 boneToNext = (m_Bones[i + 1].position - m_Bones[i].position).normalized;
            rot = Quaternion.AngleAxis(angle, boneToNext) * m_Bones[i].rotation;
            m_Bones[i].rotation = rot;
        }
    }

    private void PRE_2TARGETS(List<Vector3> targets)
    {
        Quaternion rot;
        for (int i = m_ChainLength - 1; i > 0; i--)
        {
            rot = RotateBone(m_Bones[i].position, m_Bones[i - 1].position, targets[i - 1]) * m_Bones[i].rotation;
            m_Bones[i].rotation = rot;
        }
    }

    private void CCD_IK(Vector3 goal)
    {
        float distance = (GetEffector() - goal).magnitude;
        int iterations = 0;
        do
        {
            //Reverse Triangular Loop [1-21-321-4321-Loop]
            for (int i = 1; i < m_ChainLength; i++)
            {
                for (int j = i; j >= 1; j--)
                {
                    Quaternion rot = RotateBone(m_Bones[j].position, GetEffector(), goal) * m_Bones[j].rotation;
                    m_Bones[j].rotation = rot;
                    distance = (GetEffector() - goal).magnitude;

                    if (distance <= m_SqrDistError)
                        return;
                }
            }
            iterations++;
        } while (distance > m_SqrDistError && iterations < m_MaxIterationCount);
    }

    private Vector3 GetEffector()
    {
        return m_Bones[0].position;
    }
    private float BetweenNormals(Vector3 bone, Vector3 prevBone, Vector3 nextBone, Vector3 goal)
    {
        Vector3 boneToNext = nextBone - bone;
        Vector3 boneToPrev = prevBone - bone;
        Vector3 goalToNext = nextBone - goal;
        Vector3 goalToPrev = prevBone - goal;

        Vector3 n1 = Vector3.Cross(boneToNext, boneToPrev).normalized;
        Vector3 n2 = Vector3.Cross(goalToNext, goalToPrev).normalized;

        Vector3 axis = prevBone - nextBone;

        return Vector3.SignedAngle(n1, n2, axis);
    }
    private Quaternion RotateBone(Vector3 bonePosition, Vector3 effector, Vector3 goal)
    {
        Vector3 boneToEffector = effector - bonePosition;
        Vector3 boneToEEGoal = goal - bonePosition;

        return Quaternion.FromToRotation(boneToEffector, boneToEEGoal);
    }
}
