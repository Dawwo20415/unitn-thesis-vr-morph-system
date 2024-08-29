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


    public void RegisterChain(IKChain2 chain)
    {
        m_chains.Add(chain);
    }
}
