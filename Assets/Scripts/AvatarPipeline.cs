using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarPipeline : MonoBehaviour
{


    /*
     * 0. Creation of Alternative Avatar by Optitrack proportions
     * 1. Setting point positions from optitrack (base)
     * 2. Modifying point positions from egocentric body mapping
     * 3. Modifying point positions from point displacement addon
     * 
     * At a certain point there is the need to recalculate the IK of arms and legs
     * Mecanim works only with the orientations and then the length of segments is fixed, even if you
     * change positions of points it is of no impact on the final Avatar Position
     * 
     * IK should be recalculated at the end of the whole process
     * 
     * Using the optitrack position as baseline we can choose the transformation that is nearest to the original
     * 
     * Points like the elbow need to be classified as derivative? Like they are the ones in which both position and orientation needs to be recalculated
     * Meanwhile some points you set position and need to only ricalculate the orientation
     * For some you don't need to calcuate either, like the hands. 
     * 
     * This pipeline needs to be like agnostic to the starting avatar
     *  Probably will need a specification to know it it is starting and ending on the same avatar because
     *  of that whole thing about local coordinates, and whatnot
     *  
     * 
     */

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
