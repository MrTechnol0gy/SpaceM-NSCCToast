using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Serialization;

public class ArcMeshGenerator : MonoBehaviour
{
    //Variables to control the resolution of the mesh. You can adjust how many horizontal and vertical segments there are. 
    [Header("Mesh Resolution")]
    [Range(10,36)]
    public int horizontalSegments = 12;
    [Range(1,6)]
    public int verticalSegments = 3;
    
    //Here you can control the horizontal and vertical angles
    [Header("Angles and Radius")]
    [Range(1, 180)]
    public float horizontalAngle = 180f;
    [Range(1,180)]
    public float verticalAngle = 180f;

    //This variable controls the radius.
    [Range(0,90)]
    public float radius = 10f;
    
    //This layermask is used for the raycasts so it will only detect things that match the correct layer.
    [Header("Misc")]
    public LayerMask obstacleLayermask = ~0;
    
    //There's a minimum of 3 segments for the mesh to exist, so the actual number of vertical segments are what is chosen on VerticalSegments times 2 + 1, so if 1 vertical segment is chosen it means 1 above and 1 below the center.
    private int adjustedNumberVerticalSegments => (verticalSegments *2) + 1;

    //This holds the mesh we'll be modifying
    private Mesh mesh;
    public MeshRenderer meshRenderer;
    
    //Here are the job handles, one for setting the vertices, one for setting up the raycastcommands and one for executing them.
    JobHandle meshModificationJobHandle, setupRaycastJobHandle, runRaycastsJob;
    
    //Here are where we hold the jobs we create.
    UpdateMeshJob meshModificationJob;
    SetupRaycastsJob setupRaycastJob;
    
    //These are the native arrays used in the jobs.
    private NativeArray<Vector3> meshVertices,InitialVerticesArray;
    private NativeArray<RaycastCommand> raycastCommands;
    private NativeArray<RaycastHit> raycastResults;

    public MeshCollider meshCollider;

    //Job for setting up the raycasts
    [BurstCompile]
    private struct SetupRaycastsJob : IJobParallelFor {
        //Here are the raycast commands we'll be writing to in this job.
        [WriteOnly]
        public NativeArray<RaycastCommand> Commands;
        
        //These are the vertices we'll be reading from to create our rays.
        [Unity.Collections.ReadOnly]
        public NativeArray<Vector3> vertices;
        
        public int layerMask;
        public Vector3 initialPos;
        public float radius;
        
        //The vertices are in local space, so we need a local to world matrix to convert them into world space.
        public Matrix4x4 localToWorldMatrix;
        
        public void Execute(int index) {
            //We multiply the vertice position with the localToWorldMatrix to get the world position of our vertice. 
            //Since our original vertices aren't built with the radius in them we multiply it here. 
            var worldVerticePos = localToWorldMatrix.MultiplyPoint(vertices[index] * radius);
            var desiredDistance = math.distance(initialPos, worldVerticePos);
            var getDirection = Vector3.Normalize(worldVerticePos - initialPos);
            //With the world position, direction and distance desired we create a Raycast Command and set it on our native array.
            Commands[index] = new RaycastCommand(initialPos, getDirection, new QueryParameters(layerMask,false,QueryTriggerInteraction.Ignore,false),desiredDistance);
        }
    }
    
    [BurstCompile]
    private struct UpdateMeshJob : IJobParallelFor
    {
        //This is where the results are stored we'll be reading from.
        [Unity.Collections.ReadOnly]
        public NativeArray<RaycastHit> Results;
        //We're also taking in the original vertices of the mesh so if our raycast fails we can fall back to those values.
        [Unity.Collections.ReadOnly]
        public NativeArray<Vector3> originalVerts;
        //We'll be then writing to this array with the results of our raycasts.
        [WriteOnly]
        public NativeArray<Vector3> vertices;
        public float radius;
        
