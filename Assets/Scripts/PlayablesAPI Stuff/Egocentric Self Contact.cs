using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class EgocentricBehaviour : PlayableBehaviour, IKTarget
{
    private Vector3 target;
    
    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData) 
    {
        EgocentricRayCaster caster = (EgocentricRayCaster)playerData;
        Vector3 result = caster.Cast();
        Debug.Log("Result for Egocentric Self Contact Retargeting " + VExtension.Print(result));
    }

    public Vector3 GetTarget()
    {
        return target;
    }
}

public class EgocentricSelfContact 
{
    //SelfContact Meshes
    private Material m_material;
    private Mesh m_armMesh;
    private float m_armThickness;

    List<GameObject> m_customMeshes;
    List<GameObject> m_cylinders;

    //PlayableGraph
    private List<Playable> m_egoPlayables;
    private List<ScriptPlayableOutput> m_egoOutputs;
    private Dictionary<HumanBodyBones, int> m_hbbConversion;

    public Playable this[int i] { get => m_egoPlayables[i]; }
    public Playable this[HumanBodyBones hbb] { get => m_egoPlayables[m_hbbConversion[hbb]]; }

    public EgocentricSelfContact(Animator animator, PlayableGraph graph, Material mat, Mesh arm, float thick, List<CustomAvatarCalibrationMesh> acms, List<HumanBodyBones> joints, EgocentricRayCaster.DebugStruct egoDebug)
    {
        m_material = mat;
        m_armMesh = arm;
        m_armThickness = thick;
        SetupAvatar(animator, graph, acms, joints, egoDebug);
    }

    public void SetupAvatar(Animator animator, PlayableGraph graph, List<CustomAvatarCalibrationMesh> acms, List<HumanBodyBones> joints, EgocentricRayCaster.DebugStruct egoDebug)
    {
        GameObject parent = new GameObject("Egocentric Stuff");
        parent.transform.parent = animator.avatarRoot;

        m_customMeshes = new List<GameObject>(acms.Count);
        m_cylinders = new List<GameObject>(8);

        { // Arm Cylinders
            InstanceCylinders(animator, new List<HumanBodyBones>() { HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand }, parent.transform, "Left Arm");
            InstanceCylinders(animator, new List<HumanBodyBones>() { HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand }, parent.transform, "Right Arm");
            InstanceCylinders(animator, new List<HumanBodyBones>() { HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot }, parent.transform, "Left Leg");
            InstanceCylinders(animator, new List<HumanBodyBones>() { HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot }, parent.transform, "Right Leg");
        }

        { // Custom Meshes
            foreach (CustomAvatarCalibrationMesh acm in acms)
            {
                InstanceCustomMesh(animator, acm, parent.transform);
            }
        }

        { // Add Raycasters & Playables
            m_egoPlayables = new List<Playable>(joints.Count);
            m_egoOutputs = new List<ScriptPlayableOutput>(joints.Count);
            m_hbbConversion = new Dictionary<HumanBodyBones, int>(joints.Count);

            foreach (HumanBodyBones hbb in joints)
            {
                EgocentricRayCaster caster = animator.GetBoneTransform(hbb).gameObject.AddComponent<EgocentricRayCaster>();
                caster.Setup(m_customMeshes, m_cylinders, egoDebug);
                InstancePlayables(graph, hbb, caster);
            }
        }
    }

    private void InstanceCustomMesh(Animator animator, CustomAvatarCalibrationMesh acm, Transform parent)
    {
        GameObject obj = new GameObject(acm.mesh_name);
        obj.transform.parent = parent;

        MeshFilter filter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
        ObjectBoneFollow follow = obj.AddComponent<ObjectBoneFollow>();

        filter.mesh = acm.getMesh();
        renderer.material = m_material;

        List<Transform> anchors = new List<Transform>();

        foreach (HumanBodyBones hbb in acm.anchors)
        {
            Transform trn = animator.GetBoneTransform(hbb);
            anchors.Add(trn);
            if (!trn) { Debug.Log("No transform found in animator for hbb: " + hbb); }
        }

        follow.calibrate(anchors, acm.position_offset, acm.rotation_offset, acm.getScale());
        m_customMeshes.Add(obj);
    }
    private void InstanceCylinders(Animator animator, List<HumanBodyBones> bones, Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        group.transform.parent = parent;
        for (int i = 0; i < bones.Count - 1; i++)
        {
            Transform from = animator.GetBoneTransform(bones[i]);
            Transform to = animator.GetBoneTransform(bones[i + 1]);

            GameObject obj = GenerateCylinder(from, to);
            obj.transform.parent = group.transform;
            m_cylinders.Add(obj);
        }
    } 
    private GameObject GenerateCylinder(Transform a, Transform b)
    {

        GameObject capsule = new GameObject("Capsule_" + a.name);
        MeshFilter filter = capsule.AddComponent<MeshFilter>();
        MeshRenderer renderer = capsule.AddComponent<MeshRenderer>();
        ObjectBoneFollow follow = capsule.AddComponent<ObjectBoneFollow>();
        follow.enabled = false;

        filter.mesh = m_armMesh;
        renderer.material = m_material;

        Vector3 position = (a.position + b.position) / 2;
        Vector3 pointer = (a.position - position).normalized;
        float distance = (b.position - a.position).magnitude;
        Quaternion rotation = Quaternion.LookRotation(pointer) * Quaternion.Euler(new Vector3(90, 0, 0));
        Vector3 scale = new Vector3(m_armThickness, distance / 2, m_armThickness);

        follow.calibrate(new List<Transform>() { a, b }, position, rotation, scale);
        follow.enabled = true;
        return capsule;
    }
    private void InstancePlayables(PlayableGraph graph, HumanBodyBones hbb, EgocentricRayCaster component)
    {
        ScriptPlayable<EgocentricBehaviour> playable = ScriptPlayable<EgocentricBehaviour>.Create(graph);
        EgocentricBehaviour behaviour = playable.GetBehaviour();

        ScriptPlayableOutput output = ScriptPlayableOutput.Create(graph, hbb.ToString() + "_ESC_ScriptOutput");
        output.SetUserData(component);

        //Connections
        playable.SetOutputCount(2);
        output.SetSourcePlayable(playable, 1);

        //Store
        m_egoPlayables.Add(playable);
        m_egoOutputs.Add(output);
        m_hbbConversion[hbb] = m_egoPlayables.Count - 1;
    }
}
