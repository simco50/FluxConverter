using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using Assimp;
using FluxConverterTool.Helpers;
using FluxConverterTool.ViewModels;
using FluxConverterTool.Views;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PhysxNet;
using SharpDX;
using System;

namespace FluxConverterTool.Models
{
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

            foreach (Mesh m in scene.Meshes)
            {
                for (int i = 0; i < m.VertexCount; i++)
                {
                    if (m.HasVertices)
                        mesh.Positions.Add(m.Vertices[i].ToVector3());
                    if (m.HasNormals)
                        mesh.Normals.Add(m.Normals[i].ToVector3());
                    if (m.HasTangentBasis)
                        mesh.Tangents.Add(m.Tangents[i].ToVector3());
                    if (m.HasTextureCoords(0))
                    {
                        Vector3 texCoord = m.TextureCoordinateChannels[0][i].ToVector3();
                        mesh.UVs.Add(new Vector2(texCoord.X, texCoord.Y));
                    }
                    if (m.HasVertexColors(0))
                        mesh.VertexColors.Add(m.VertexColorChannels[0][i].ToColor());
                }
                if (m.HasFaces)
                {
                    foreach (int index in m.GetIndices())
                        mesh.Indices.Add(index);
                }
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
            exportingDialog.ShowCloseButton = false;
            exportingDialog.ShowMaxRestoreButton = false;
            exportingDialog.ShowMinButton = false;
            exportingDialog.ShowDialogsOverTitleBar = true;
            exportingDialog.ShowInTaskbar = false;
            exportingDialog.Show();

            worker.ProgressChanged += viewModel.OnWorkerOnProgressChanged;
            worker.RunWorkerCompleted += (sender, args) =>
            {
                exportingDialog.Close();
                if(count > 1)
                    ((MetroWindow) Application.Current.MainWindow).ShowMessageAsync("Export", $"Exported {count} mesh(es) successfully");
                DebugLog.Log($"Exported {count}", "Mesh Formatter");

            };
            worker.RunWorkerAsync(request);
        }

        private void MeshWriter_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null)
                return;
            MeshConvertRequest request = e.Argument as MeshConvertRequest;
            if (request == null)
                return;

            int meshCount = request.MeshQueue.Count;
            int progressIncrement = 100 / meshCount / 3;
            int progress = 0;
            for (int i = 0; i < meshCount; i++)
            {
                FluxMesh mesh = request.MeshQueue.Dequeue();

                string filePath = $"{request.SaveDirectory}\\{mesh.Name}.flux";
                FileStream stream = File.Create(filePath);

                worker.ReportProgress(progress, $"'{mesh.Name}' Writing mesh data...");
                WriteMesh(mesh, stream);
                progress += progressIncrement;

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
                progress += progressIncrement;
                stream.Close();
                DebugLog.Log($"Exported {mesh.Name}", "Mesh Formatter");
            }
        }

        private bool WriteMesh(FluxMesh mesh, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream, Encoding.Default);
            
                writer.Write("FLUX");
                writer.Write((char)0);
                writer.Write((char)1);

                if (mesh.WriteIndices)
                {
                    writer.Write("INDEX");
                    writer.Write(mesh.Indices.Count);
                    foreach (uint index in mesh.Indices)
                        writer.Write(index);
                }

                if (mesh.WritePositions)
                {
                    writer.Write("POSITION");
                    writer.Write(mesh.Positions.Count);
                    foreach (Vector3 v in mesh.Positions)
                        writer.Write(v);
                }

                if (mesh.WriteNormals)
                {
                    writer.Write("NORMAL");
                    writer.Write(mesh.Normals.Count);
                    foreach (Vector3 v in mesh.Normals)
                        writer.Write(v);
                }

                if (mesh.WriteTangents)
                {
                    writer.Write("TANGENT");
                    writer.Write(mesh.Tangents.Count);
                    foreach (Vector3 v in mesh.Tangents)
                        writer.Write(v);
                }

                if (mesh.WriteColors)
                {
                    writer.Write("COLOR");
                    writer.Write(mesh.VertexColors.Count);
                    foreach (Color c in mesh.VertexColors)
                        writer.Write(c);
                }

                if (mesh.WriteTexcoords)
                {
                    writer.Write("TEXCOORD");
                    writer.Write(mesh.UVs.Count);
                    foreach (Vector2 v in mesh.UVs)
                        writer.Write(v);
                }
            return true;
        }

        private bool WriteConvexMeshData(FluxMesh mesh, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream, Encoding.Default);

            try
            {
                if (mesh.ConvexMesh == null)
                {
                    mesh.ConvexMesh =
                        Cooking.CreateConvexMesh(new ConvexMeshDesc(mesh.Positions.ToCookerVertices(), mesh.Indices));
                    DebugLog.Log($"Cooked convex mesh for {mesh.Name}", "Mesh Formatter");
                }

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

        private bool WriteTriangleMeshData(FluxMesh mesh, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream, Encoding.Default);

            try
            {
                if (mesh.TriangleMesh == null)
                {
                    mesh.TriangleMesh =
                        Cooking.CreateTriangleMesh(new TriangleMeshDesc(mesh.Positions.ToCookerVertices(), mesh.Indices));
                    DebugLog.Log($"Cooked triangle mesh for {mesh.Name}", "Mesh Formatter");
                }
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

    }

    public static class CookingExtensions
    {
        public static List<PxVec3> ToCookerVertices(this List<Vector3> vertices)
        {
            List<PxVec3> output = new List<PxVec3>(vertices.Count);
            foreach(Vector3 v in vertices)
                output.Add(new PxVec3(v.X, v.Y, v.Z));
            return output;
        }
    }
}
