using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EgocentricRayCasterWrapper : MonoBehaviour
{
    private EgocentricRayCasterSource m_source;
    private EgocentricRayCasterDestination m_destination;

    public void Set(EgocentricRayCasterSource source, EgocentricRayCasterDestination destination)
    {
        m_source = source;
        m_destination = destination;
    }

    public List<BSACoordinates> GetSourceCoordinates()
    {
        List<BSACoordinates> tmp = m_source.Cast();
        return tmp;
    }

    public Vector3 SetDestinationCoordinates(List<BSACoordinates> coordinates)
    {
        return m_destination.Calculate(coordinates);
    }

    public Quaternion MatchPlaneNormal(HumanBodyBones hbb)
    {
        Quaternion q = m_destination.CompareNormals(hbb);
        return q;
    }
}
