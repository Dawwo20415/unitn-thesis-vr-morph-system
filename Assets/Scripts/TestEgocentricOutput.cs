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

    private EgocentricProjectionDebug source_debug;
    private EgocentricProjectionDebug destin_debug;

    private List<IKChain2> m_chains;
    private IKChain chain;
    private NativeArray<Vector3> m_TargetsArray;

    private BSAComponent m_SourceBSA;
    private BSAComponent m_DestBSA;

    public void InstanceTargets()
    {
        source_debug = new EgocentricProjectionDebug(m_SourceBSA.BSAD.coordinateSpan);
        destin_debug = new EgocentricProjectionDebug(m_DestBSA.BSAD.coordinateSpan);

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
        List<BSACoordinates> coords = m_SourceBSA.Project(hbb, ref source_debug);
        return m_DestBSA.ReverseProject(hbb, coords, ref destin_debug);
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

        source_debug.OnGizmoDraw(true, true);
        destin_debug.OnGizmoDraw(true, true);
    }
}
