using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using Assimp;
using FluxConverterTool.Helpers;
using FluxConverterTool.ViewModels;
using FluxConverterTool.Views;
using PhysxNet;
using SharpDX;
using System;
using System.Runtime.InteropServices;


namespace FluxConverterTool.Models
{
    public enum FLUX_VERSION : byte
    {
        MIN = 7,
        MAX = 7,
    }

    public class MeshConvertRequest
    {
        public Queue<FluxMesh> MeshQueue;
        public string SaveDirectory;
    };

    public class MeshFormatter
    {
        private Foundation _foundation;
        public Cooking Cooking;
        private Physics _physics;

        public void Initialize()
        {
            _foundation = new Foundation();
            _physics = new Physics(_foundation);
            Cooking = new Cooking(_foundation, _physics);

            DebugLog.Log("Initialized", "Mesh Formatter");
        }

        public void Shutdown()
        {
            Cooking.Release();
            _physics.Release();
            _foundation.Release();

            DebugLog.Log("Shutdown", "Mesh Formatter");
        }

        private BoundingBox GetBoundingBox(Scene scene)
        {
            SharpDX.BoundingBox box = new SharpDX.BoundingBox();
            if (scene.HasMeshes)
            {
                foreach (Mesh mesh in scene.Meshes)
                {
                    SharpDX.BoundingBox meshBox = GetBoundingBox(mesh);
                    box.Minimum.X = Math.Min(box.Minimum.X, meshBox.Minimum.X);
                    box.Minimum.Y = Math.Min(box.Minimum.Y, meshBox.Minimum.Y);
                    box.Minimum.Z = Math.Min(box.Minimum.Z, meshBox.Minimum.Z);

                    box.Maximum.X = Math.Max(box.Maximum.X, meshBox.Maximum.X);
                    box.Maximum.Y = Math.Max(box.Maximum.Y, meshBox.Maximum.Y);
                    box.Maximum.Z = Math.Max(box.Maximum.Z, meshBox.Maximum.Z);
                }
            }

            BoundingBox newBox = new BoundingBox();
            newBox.Center.X = (box.Minimum.X + box.Maximum.X) / 2.0f;
            newBox.Center.Y = (box.Minimum.Y + box.Maximum.Y) / 2.0f;
            newBox.Center.Z = (box.Minimum.Z + box.Maximum.Z) / 2.0f;
            newBox.Extents.X = Math.Abs(box.Maximum.X - newBox.Center.X);
            newBox.Extents.Y = Math.Abs(box.Maximum.Y - newBox.Center.Y);
            newBox.Extents.Z = Math.Abs(box.Maximum.Z - newBox.Center.Z);

            return newBox;
        }

        private SharpDX.BoundingBox GetBoundingBox(Mesh sceneMesh)
        {
            SharpDX.BoundingBox box = new SharpDX.BoundingBox();
            if (sceneMesh.HasVertices)
            {
                foreach (Vector3D v in sceneMesh.Vertices)
                {
                    box.Minimum.X = Math.Min(v.X, box.Minimum.X);
                    box.Minimum.Y = Math.Min(v.Y, box.Minimum.Y);
                    box.Minimum.Z = Math.Min(v.Z, box.Minimum.Z);

                    box.Maximum.X = Math.Max(v.X, box.Maximum.X);
                    box.Maximum.Y = Math.Max(v.Y, box.Maximum.Y);
                    box.Maximum.Z = Math.Max(v.Z, box.Maximum.Z);
                }
            }
            return box;
        }

