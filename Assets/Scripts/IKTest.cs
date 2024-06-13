using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Progress: It works well enough but there needs to be a better handling of the up direction to match more
 * the normal of the plane created by the 3 points origin | target | effector target
*/

public class IKTest : MonoBehaviour
{
    public List<Transform> m_Bones;
    public List<Transform> m_Targets;
    private List<Quaternion> m_Pose;

    public float m_SqrDistError;
    public int m_MaxIterationCount;

    public enum Method
    {
        PrePosition,
        PrePositionWAdjust1,
        PrePositionWAdjust2,
        SingleAdjust,
        DebugPrint
    }

    public Method method;

    private void Start()
    {
        m_Pose = new List<Quaternion>();

        foreach (Transform trn in m_Bones)
        {
            m_Pose.Add(trn.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        SetPose();
        //Vector3 goal = m_target.GetTarget();
        Vector3 goal = m_Targets[0].position;
        //EE = EndEffector
        Vector3 currentEE = m_Bones[0].position;
        float distance = (currentEE - goal).magnitude;

        if (method == Method.PrePosition)
        {
            //If you don't input a specific pose each frame like in an animation this algorithm starts to rotate
            //the bones on themselves because of the circular motion in going from one vector to the other
            SetPose();
            
            for (int i = m_Bones.Count - 1; i > 0; i--)
            {
                RotateBone(m_Bones[i], m_Bones[i - 1].position, m_Targets[i - 1].position);
            }

            int iterations = 0;
            do
            {
                //Triangular Loop
                for (int i = 1; i < m_Bones.Count; i++)
                {
                    for (int j = i; j >= 1; j--)
                    {
                        RotateBone(m_Bones[j], currentEE, goal);
                        currentEE = m_Bones[0].position;
                        distance = (currentEE - goal).magnitude;

                        if (distance <= m_SqrDistError)
                            return;
                    }
                }
                iterations++;
            } while (distance > m_SqrDistError && iterations < m_MaxIterationCount);
            Debug.Log("Arrived at 10 iterations", this); 
        } 
        else if (method == Method.DebugPrint)
        {
            SetPose();
            float[] angles = { 0.0f, 0.0f, 0.0f, 0.0f };

            for (int i = 1; i < m_Bones.Count - 1; i++)
            {
                angles[i] = PrintNormals(m_Bones[i], m_Bones[i+1], m_Targets[0], m_Targets[i].position);
            }

            for (int i = 1; i < m_Bones.Count - 1; i++)
            {
                Vector3 boneToNext = (m_Bones[i + 1].position - m_Bones[i].position).normalized;
                m_Bones[i].rotation = Quaternion.AngleAxis(angles[i], boneToNext) * m_Bones[i].rotation;
            }

            for (int i = m_Bones.Count - 1; i > 0; i--)
            {
                RotateBone(m_Bones[i], m_Bones[i - 1].position, m_Targets[i - 1].position);
            }
        }
        else if (method == Method.PrePositionWAdjust1)
        {
            //currentEE = m_Bones[0].position;
            float[] angles = { 0.0f, 0.0f, 0.0f, 0.0f };

            for (int i = 1; i < m_Bones.Count - 1; i++)
            {
                angles[i] = PrintNormals(m_Bones[i], m_Bones[i + 1], m_Targets[0], m_Targets[i].position);
                Vector3 boneToNext = (m_Bones[i + 1].position - m_Bones[i].position).normalized;
                m_Bones[i].rotation = Quaternion.AngleAxis(angles[i], boneToNext) * m_Bones[i].rotation;
            }

            for (int i = m_Bones.Count - 1; i > 0; i--)
            {
                m_Bones[i].rotation = RotateBone(m_Bones[i], m_Bones[i - 1].position, m_Targets[i - 1].position) * m_Bones[i].rotation;
            }

            int iterations = 0;
            do
            {
                //Triangular Loop
                for (int i = 1; i < m_Bones.Count; i++)
                {
                    for (int j = i; j >= 1; j--)
                    {
                        m_Bones[j].rotation = RotateBone(m_Bones[j], currentEE, goal) * m_Bones[j].rotation;
                        currentEE = m_Bones[0].position;
                        distance = (currentEE - goal).magnitude;

                        if (distance <= m_SqrDistError)
                            return;
                    }
                }
                iterations++;
            } while (distance > m_SqrDistError && iterations < m_MaxIterationCount);
        }
        else if (method == Method.PrePositionWAdjust2)
        {
            float angles = 0.0f;

            for (int i = 1; i < m_Bones.Count - 1; i++)
            {
                angles = PrintNormals(m_Bones[i], m_Bones[i + 1], m_Targets[0], m_Targets[i].position);
                Vector3 boneToNext = (m_Bones[i + 1].position - m_Bones[i].position).normalized;
                m_Bones[i].rotation = Quaternion.AngleAxis(angles, boneToNext) * m_Bones[i].rotation;
            }

            for (int i = m_Bones.Count - 1; i > 0; i--)
            {
                m_Bones[i].rotation = RotateBone(m_Bones[i], m_Bones[i - 1].position, m_Targets[i - 1].position) * m_Bones[i].rotation;
            }

            int iterations = 0;
            do
            {
                //Triangular Loop
                for (int i = 1; i < m_Bones.Count; i++)
                {
                    for (int j = i; j >= 1; j--)
                    {
                        m_Bones[j].rotation = RotateBone(m_Bones[j], m_Bones[0].position, goal) * m_Bones[j].rotation;
                        //currentEE = m_Bones[0].position;
                        distance = (m_Bones[0].position - goal).magnitude;

                        if (distance <= m_SqrDistError)
                            return;
                    }
                }
                iterations++;
            } while (distance > m_SqrDistError && iterations < m_MaxIterationCount);
        }
    }

    private float PrintNormals(Transform bone, Transform prevBone, Transform nextBone, Vector3 goal)
    {
        Vector3 boneToNext = nextBone.position - bone.position;
        Vector3 boneToPrev = prevBone.position - bone.position;
        Vector3 goalToNext = nextBone.position - goal;
        Vector3 goalToPrev = prevBone.position - goal;

        Vector3 n1 = Vector3.Cross(boneToNext, boneToPrev).normalized;
        Vector3 n2 = Vector3.Cross(goalToNext, goalToPrev).normalized;

        //Debug.DrawLine(bone.position, bone.position + boneToNext, Color.magenta, Time.deltaTime, false);
        //Debug.DrawLine(bone.position, bone.position + boneToPrev, Color.magenta, Time.deltaTime, false);
        //Debug.DrawLine(goal, goal + goalToNext, Color.black, Time.deltaTime, false);
        //Debug.DrawLine(goal, goal + goalToPrev, Color.black, Time.deltaTime, false);

        Debug.DrawLine(bone.position, bone.position + (n1 * 0.3f), Color.yellow, Time.deltaTime, false);
        Debug.DrawLine(bone.position, bone.position + (n2 * 0.2f), Color.yellow, Time.deltaTime, false);
        Debug.DrawLine(bone.position, bone.position + Vector3.Cross(n1, n2), Color.red, Time.deltaTime, false);

        Vector3 axis = prevBone.position - nextBone.position;
        Debug.DrawLine(bone.position, bone.position + axis, Color.black, Time.deltaTime, false);

        // Debug.Log("Cross: " + VExtension.Print(Vector3.Cross(n1, n2)) + " Angle Between: " + Vector3.SignedAngle(n1, n2, axis));

        return Vector3.SignedAngle(n1, n2, axis);
    }

    private Quaternion RotateBone(Transform bone, Vector3 effector, Vector3 eeGoal)
    {
        Vector3 bonePosition = bone.position;
        Quaternion boneRotation = bone.rotation;

        Vector3 boneToEffector = effector - bonePosition;
        Vector3 boneToEEGoal = eeGoal - bonePosition;

        Quaternion fromToRotation = Quaternion.FromToRotation(boneToEffector, boneToEEGoal);
        //bone.rotation = fromToRotation * boneRotation;
        return fromToRotation;
    }

    private void SetPose()
    {
        for (int i = 0; i < m_Bones.Count; i++)
        {
            m_Bones[i].rotation = m_Pose[i];
        }
    }
}
