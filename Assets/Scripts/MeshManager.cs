using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshManager : MonoBehaviour
{
    enum MeshUpdateState
    {
        Normal,
        Jobified,
        JobifiedAsyncLateUpdate,
        JobifiedAsyncNextUpdate,
    }

    //General
    [SerializeField] Vector3 rippleOrigin;
    [SerializeField] MeshUpdateState updateState;

    Mesh mesh;

    //Regular
    Vector3[] verticesArray, newVerticesArray;

    //Jobified
    NativeArray<float3> verticesNativeArray, newVerticesNativeArray;
    DeformMeshJob deformJob;
    JobHandle deformJobHandle;

    bool canUpdate;

    private void Start()
    {
        //Setup general data
        mesh = GetComponent<MeshFilter>().mesh;

        //Setup regular deform data
        verticesArray = mesh.vertices;
        newVerticesArray = new Vector3[mesh.vertexCount];

        //Setup jobified data
        verticesNativeArray = new NativeArray<float3>(mesh.vertexCount, Allocator.Persistent);
        newVerticesNativeArray = new NativeArray<float3>(mesh.vertexCount, Allocator.Persistent);
        using (var dataArray = Mesh.AcquireReadOnlyMeshData(mesh))
        {
            dataArray[0].GetVertices(verticesNativeArray.Reinterpret<Vector3>());
        }
    }

    private void Update()
    {
        switch (updateState)
        {
            case MeshUpdateState.Normal:

                for (int i = 0; i < verticesArray.Length; i++)
                {
                    Vector3 originalVertex = verticesArray[i];
                    float distance = Vector3.Distance(originalVertex, rippleOrigin);
                    float rippleAmount = Mathf.Sin(distance - Time.time);
                    Vector3 offset = (originalVertex - rippleOrigin).normalized * rippleAmount;
                    Vector3 newPos = originalVertex + offset;

                    newVerticesArray[i] = newPos;
                }
                mesh.SetVertices(newVerticesArray);

                break;
            case MeshUpdateState.Jobified:

                deformJob = new DeformMeshJob()
                {
                    OriginalVertices = verticesNativeArray,
                    RipplePosition = rippleOrigin,
                    SineTime = Time.time,

                    NewVertices = newVerticesNativeArray,
                };
                deformJobHandle = deformJob.Schedule(verticesNativeArray.Length, 64);
                UpdateMesh();

                break;
            case MeshUpdateState.JobifiedAsyncLateUpdate:

                deformJob = new DeformMeshJob()
                {
                    OriginalVertices = verticesNativeArray,
                    RipplePosition = rippleOrigin,
                    SineTime = Time.time,

                    NewVertices = newVerticesNativeArray,
                };
                deformJobHandle = deformJob.Schedule(verticesNativeArray.Length, 64);
                JobHandle.ScheduleBatchedJobs();

                break;
            case MeshUpdateState.JobifiedAsyncNextUpdate:

                if(canUpdate) UpdateMesh();

                deformJob = new DeformMeshJob()
                {
                    OriginalVertices = verticesNativeArray,
                    RipplePosition = rippleOrigin,
                    SineTime = Time.time,

                    NewVertices = newVerticesNativeArray,
                };
                deformJobHandle = deformJob.Schedule(verticesNativeArray.Length, 64);
                JobHandle.ScheduleBatchedJobs();
                canUpdate = true;
                break;
        }
    }

    private void LateUpdate()
    {
        if(updateState == MeshUpdateState.JobifiedAsyncLateUpdate) UpdateMesh();
    }

    private void OnApplicationQuit()
    {
        if(verticesNativeArray.IsCreated) verticesNativeArray.Dispose();
        if(newVerticesNativeArray.IsCreated) newVerticesNativeArray.Dispose();
    }

    private void UpdateMesh()
    {
        deformJobHandle.Complete();

        mesh.SetVertices(deformJob.NewVertices);
    }
}
