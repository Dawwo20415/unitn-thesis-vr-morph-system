using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IKChain2
{
    public List<HumanBodyBones> bones;
    public List<Vector3> targets;
}

public class TestEgocentricOutput : MonoBehaviour
{
    private List<IKChain2> m_chains;
    private IKChain chain;

    private BSAComponent m_SourceBSA;
    private BSAComponent m_DestBSA;

    public void SetBSAComponents(BSAComponent source, BSAComponent dest)
    {
        m_SourceBSA = source;
        m_DestBSA = dest;
    }

    public Vector3 Calculate(HumanBodyBones hbb)
    {
        List<BSACoordinates> coords = m_SourceBSA.Project(hbb);
        return m_DestBSA.ReverseProject(hbb, coords);
    }

    public void SetTarget(HumanBodyBones hbb, Vector3 position)
    {
        //Debug.Log("Setting Target for bone " + hbb.ToString() + " to " + position);
    }

    public void RegisterChain(IKChain2 chain)
    {
        m_chains.Add(chain);
    }
}
