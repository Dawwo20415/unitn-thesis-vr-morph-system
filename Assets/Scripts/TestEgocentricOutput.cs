using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public struct IKChain2
{
    public List<HumanBodyBones> bones;
    public List<Vector3> targets;
}

public class TestEgocentricOutput : MonoBehaviour
{
    public NativeArray<Vector3> targets { get => m_TargetsArray; }

    private List<Vector3> m_DebugSourceLines;
    private List<Vector3> m_DebugSourceOnMesh;
    private List<Vector3> m_DebugDestLines;

    private List<IKChain2> m_chains;
    private IKChain chain;
    private NativeArray<Vector3> m_TargetsArray;

    private BSAComponent m_SourceBSA;
    private BSAComponent m_DestBSA;

    public void InstanceTargets()
    {
        m_DebugDestLines = new List<Vector3>();
        m_DebugSourceLines = new List<Vector3>();
        m_DebugSourceOnMesh = new List<Vector3>();

        m_TargetsArray = new NativeArray<Vector3>((int)HumanBodyBones.LastBone, Allocator.Persistent);
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            m_TargetsArray[i] = Vector3.zero;
        } 
    }

    public void SetBSAComponents(BSAComponent source, BSAComponent dest)
    {
        m_SourceBSA = source;
        m_DestBSA = dest;
    }

    public Vector3 Calculate(HumanBodyBones hbb)
    {
        m_DebugSourceLines.Clear();
        List<BSACoordinates> coords = m_SourceBSA.Project(hbb, ref m_DebugSourceLines, ref m_DebugSourceOnMesh);
        return m_DestBSA.ReverseProject(hbb, coords);
    }

    public void SetTarget(HumanBodyBones hbb, Vector3 position)
    {
        //Debug.Log("Setting Target for bone " + hbb.ToString() + " to " + position);
        m_TargetsArray[(int)hbb] = position;
    }

    public Vector3 GetTarget(HumanBodyBones hbb)
    {
        return m_TargetsArray[(int)hbb];
    }

    public void RegisterChain(IKChain2 chain)
    {
        m_chains.Add(chain);
    }

    private void OnDestroy()
    {
        m_TargetsArray.Dispose();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        foreach (Vector3 pos in m_TargetsArray)
        {
            Gizmos.DrawWireSphere(pos, 0.05f);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLineList(m_DebugSourceLines.ToArray());

        Gizmos.color = Color.black;
        Gizmos.DrawLineList(m_DebugSourceOnMesh.ToArray());
    }
}