        public FluxMesh LoadMesh(string filePath)
        {
            FluxMesh mesh = new FluxMesh();
            mesh.Name = Path.GetFileNameWithoutExtension(filePath);
			PostProcessSteps importFlags = 
				PostProcessSteps.Triangulate 
				| PostProcessSteps.JoinIdenticalVertices 
				| PostProcessSteps.CalculateTangentSpace 
				| PostProcessSteps.FlipUVs 
				| PostProcessSteps.GenerateSmoothNormals;

			AssimpContext context = new AssimpContext();
            Scene scene = context.ImportFile(filePath, importFlags);
            var mats = scene.Materials;

            mesh.BoundingBox = GetBoundingBox(scene);

            foreach (Mesh sceneMesh in scene.Meshes)
            {
                SubMesh subMesh = new SubMesh();
                subMesh.MaterialID = sceneMesh.MaterialIndex;

                if (sceneMesh.HasVertices)
					subMesh.Positions = sceneMesh.Vertices;
                if (sceneMesh.HasNormals)
					subMesh.Normals = sceneMesh.Normals;
                if (sceneMesh.HasTangentBasis)
					subMesh.Tangents = sceneMesh.Tangents;
                if (sceneMesh.HasVertexColors(0))
					subMesh.VertexColors = sceneMesh.VertexColorChannels[0];
				if (sceneMesh.HasTextureCoords(0))
					subMesh.TexCoords = sceneMesh.TextureCoordinateChannels[0].ToTexCoord();
                if (sceneMesh.HasFaces)
					subMesh.Indices = new List<int>(sceneMesh.GetIndices());

                bool merged = false;
                foreach (SubMesh subM in mesh.Meshes)
                {
                    if (subM.TryMerge(subMesh))
                    {
                        merged = true;
                        break;
                    }
                }
                if(merged == false)
                    mesh.Meshes.Add(subMesh);
            }

            DebugLog.Log($"Imported mesh '{mesh.Name}'", "Mesh Formatter");
            return mesh;
        }

        public void ExportMeshesAsync(MeshConvertRequest request)
        {
            int count = request.MeshQueue.Count;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += MeshWriter_DoWork;
            worker.WorkerReportsProgress = true;

            ExportingDialogViewModel viewModel = new ExportingDialogViewModel();
            ExportingDialog exportingDialog = new ExportingDialog();
            exportingDialog.DataContext = viewModel;
            exportingDialog.ShowInTaskbar = false;
            exportingDialog.Show();

            worker.ProgressChanged += viewModel.OnWorkerOnProgressChanged;
            worker.RunWorkerCompleted += (sender, args) =>
            {
                exportingDialog.Close();
                DebugLog.Log($"Exported {count} meshes", "Mesh Formatter");
            };
            worker.RunWorkerAsync(request);
        }

        private void MeshWriter_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null) return;

            MeshConvertRequest request = e.Argument as MeshConvertRequest;
            if (request == null) return;

