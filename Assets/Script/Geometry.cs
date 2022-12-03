using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geometry
{
    public static bool SegmentSegmentIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 line1point2, Vector3 linePoint2, Vector3 line2point2) {
        Vector3 lineVec1 = line1point2 - linePoint1;
        Vector3 lineVec2 = line2point2 - linePoint2;

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);                

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if(Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f) {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            lineVec3 = linePoint1 - linePoint2;
            crossVec1and2 = Vector3.Cross(lineVec2, lineVec1);
            crossVec3and2 = Vector3.Cross(lineVec3, lineVec1);
            float s2 = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

            intersection = linePoint1 + (lineVec1 * s);  

            // Check if intersection is inside both segments
            if(s < 0 || s > 1 || s2 < 0 || s2 > 1) return false; 
            return true;
        } else {        
            intersection = Vector3.zero;
            return false;
        }
    }
}
