using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EgocentricRetargeting
{
    private BSAComponent m_sourceBSA;
    private BSAComponent m_destBSA;

    private BSAOperator m_referenceObject;
    private AvatarChainsHandler m_chainHandler;

    private AvatarTargetsComponent m_targetsComponent;

    private List<CCDIKComponent> m_IKs;

    private NormalAlignmentComponent m_normalAlignment;

    public EgocentricRetargeting(GameObject source, BodySurfaceApproximationDefinition source_BSAD, GameObject avatar, BodySurfaceApproximationDefinition avatar_BSAD, AvatarChainsHandler handler, Animator animator)
    {
        SetupAvatarBSA(source, source_BSAD, avatar, avatar_BSAD);

        //SETUP REFERENCE DATA OBJECT
        m_referenceObject = avatar.AddComponent<BSAOperator>();
        m_referenceObject.SetBSAComponents(m_sourceBSA, m_destBSA);

        //TARGETS
        m_targetsComponent = avatar.AddComponent<AvatarTargetsComponent>();
        m_targetsComponent.InstanceTargets();

        //SETUP CHAINS
        m_chainHandler = handler;

        //SETUP NORMALS
        m_normalAlignment = avatar.AddComponent<NormalAlignmentComponent>();
        m_normalAlignment.Init(animator, avatar_BSAD, m_destBSA);

        //CCDIK Components
        m_IKs = new List<CCDIKComponent>(m_chainHandler.length);

        foreach (AvatarChainStructure structure in m_chainHandler.chains())
        {
            CCDIKComponent ik_component = avatar.AddComponent<CCDIKComponent>();
            ik_component.Init(animator, structure.chain);
            m_IKs.Add(ik_component);

            for (int i = 0; i < structure.chain.Count; i++)
            {
                if (structure.egocentric[i])
                {
                    m_targetsComponent.RegisterEgocentricBone((int)structure.chain[i], structure.ops[i]);
                }
                else
                {
                    m_targetsComponent.RegisterBone((int)structure.chain[i], structure.ops[i]);
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

    private void BSAProjections()
    {
        foreach (AvatarChainStructure structure in m_chainHandler.chains())
        {
            for (int i = 0; i < structure.chain.Count; i++)
            {
                if (structure.egocentric[i])
                {
                    Vector3 set = m_referenceObject.Calculate(structure.chain[i]);
                    m_targetsComponent.SetTargetPosition(structure.chain[i], set);
                }
            }
        }
    }

    private void SolveIKs()
    {
        if (m_IKs.Count != m_chainHandler.length)
            throw new UnityException("IK components on object are not the same quantity as IK chains in handler object");

        int k = 0;
        foreach (AvatarChainStructure structure in m_chainHandler.chains())
        {
            List<Vector3> targets = new List<Vector3>(structure.chain.Count);

            foreach (HumanBodyBones hbb in structure.chain)
            {
                targets.Add(m_targetsComponent.GetTargetPosition(hbb));
            }

            m_IKs[k].IKSolver(targets);

            k++;
        }
    }

    public void Retarget(Animator animator)
    {
        //Set IK target for all bones to their current position
        m_targetsComponent.SetTargets(animator);

        BSAProjections();

        //Execute all mathematical operations defined in the chain handler
        m_targetsComponent.CompoundOperations();

        SolveIKs();

        //Normal Adjustments
        m_normalAlignment.RotateNormals();
    }
}
