# How to use the Unity Job System with Burst
Source code for the tutorial on how to use the job system

[![https://www.youtube.com/watch?v=1VZaW4_quzI](https://img.youtube.com/vi/1VZaW4_quzI/0.jpg)](https://www.youtube.com/watch?v=1VZaW4_quzI)


# Intro

Performance is crucial for real-time 3D software, like games.
The Unity Job System combined with Burst is a great tool to obtain a huge performance increase for parallel tasks.
In my current graduation project, where I deform meshes in real-time, it got me from 5ms to 0.1ms on the main thread. This is done with a mesh with over 80.000 vertices. This performance is great for standalone VR.
Most calculations in `for` loops can be made parallel using the job system, as long as it does not make any references to managed code. References to Transform can be made using their own job type, as this is a common use case.
This guide will show you the basics of how to implement the job system in your project.

This guide is made in Unity 2021.3LTS, but will also work in other Unity versions.


# What you need to know

To properly follow this guide, it is expected that you know the basics of Unity development and C#. 
Topics like performance metrics, Unity Profiler, and coding conventions will be used to demonstrate the Unity Job System combined with the Unity Burst Compiler. More advanced code conventions will be mentioned in the video, but are not required to write code using Unity Jobs + Burst. These can be seen as additional info that can be looked into more deeply when you are working with extremely high-performance codes, like real-time physics simulations of a 3D environment.


# How to Implement

The original non-jobified code can be found in the [TutorialStart branch](https://github.com/smitdylan2001/JobSystemTutorial/tree/TutorialStart). You can use this if you want to learn how to implement the job system step by step with a demo project!
The focus on this guide will be on a `for` loop, some steps can be skipped if you use a singular job.


## Step 1 - Install Burst, Collections, and Mathematics

- Open your project and head to the Package Manager
- Go to Unity Registry and install Burst, Collections, and Mathematics
- (optional) Go to [project root]/Packages/manifest.json and change the Burst and Collections package to the latest version available. (at the point of recording 1.8.4 for burst, and 1.4.0 for Collections. Collections is on 2.1.4 for 2022.2+)
- Restart the Unity editor for Burst to apply

## Step 2 - Write out the job

- (optional - to manage code) Make a new script for your jobs
- Make sure you call `using Unity.Jobs; using Unity.Burst; using Unity.Collections;` at the top of your script
- Make a new struct and inherit from `IJobParralelFor` when using `for` loops, `IJobParralelForTransform` when using a `for` loop where each entry has a transform component, `IJob` when using single jobs, `IJobFor` when using a loop in sequence instead of parallel 
- Implement the interface's `Execute()` method (`Execute(int index)` when using `for` loops)
- Move the code from the original for loop to the Execute() method
- Add the required variables in the struct using NativeContainers instead of regular Arrays and Lists (example: Vector3[] -> NativeArray<float3>)
- Use `[ReadOnly]` for values that will only be read and `[WriteOnly]` to values which are only written to so Burst can optimize the job
- Change Vector values to float instead (example: Vector3 -> float3)
- Change any Mathf calculations to the new mathematics library where needed
- Add `[BurstCompile]` to the top of the struct so Burst can compile the job

## Step 3 - Prepare and schedule the job
  
- Go to the script that the `for` loop was originally in
- Change the lists and arrays to their NativeCollection, mathematics compatible counterparts (example: Vector3[] -> NativeArray<float3>)
- Create a new job (example: `deformJob = new DeformMeshJob()`) and fill in the variables required
- Use a JobHandle to schedule the jobs (example: `deformJobHandle = deformJob.Schedule(verticesNativeArray.Length, 64);`. Here the first value is the amount of iterations in the loop, and the second value the amount of jobs batched together (so not every job has to be scheduled individually)
- (optional - if used asynchronously) Use `JobHandle.ScheduleBatchedJobs();` to start execution on the jobs in the background
- Call `.Complete()` on the job handle in order to complete the job, this will stall the main thread if the job is not completed yet. (example: `deformJobHandle.Complete();`)
- (optional - if used asynchronously) Use `.Complete()` in `LateUpdate()` or at the next `Update()` so code can run in the background instead
- After completing the job use the values. (example: `mesh.SetVertices(deformJob.NewVertices);`)
- Deallocate any Native Collections, otherwise you might cause a memory leak.
  
  
  
# References
  
Official Job System documentation: https://docs.unity3d.com/Manual/JobSystem.html
  
Official Burst Compiler documentation: https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/index.html
  
Latest Burst version: https://docs.unity3d.com/Packages/com.unity.burst@1.8/changelog/CHANGELOG.html
  
Latest Collections verion (Unity 2021): https://docs.unity3d.com/Packages/com.unity.collections@1.4/changelog/CHANGELOG.html

Latest Collections version (Unity 2022+): https://docs.unity3d.com/Packages/com.unity.collections@2.1/changelog/CHANGELOG.html

Latest Mathematics version: https://docs.unity3d.com/Packages/com.unity.mathematics@1.2/changelog/CHANGELOG.html
