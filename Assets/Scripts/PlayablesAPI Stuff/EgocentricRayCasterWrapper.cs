using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BSACoordinates
{
    //Normalized weight of the displacement vector
    public float weight;
    //Normalized vector from Surface Projection to Joint
    public Vector3 displacement;
    //Barycentric/Cylindrical coordinates of the joint projected on mesh
    public Vector2 surfaceProjection;

    public BSACoordinates(float i)
    {
        weight = i;
        displacement = new Vector3(i, i, i);
        surfaceProjection = new Vector2(i, i);
    }

    public BSACoordinates(Vector2 sp, Vector3 d, float w)
    {
        weight = w;
        displacement = d;
        surfaceProjection = sp;
    }
}

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
