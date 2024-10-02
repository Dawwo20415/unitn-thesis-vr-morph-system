using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EgocentricRetargeting
{
    private BSAComponent m_sourceBSA;
    private BSAComponent m_destBSA;

    private TestEgocentricOutput m_referenceObject;
    private AvatarChainsHandler m_chainHandler;

    private AvatarTargetsComponent m_tComponent;

    public EgocentricRetargeting(GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject avatar, BodySurfaceApproximationDefinition avatar_BSAD, AvatarChainsHandler handler)
    {
        SetupAvatarBSA(source, source_BSAD, avatar, avatar_BSAD);

        //SETUP REFERENCE DATA OBJECT
        m_referenceObject = avatar.AddComponent<TestEgocentricOutput>();
        m_referenceObject.SetBSAComponents(m_sourceBSA, m_destBSA);
        m_referenceObject.InstanceTargets();

        //TARGETS
        m_tComponent = avatar.AddComponent<AvatarTargetsComponent>();
        m_tComponent.InstanceTargets();

        //SETUP CHAINS
        m_chainHandler = handler;

        foreach (AvatarChainStructure structure in m_chainHandler.chains())
        {
            for (int i = 0; i < structure.chain.Count; i++)
            {
                if (structure.egocentric[i])
                {
                    m_tComponent.RegisterEgocentricBone((int)structure.chain[i], structure.ops[i]);
                }
                else
                {
                    m_tComponent.RegisterBone((int)structure.chain[i], structure.ops[i]);
                }
            }
        }
    }

    private void SetupAvatarBSA(GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject dest, BodySurfaceApproximationDefinition dest_BSAD)
    {
        m_sourceBSA = source.AddComponent<BSAComponent>();
        m_sourceBSA.BSAD = source_BSAD;

        m_destBSA = dest.AddComponent<BSAComponent>();
        m_destBSA.BSAD = dest_BSAD;
    }

    public void Project(HumanBodyBones hbb)
    {
        Vector3 previous = m_referenceObject.GetTarget(hbb);
        Vector3 result = m_referenceObject.Calculate(hbb);
    }

    public void Targets(Animator animator)
    {
        m_tComponent.SetTargets(animator);
        m_tComponent.CompoundOperations();
    }
}
