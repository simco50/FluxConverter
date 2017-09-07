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
        MIN = 5,
        MAX = 5,
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

        public FluxMesh LoadMesh(string filePath)
        {
            FluxMesh mesh = new FluxMesh();
            mesh.Name = Path.GetFileNameWithoutExtension(filePath);
            AssimpContext context = new AssimpContext();
            Scene scene = context.ImportFile(filePath, PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.CalculateTangentSpace | PostProcessSteps.FlipUVs);

            Mesh m = scene.Meshes[0];

            for (int i = 0; i < m.VertexCount; i++)
            {
                if (m.HasVertices)
                    mesh.Positions.Add(m.Vertices[i]);
                if (m.HasNormals)
                    mesh.Normals.Add(m.Normals[i]);
                if (m.HasTangentBasis)
                    mesh.Tangents.Add(m.Tangents[i]);
                if (m.HasTextureCoords(0))
                {
                    Vector3D texCoord = m.TextureCoordinateChannels[0][i];
                    mesh.TexCoords.Add(new Vector2D(texCoord.X, texCoord.Y));
                }
                if (m.HasVertexColors(0))
                    mesh.VertexColors.Add(m.VertexColorChannels[0][i]);
            }
            if (m.HasFaces)
            {
                foreach (int index in m.GetIndices())
                    mesh.Indices.Add(index);
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
            const int stageCount = 3;
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
                if (mesh.CookConvexMesh || mesh.CookTriangleMesh)
                {
                    stream = File.Create($"{filePath}.collision");
                    if (mesh.CookConvexMesh)
                    {
                        worker.ReportProgress(progress, $"'{mesh.Name}' Cooking convex mesh...");
                        WriteConvexMeshData(mesh, stream);
                    }
                    progress += progressIncrement;

                    if (mesh.CookTriangleMesh)
                    {
                        worker.ReportProgress(progress, $"'{mesh.Name}' Cooking triangle mesh...");
                        WriteTriangleMeshData(mesh, stream);
                    }
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

            if (mesh.WriteIndices)
            {
                writer.Write("INDEX");
                writer.Write(mesh.Indices.Count);
                writer.Write(sizeof(int));
                foreach (uint index in mesh.Indices)
                    writer.Write(index);
            }

            if (mesh.WritePositions)
            {
                writer.Write("POSITION");
                writer.Write(mesh.Positions.Count);
                writer.Write(Marshal.SizeOf(typeof(Vector3D)));
                foreach (Vector3D v in mesh.Positions)
                    writer.Write(v);
            }

            if (mesh.WriteNormals)
            {
                writer.Write("NORMAL");
                writer.Write(mesh.Normals.Count);
                writer.Write(Marshal.SizeOf(typeof(Vector3D)));
                foreach (Vector3D v in mesh.Normals)
                    writer.Write(v);
            }

            if (mesh.WriteTangents)
            {
                writer.Write("TANGENT");
                writer.Write(mesh.Tangents.Count);
                writer.Write(Marshal.SizeOf(typeof(Vector3D)));
                foreach (Vector3D v in mesh.Tangents)
                    writer.Write(v);
            }

            if (mesh.WriteColors)
            {
                writer.Write("COLOR");
                writer.Write(mesh.VertexColors.Count);
                writer.Write(Marshal.SizeOf(typeof(Color4D)));
                foreach (Color4D c in mesh.VertexColors)
                    writer.Write(c);
            }

            if (mesh.WriteTexcoords)
            {
                writer.Write("TEXCOORD");
                writer.Write(mesh.TexCoords.Count);
                writer.Write(Marshal.SizeOf(typeof(Vector2D)));
                foreach (Vector2D v in mesh.TexCoords)
                    writer.Write(v);
            }
            writer.Write("END");
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
            mesh.ConvexMesh = Cooking.CreateConvexMesh(new ConvexMeshDesc(mesh.Positions.ToCookerVertices()));
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
            mesh.TriangleMesh = Cooking.CreateTriangleMesh(new TriangleMeshDesc(mesh.Positions.ToCookerVertices(), mesh.Indices));
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
