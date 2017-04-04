using PhysxNet;
using System.IO;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace PhysxNetTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Foundation foundation = new Foundation();
            Physics physics = new Physics(foundation, new ToleranceScale(1.0f, 9.81f, 1.0f));
            CookingParams cookingParams = new CookingParams(new ToleranceScale(1.0f,9.81f,1.0f));
            Cooking cooking = new Cooking(foundation, physics, cookingParams);

            List<PxVec3> vertices = new List<PxVec3>() {
                new PxVec3 (0, 0, 0),
                new PxVec3 (1, 0, 0),
                new PxVec3 (1, 1, 0),
                new PxVec3 (0, 1, 0),
                new PxVec3 (0, 1, 1),
                new PxVec3 (1, 1, 1),
                new PxVec3 (1, 0, 1),
                new PxVec3 (0, 0, 1),
            };

            List<int> indices = new List<int>() {
                0, 2, 1, //face front
	            0, 3, 2,
                2, 3, 4, //face top
	            2, 4, 5,
                1, 2, 5, //face right
	            1, 5, 6,
                0, 7, 4, //face left
	            0, 4, 3,
                5, 4, 7, //face back
	            5, 7, 6,
                0, 6, 7, //face bottom
	            0, 1, 6
            };

            ConvexMeshDesc convexDesc = new ConvexMeshDesc(vertices, indices);
            Console.WriteLine("Cooking convex mesh");
            Stopwatch w = new Stopwatch();
            w.Start();
            PhysicsMesh convexMesh = cooking.CreateConvexMesh(convexDesc);
            w.Stop();
            Console.WriteLine("Complete in " + w.ElapsedMilliseconds + " ms");
            File.WriteAllBytes("convexMesh.txt", convexMesh.MeshData.ToArray());

            Console.WriteLine("Cooking triangle mesh");
            w = new Stopwatch();
            w.Start();
            TriangleMeshDesc triangleDesc = new TriangleMeshDesc(vertices, indices);
            PhysicsMesh triangleMesh = cooking.CreateTriangleMesh(triangleDesc);
            w.Stop();
            File.WriteAllBytes("triangleMesh.txt", triangleMesh.MeshData.ToArray());
            Console.WriteLine("Complete in " + w.ElapsedMilliseconds + " ms");

            Console.WriteLine("Complete");

            cooking.Release();
            physics.Release();
            foundation.Release();
        }
    }
}