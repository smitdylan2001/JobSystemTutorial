using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;

[BurstCompile]
public struct DeformMeshJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> OriginalVertices;
    [ReadOnly] public float3 RipplePosition;
    [ReadOnly] public float SineTime;

    [WriteOnly] public NativeArray<float3> NewVertices;

    public void Execute(int index)
    {
        float3 originalVertex = OriginalVertices[index];
        float distance = math.distance(originalVertex, RipplePosition);
        float rippleAmount = math.sin(distance - SineTime);
        float3 offset = math.normalize(originalVertex - RipplePosition) * rippleAmount;
        float3 newPos = originalVertex + offset;

        NewVertices[index] = newPos;
    }
}
