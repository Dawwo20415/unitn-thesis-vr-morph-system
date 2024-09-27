using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public interface IDisplacementOperation
{
    public Vector3 Operation(Vector3 target);
}

public struct EmptyDisplacement : IDisplacementOperation
{
    public Vector3 Operation(Vector3 target)
    {
        return target;
    }
}

public struct AmplyfyDisplacement : IDisplacementOperation
{
    public float scale;

    public AmplyfyDisplacement(float value)
    {
        scale = value;
    }

    public Vector3 Operation(Vector3 target)
    {
        return target * scale;
    }
}

public struct ScalarDisplacement : IDisplacementOperation
{
    public Vector3 displacement;

    public ScalarDisplacement(Vector3 value)
    {
        displacement = value;
    }

    public Vector3 Operation(Vector3 target)
    {
        return target + displacement;
    }
}

public class TargetDisplacementBehaviour : PlayableBehaviour
{
    List<HumanBodyBones> m_Chain;
    List<IDisplacementOperation> m_Ops;

    public void Setup(List<HumanBodyBones> chain, List<IDisplacementOperation> ops)
    {
        if (chain.Count != ops.Count) { throw new UnityException("In Target Displacement Behavior chain and operations are not of the same length"); }
        m_Chain = chain;
        m_Ops = ops;
    }

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!playerData.GetType().Equals(typeof(TestEgocentricOutput))) { return; }

        TestEgocentricOutput output = (TestEgocentricOutput)playerData;

        for (int i = 0; i < m_Chain.Count; i++)
        {
            Vector3 target = output.GetTarget(m_Chain[i]);
            Vector3 pre = target;
            target = m_Ops[i].Operation(target);
            Vector3 post = target;
            output.SetTarget(m_Chain[i], target);
            if (m_Chain[i] == HumanBodyBones.RightHand)
                Debug.Log(m_Chain[i].ToString() + " displacement from " + pre + " to " + post + " | Subsequent GetTarget " + output.GetTarget(m_Chain[i]));
        }
    }
}
