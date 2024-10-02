using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSAOperator : MonoBehaviour
{
    private EgocentricProjectionDebug source_debug;
    private EgocentricProjectionDebug destin_debug;

    [SerializeField] private BSAComponent m_SourceBSA;
    [SerializeField] private BSAComponent m_DestBSA;

    public void SetBSAComponents(BSAComponent source, BSAComponent dest)
    {
        m_SourceBSA = source;
        m_DestBSA = dest;

        source_debug = new EgocentricProjectionDebug(m_SourceBSA.BSAD.coordinateSpan);
        destin_debug = new EgocentricProjectionDebug(m_DestBSA.BSAD.coordinateSpan);
    }

    public Vector3 Calculate(HumanBodyBones hbb)
    {
        List<BSACoordinates> coords = m_SourceBSA.Project(hbb, ref source_debug);
        return m_DestBSA.ReverseProject(hbb, coords, ref destin_debug);
    }

    private void OnDrawGizmos()
    {
        source_debug.OnGizmoDraw(true, true);
        destin_debug.OnGizmoDraw(true, true);
    }
}
