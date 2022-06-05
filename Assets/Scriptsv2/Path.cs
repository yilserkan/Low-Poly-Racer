using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField] private Transform path;
    [SerializeField] private Color lineColor;

    private List<Transform> pathNodes = new List<Transform>();

    private void OnDrawGizmos() 
    {
        Transform[] pathTransforms = GetComponentsInChildren<Transform>();
        pathNodes = new List<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if(pathTransforms[i] != transform)
                pathNodes.Add(pathTransforms[i]);
            
        }
        for (int i = 0; i < pathNodes.Count; i++)
        {
            Vector3 currPos = pathNodes[i].position;
            Vector3 prevPos = new Vector3(); 
            if(i > 0)
            {
                prevPos = pathNodes[i-1].position;
            }
            else if(i == 0 && pathNodes.Count > 1)
            {
                prevPos = pathNodes[pathNodes.Count - 1].position;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(prevPos, currPos);
            Gizmos.DrawSphere(currPos, 2f);
        }
    }

}
