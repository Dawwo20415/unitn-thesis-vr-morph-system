using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;
using UnityEngine;

public class AnimatorPlayableDebugging : MonoBehaviour
{
    [SerializeField, Range(0.0f, 1.0f)]
    private float weight;

    private Animator animator;
    private PlayableGraph graph;
    private PlayableOutput output;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        graph = animator.playableGraph;
        output = graph.GetOutput(0);
        weight = output.GetWeight<PlayableOutput>();
    }

    // Update is called once per frame
    void Update()
    {
        output.SetWeight<PlayableOutput>(weight);
    }
}