        public Matrix4x4 inverseWorldMatrix;
        public void Execute(int index)
        {
            //Our raycasts found something if distance value is above 0, otherwise they didn't hit anything.
            if (Results[index].distance > 0.0001f) //Since the found position is in worldpsace we need to multiply it by WorldToLocal Matrix to get a localspace vertex position for our mesh.
                vertices[index] = inverseWorldMatrix.MultiplyPoint(Results[index].point);
            else vertices[index] = originalVerts[index] * radius;
            //Since our original vertices aren't built with the radius in them we multiply them above if we're using the original verts array.
        }
    }

    //I'm using this bool to know if I currently have a job scheduled, if I don't have a job scheduled it doesn't make sense trying to complete a job.
    private bool hasJobScheduled = false;
    public void Update()
    {
        //At the beginning of my frame if I have a job scheduled I'll complete it. I'm letting jobs run between a frame and the next.
        if (hasJobScheduled) {
            meshModificationJobHandle.Complete();
            hasJobScheduled = false;
            
            //I then take the resulting native array and set it to my mesh.
            mesh.SetVertices(meshModificationJob.vertices);
            mesh.RecalculateNormals();
        }
        
        //Here I'm setting up the raycast job with everything it needs to know.
        setupRaycastJob = new SetupRaycastsJob() {
            vertices = InitialVerticesArray,
            Commands = raycastCommands,
            layerMask = obstacleLayermask,
            initialPos = transform.position,
            localToWorldMatrix = transform.localToWorldMatrix,
            radius = this.radius* 0.5f
        };

        //I then schedule the job, I pass in 1 less vertex to it since my last vertice is my 0,0,0 position, it never changes. 
        setupRaycastJobHandle = setupRaycastJob.Schedule(meshVertices.Length-1,64);

        //I then schedule the raycast command to run with the previous job as a dependency, so it will only run after the previous one.
        runRaycastsJob = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 64, setupRaycastJobHandle);
        
        //I then setup the mesh update job with all it needs.
        meshModificationJob = new UpdateMeshJob() {
            Results = raycastResults,
            originalVerts = InitialVerticesArray,
            vertices = meshVertices,
            inverseWorldMatrix = transform.worldToLocalMatrix,
            radius = this.radius* 0.5f
        };
        
        //And then I schedule it with the raycastcommands job as a dependency so it will only run once all the raycasts have been done.
        meshModificationJobHandle = meshModificationJob.Schedule(meshVertices.Length-1, 64, runRaycastsJob);
        
        // I then start to run all the jobs.
        JobHandle.ScheduleBatchedJobs();
        
