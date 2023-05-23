using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFunctions : MonoBehaviour
{
    [SerializeField] int noiseCount;

    void Update()
    {
        for (int i = 0; i < noiseCount; i++)
        {
            transform.GetLocalPositionAndRotation(out Vector3 pos, out Quaternion rot);
            transform.SetLocalPositionAndRotation(pos, rot);
        }
    }
}