            int meshCount = request.MeshQueue.Count;
            const int stageCount = 4;
            int progressIncrement = 100 / meshCount / stageCount;
            int progress = 0;
            for (int i = 0; i < meshCount; i++)
            {
                FluxMesh mesh = request.MeshQueue.Dequeue();

                string filePath = $"{request.SaveDirectory}\\{mesh.Name}";
                FileStream stream = File.Create($"{filePath}.flux");

                worker.ReportProgress(progress, $"'{mesh.Name}' Writing mesh data...");
                WriteMesh(mesh, stream);
                progress += progressIncrement;
                stream.Close();
                if (mesh.CookConvexMesh) 
                {
                    stream = File.Create($"{filePath}_convex.collision");
                    worker.ReportProgress(progress, $"'{mesh.Name}' Cooking convex mesh...");
					WriteConvexMeshData(mesh, stream);
                    progress += progressIncrement;
                    stream.Close();
                }
				if (mesh.CookTriangleMesh)
				{
					stream = File.Create($"{filePath}_triangle.collision");
					worker.ReportProgress(progress, $"'{mesh.Name}' Cooking triangle mesh...");
					WriteTriangleMeshData(mesh, stream);
					stream.Close();
				}

                progress += progressIncrement;
                DebugLog.Log($"Exported {mesh.Name}", "Mesh Formatter");
            }
        }

        private bool WriteMesh(FluxMesh mesh, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream, Encoding.Default);

            writer.Write("FLUX");
            writer.Write((char)FLUX_VERSION.MIN);
            writer.Write((char)FLUX_VERSION.MAX);

            writer.Write((float)mesh.BoundingBox.Center.X);
            writer.Write((float)mesh.BoundingBox.Center.Y);
            writer.Write((float)mesh.BoundingBox.Center.Z);
            writer.Write((float)mesh.BoundingBox.Extents.X);
            writer.Write((float)mesh.BoundingBox.Extents.Y);
            writer.Write((float)mesh.BoundingBox.Extents.Z);

            writer.Write((int)mesh.Meshes.Count);

            foreach (SubMesh subMesh in mesh.Meshes)
            {
                if (mesh.WriteIndices)
                {
                    writer.Write("INDEX");
                    writer.Write(subMesh.Indices.Count);
                    writer.Write(sizeof(int));
                    foreach (uint index in subMesh.Indices)
                        writer.Write(index);
                }

                if (mesh.WritePositions)
                {
                    writer.Write("POSITION");
                    writer.Write(subMesh.Positions.Count);
                    writer.Write(Marshal.SizeOf(typeof(Vector3D)));
                    foreach (Vector3D v in subMesh.Positions)
                        writer.Write(v);
                }

                if (mesh.WriteNormals)
                {
                    writer.Write("NORMAL");
                    writer.Write(subMesh.Normals.Count);
                    writer.Write(Marshal.SizeOf(typeof(Vector3D)));
                    foreach (Vector3D v in subMesh.Normals)
                        writer.Write(v);
                }

                if (mesh.WriteTangents)
                {
                    writer.Write("TANGENT");
                    writer.Write(subMesh.Tangents.Count);
                    writer.Write(Marshal.SizeOf(typeof(Vector3D)));
                    foreach (Vector3D v in subMesh.Tangents)
                        writer.Write(v);
                }

                if (mesh.WriteColors)
                {
                    writer.Write("COLOR");
                    writer.Write(subMesh.VertexColors.Count);
                    writer.Write(Marshal.SizeOf(typeof(Color4D)));
                    foreach (Color4D c in subMesh.VertexColors)
                        writer.Write(c);
                }

                if (mesh.WriteTexcoords)
                {
                    writer.Write("TEXCOORD");
                    writer.Write(subMesh.TexCoords.Count);
                    writer.Write(Marshal.SizeOf(typeof(Vector2D)));
                    foreach (Vector2D v in subMesh.TexCoords)
                        writer.Write(v);
                }
                writer.Write("ENDMESH");
            }
            return true;
        }

        private bool WriteConvexMeshData(FluxMesh mesh, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream, Encoding.Default);

            try
            {
                if (mesh.ConvexMesh == null)
                    LoadConvexMeshData(ref mesh);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            writer.Write("CONVEXMESH");
            writer.Write(mesh.ConvexMesh.MeshData.Count);
            writer.Write(mesh.ConvexMesh.MeshData.ToArray());
            return true;
        }

        public void LoadConvexMeshData(ref FluxMesh mesh)
        {
            List<PxVec3> CookVertices = new List<PxVec3>();
            foreach(SubMesh subMesh in mesh.Meshes)
                CookVertices.AddRange(subMesh.Positions.ToCookerVertices());
            mesh.ConvexMesh = Cooking.CreateConvexMesh(new ConvexMeshDesc(CookVertices));
            DebugLog.Log($"Cooked convex mesh for {mesh.Name}", "Mesh Formatter");
        }

        private bool WriteTriangleMeshData(FluxMesh mesh, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream, Encoding.Default);

            try
            {
                if (mesh.TriangleMesh == null)
                    LoadTriangleMeshData(ref mesh);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            writer.Write("TRIANGLEMESH");
            writer.Write(mesh.TriangleMesh.MeshData.Count);
            writer.Write(mesh.TriangleMesh.MeshData.ToArray());
            return true;
        }

        public void LoadTriangleMeshData(ref FluxMesh mesh)
        {
            List<PxVec3> CookVertices = new List<PxVec3>();
            List<int> CookIndices = new List<int>();
            int currentIndexOffset = 0;
            foreach (SubMesh subMesh in mesh.Meshes)
            {
                CookVertices.AddRange(subMesh.Positions.ToCookerVertices());
                foreach (int index in subMesh.Indices)
                    CookIndices.Add(index + currentIndexOffset);
                currentIndexOffset += subMesh.Positions.Count;
            }
            mesh.TriangleMesh = Cooking.CreateTriangleMesh(new TriangleMeshDesc(CookVertices, CookIndices));
            DebugLog.Log($"Cooked triangle mesh for {mesh.Name}", "Mesh Formatter");
        }
    }

    public static class CookingExtensions
    {
        public static List<PxVec3> ToCookerVertices(this List<Vector3D> vertices)
        {
            List<PxVec3> output = new List<PxVec3>(vertices.Count);
            foreach (Vector3D v in vertices)
                output.Add(new PxVec3(v.X, v.Y, v.Z));
            return output;
        }
    }
}