        //And this boolean will make it so the jobs get completed next frame.
        hasJobScheduled = true;
    }


    private void Start()
    {
        //I create the mesh on start.
        RecreateMesh();
    }

    private void OnDestroy() {
        //With NativeArrays it's very important to dispose of them after they're created, otherwise they stay around in memory.
        DisposeOfStuff();
    }

    private void DisposeOfStuff()
    {
        //Before disposing if there's a job currently running we need to complete this job. So I use the hasJobScheduled boolean to complete it if that's the case.
        if (hasJobScheduled) {
            meshModificationJobHandle.Complete();
            hasJobScheduled = false;
        }
        //Here I dispose of all of the nativearrays.
        if (meshVertices.IsCreated)
            meshVertices.Dispose();
        if (raycastCommands.IsCreated)
            raycastCommands.Dispose();
        if (raycastResults.IsCreated)
            raycastResults.Dispose();
        if (InitialVerticesArray.IsCreated)
            InitialVerticesArray.Dispose();
    }

    //I didn't feel like figuring out how many triangles would be the end result to use an array...
    //So I'm just using a list for the triangles.
    List<int> OutputTriangles = new List<int>();
    [ContextMenu("RecreateMesh")]
    public void RecreateMesh()
    {
        //Here I set the total number of indices I'll be iterating through.
        int totalPoints = (horizontalSegments * (adjustedNumberVerticalSegments)) + 1;
        
        //And I dispose of the native arrays in case I already have them, if the values for the mesh have changed we need to create new ones with the new number of total points.
        DisposeOfStuff();
        InitialVerticesArray = new NativeArray<Vector3>(totalPoints, Allocator.Persistent); // 2
        meshVertices = new NativeArray<Vector3>(totalPoints, Allocator.Persistent); // 2
        raycastCommands = new NativeArray<RaycastCommand>(totalPoints, Allocator.Persistent);
        raycastResults = new NativeArray<RaycastHit>(totalPoints, Allocator.Persistent);
        
        //Clear the triangles to start fresh
        OutputTriangles.Clear();
        
        //If there is no mesh we need to create one and mark it dynamic since we'll be updating it frequently
        if (mesh == null) {
            mesh = new Mesh();
            mesh.MarkDynamic();
            GetComponent<MeshFilter>().mesh = mesh;
            if (meshCollider != null) meshCollider.sharedMesh = mesh;
        }
        
        //For finding the vertice position I need the starting angle and the ending angle, so I use this variables to find all the information I need
        int halfSegments = Mathf.RoundToInt(horizontalSegments * 0.5f);
        float angleStep = this.horizontalAngle / (halfSegments);
        float startingAngle = 90 - (angleStep * halfSegments);
        float endingAngle = 90 + (angleStep * halfSegments);
        int halfSegmentsY = Mathf.RoundToInt(adjustedNumberVerticalSegments * 0.5f);
        float angleStepY = this.verticalAngle / (halfSegmentsY);
        float startingAngleY = 90 - (angleStepY * halfSegmentsY);
        float endingAngleY = 90 + (angleStepY * halfSegmentsY);
        float useRadius = radius * 0.5f;
        
        //I'm storing my 0,0,0 vertice at the end of the array, so I'll iterate through the array except for the last vertice.
        for (int i = 0; i < totalPoints-1; i++)
        {
            //I need to find what my X and y is for the current array position, so this is how I do it:
            int x = i % horizontalSegments;
            int y = Mathf.FloorToInt(i / horizontalSegments) - (verticalSegments);
            //This way Y goes 0 in the center right where this object is "looking" and then negative underneath and positive above.
            
            InitialVerticesArray[i] = GetVertexPositionForXY(startingAngle,endingAngle,startingAngleY,endingAngleY,x,y);
            meshVertices[i] = InitialVerticesArray[i] * useRadius;
            
            //If this is the last vertice I can skip the triangles for it since it's already included
            if (i >= totalPoints - 2) continue;
            
            //If this is true then this is one of the top vertices
            if (y == verticalSegments) {
                //I then add a triangle connecting it back to the 0,0,0 point.
                OutputTriangles.Add(totalPoints-1);  OutputTriangles.Add(i); OutputTriangles.Add(i+1);
                continue;
            }
            //If this is the last vertice of each row
            if (i % horizontalSegments == horizontalSegments - 1) {
                //I then add a triangle connecting it back to the 0,0,0 point.
                OutputTriangles.Add(i);  OutputTriangles.Add(totalPoints-1);OutputTriangles.Add(i+horizontalSegments);
                //I can continue since the last vertice already has a triangle made by the previous one.
                continue;
            }
            //If this is the first vertice of each row
            if (x == 0) {
                //I then add a triangle connecting it back to the 0,0,0 point.
                OutputTriangles.Add(totalPoints-1);  OutputTriangles.Add(i);OutputTriangles.Add(i+horizontalSegments);
            }
            //If this is the bottom row of vertices
            if (y == -verticalSegments) {
                //I then add a triangle connecting it back to the 0,0,0 point.
                OutputTriangles.Add(totalPoints-1);  OutputTriangles.Add(i+1);OutputTriangles.Add(i);
            }
            
            // Here I add the first triangle
            OutputTriangles.Add(i);  OutputTriangles.Add(i+1);OutputTriangles.Add(i+horizontalSegments);
            // And then the second triangle
            OutputTriangles.Add(i+1);  OutputTriangles.Add(i + horizontalSegments + 1);OutputTriangles.Add(i+horizontalSegments);
        }

        //Now the initial vertice and triangle list is created I can set it back to the mesh and recalculate bounds and normals.
        mesh.SetVertices(meshVertices);
        mesh.SetTriangles(OutputTriangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        
        if (meshCollider != null)
        {
            meshCollider.enabled = false;
            meshCollider.sharedMesh = null;
            meshCollider.enabled = true;
            meshCollider.enabled = false;
            meshCollider.sharedMesh = mesh;
            meshCollider.enabled = true;
        }
    }

    //I'm using OnDrawGizmosSelected to see visually how the mesh is going to with gizmo spheres
    private void OnDrawGizmosSelected() {
        int halfSegments = Mathf.RoundToInt(horizontalSegments * 0.5f);
        int totalPoints = horizontalSegments * (adjustedNumberVerticalSegments);
        float angleStep = this.horizontalAngle / (halfSegments);
        float startingAngle = 90 - (angleStep * halfSegments);
        float endingAngle = 90 + (angleStep * halfSegments);
        int halfSegmentsY = Mathf.RoundToInt(adjustedNumberVerticalSegments * 0.5f);
        float angleStepY = this.verticalAngle / (halfSegmentsY);
        float startingAngleY = 90 - (angleStepY * halfSegmentsY);
        float endingAngleY = 90 + (angleStepY * halfSegmentsY);
        float useRadius = radius * 0.5f;

        Gizmos.color = Color.yellow;
        
        for (int i = 0; i < totalPoints; i++) { 
            int x = i % horizontalSegments;
            int y = Mathf.FloorToInt(i / horizontalSegments) - (verticalSegments);
            
            var desiredArchPoint = GetVertexPositionForXY(startingAngle, endingAngle,startingAngleY,endingAngleY, x, y);

            Gizmos.DrawSphere(transform.TransformPoint(desiredArchPoint * useRadius), 0.05f);
        }
    }

    
    //This is where I actually find the position for each vertex using their X and Y index and their starting/ending angles.
    private Vector3 GetVertexPositionForXY(float startingAngle, float endingAngle,float startingAngleY, float endingAngleY, int x, int y) {
        //For the horizontal angle I lerp between starting and ending horizontal angle using an inverse lerp of x between 0 and maximum x value
        float azimuthalAngle = Mathf.Lerp(startingAngle,endingAngle,Mathf.InverseLerp(0, horizontalSegments-1, x)) * Mathf.Deg2Rad;
        //For the vertical angle I lerp between starting and ending vertical angle using an inverse lerp of x between -vertical and maximum y vertical
        float polarAngle = Mathf.Lerp(startingAngleY,endingAngleY,Mathf.InverseLerp(-verticalSegments, verticalSegments, y)) * Mathf.Deg2Rad;
        
        //I find the center angle value
        float centerAngle = Mathf.Lerp(startingAngle, endingAngle, 0.5f) * Mathf.Deg2Rad;
        //I then find the distance between Y/X and the center value, for Y that's simply ABS(Y)
        float yDistance = Mathf.InverseLerp(0,verticalSegments,Mathf.Abs(y));
        //For X to find the distance I need to subtract half the number of segments so it goes from -halfX to +halfX with the middle being 0.
        float halfNumberSegments = (horizontalSegments-1) * 0.5f;
        float xDistance = Mathf.InverseLerp(0,halfNumberSegments,Mathf.Abs(x - halfNumberSegments));
        //I now have 2 distance values that go from 0 to 1 based on how far away they are from the center value.
        
        //I then lerp back the horizontal and vertical angles a bit back to the center angle, using an inverse lerp of the sum of both distances. That has the effect of pulling the farther away points on the edges a bit closer to the center, making it more of a cone.
        azimuthalAngle = Mathf.Lerp(azimuthalAngle, centerAngle, (  0.33f * Mathf.InverseLerp(0,2,xDistance+yDistance)));
        polarAngle = Mathf.Lerp(polarAngle, centerAngle, (  0.33f * Mathf.InverseLerp(0,2,xDistance+yDistance)));

        //I then construct the vector using Sin/Cos of the angles and return it.
        float posx = Mathf.Sin(polarAngle) * Mathf.Cos(azimuthalAngle);
        float posy = Mathf.Cos(polarAngle);
        float posz = Mathf.Sin(polarAngle) * Mathf.Sin(azimuthalAngle);
        return new Vector3(posx, posy, posz);
    }

}