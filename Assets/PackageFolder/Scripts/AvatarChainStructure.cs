using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarChainStructure
{
    public List<HumanBodyBones> chain { get => m_Chain; }
    public List<IDisplacementOperation> ops { get => m_Ops; }
    public List<bool> egocentric { get => m_EgocentricBones; }
    public List<int> chain_indexes { get => GetIndexChain(); }

    private List<HumanBodyBones> m_Chain;
    private List<IDisplacementOperation> m_Ops;
    private List<bool> m_EgocentricBones;

    public AvatarChainStructure(List<HumanBodyBones> chain, List<IDisplacementOperation> ops, List<bool> ego)
    {
        if ((chain.Count != ops.Count) || (chain.Count != ego.Count)) { throw new UnityException("In creation of AvatarChain bone number and operations numbers do not match"); }

        m_Chain = chain;
        m_Ops = ops;
        m_EgocentricBones = ego;
    }

    private List<int> GetIndexChain()
    {
        List<int> value = new List<int>(m_Chain.Count);

        for (int i = 0; i < m_Chain.Count; i++)
        {
            value.Add((int)(m_Chain[i]));
        }

        return value;
    }
}

public class AvatarChainsHandler
{
    private List<AvatarChainStructure> m_Chains;

    public AvatarChainsHandler()
    {
        m_Chains = new List<AvatarChainStructure>();
    }

    public void AddChain(List<HumanBodyBones> chain, List<IDisplacementOperation> ops, List<bool> ego)
    {
        m_Chains.Add(new AvatarChainStructure(chain, ops, ego));
    }

    public void AddChain(List<HumanBodyBones> chain, bool end_effector_is_egocentric)
    {
        List<IDisplacementOperation> ops = new List<IDisplacementOperation>(chain.Count);
        List<bool> ego = new List<bool>(chain.Count);

        for (int i = 0; i < chain.Count; i++)
        {
            ops.Add(new EmptyDisplacement());

            if (i == 0 && end_effector_is_egocentric)
            {
                ego.Add(true);
            } else
            {
                ego.Add(false);
            }
        }

        m_Chains.Add(new AvatarChainStructure(chain, ops, ego));
    }

    public IEnumerable<AvatarChainStructure> chains()
    {
        foreach (AvatarChainStructure chain in m_Chains)
        {
            yield return chain;
        }
    }
}
