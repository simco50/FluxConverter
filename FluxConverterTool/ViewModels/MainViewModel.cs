using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using FluxConverterTool.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using FluxConverterTool.Helpers;
using FluxConverterTool.Views;
using DebugLogView = FluxConverterTool.Views.DebugLogView;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace FluxConverterTool.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region VARIABLES
        private readonly MeshFormatter _formatter;

        public ObservableCollection<FluxMesh> Meshes { get; set; } = new ObservableCollection<FluxMesh>();

        private List<FluxMesh> _selectedMeshes = new List<FluxMesh>();

        #endregion

        #region PROPERTIES

        public int TotalVertexCount => _selectedMeshes.Sum(mesh => mesh.Positions.Count);
        public int TotalTriangleCount => _selectedMeshes.Sum(mesh => mesh.Indices.Count / 3);
        public int SelectedMeshCount => _selectedMeshes.Count;

        public bool IsSingleSelected => _selectedMeshes.Count == 1;
        public bool EnableLoadDiffuseTextureButton => IsSingleSelected && _selectedMeshes[0].UVs.Count > 0;
        public bool EnableLoadNormalTextureButton => IsSingleSelected && _selectedMeshes[0].Normals.Count > 0 && _selectedMeshes[0].Tangents.Count > 0;
        public bool EnableAnimationSection => IsSingleSelected && _selectedMeshes[0].HasAnimations;

        public bool EnableRemoveOrSaveButton => _selectedMeshes.Count != 0;
        public bool HasSelection => _selectedMeshes.Count > 0;
        public bool EnableSaveAllButton => Meshes.Count > 0;

        public bool EnablePositions => false;
        public bool EnableIndices => false;
        public bool EnableNormals => _selectedMeshes.Count(mesh => mesh.Normals.Count > 0) == _selectedMeshes.Count;
        public bool EnableTangents => _selectedMeshes.Count(mesh => mesh.Tangents.Count > 0) == _selectedMeshes.Count;
        public bool EnableUVs => _selectedMeshes.Count(mesh => mesh.UVs.Count > 0) == _selectedMeshes.Count;
        public bool EnableColors => _selectedMeshes.Count(mesh => mesh.VertexColors.Count > 0) == _selectedMeshes.Count;

        public bool WritePositions
        {
            get
            {
                return _selectedMeshes.Count != 0 && _selectedMeshes.Count(mesh => mesh.WritePositions) == _selectedMeshes.Count;
            }
            set
            {
                foreach (FluxMesh mesh in _selectedMeshes)
                    mesh.WritePositions = value;
                RaisePropertyChanged("WritePositions");
            }
        }
        public bool WriteIndices
        {
            get
            {
                return _selectedMeshes.Count != 0 && _selectedMeshes.Count(mesh => mesh.WriteIndices) == _selectedMeshes.Count;
            }
            set
            {
                foreach (FluxMesh mesh in _selectedMeshes)
                    mesh.WriteIndices = value;
                RaisePropertyChanged("WriteIndices");
            }
        }
        public bool WriteNormals
        {
            get
            {
                return _selectedMeshes.Count != 0 && _selectedMeshes.Count(mesh => mesh.WriteNormals) == _selectedMeshes.Count;
            }
            set
            {
                foreach (FluxMesh mesh in _selectedMeshes)
                    mesh.WriteNormals = value;
                RaisePropertyChanged("WriteNormals");
            }
        }
        public bool WriteTangents
        {
            get
            {
                return _selectedMeshes.Count != 0 && _selectedMeshes.Count(mesh => mesh.WriteTangents) == _selectedMeshes.Count;
            }
            set
            {
                foreach (FluxMesh mesh in _selectedMeshes)
                    mesh.WriteTangents = value;
                RaisePropertyChanged("WriteTangents");
            }
        }
        public bool WriteTexcoords
        {
            get
            {
                return _selectedMeshes.Count != 0 && _selectedMeshes.Count(mesh => mesh.WriteTexcoords) == _selectedMeshes.Count;
            }
            set
            {
                foreach (FluxMesh mesh in _selectedMeshes)
                    mesh.WriteTexcoords = value;
                RaisePropertyChanged("WriteTexcoords");
            }
        }
        public bool WriteColors
        {
            get
            {
                return _selectedMeshes.Count != 0 && _selectedMeshes.Count(mesh => mesh.WriteColors) == _selectedMeshes.Count;
            }
            set
            {
                foreach (FluxMesh mesh in _selectedMeshes)
                    mesh.WriteColors = value;
                RaisePropertyChanged("WriteColors");
            }
        }

        public bool CookTriangleMesh
        {
            get { return _selectedMeshes.Count != 0 && _selectedMeshes.Count(mesh => mesh.CookTriangleMesh) == _selectedMeshes.Count; }
            set
            {
                foreach (FluxMesh mesh in _selectedMeshes)
                    mesh.CookTriangleMesh = value;
                RaisePropertyChanged("CookTriangleMesh");
            }
        }

        public bool CookConvexMesh
        {
            get { return _selectedMeshes.Count != 0 && _selectedMeshes.Count(mesh => mesh.CookConvexMesh) == _selectedMeshes.Count; }
            set
            {
                foreach (FluxMesh mesh in _selectedMeshes)
                    mesh.CookConvexMesh = value;
                RaisePropertyChanged("CookConvexMesh");
            }
        }

        #endregion

        #region METHODS

        public MainViewModel()
        {
            FileLogger fileLogger = new FileLogger("ApplicationLog.log");
            DebugLog.LogEvent += fileLogger.LogInfo;
            ApplicationLogger appLogger = new ApplicationLogger();
            DebugLog.LogEvent += appLogger.LogInfo;

            Meshes.CollectionChanged += Meshes_CollectionChanged;
            _formatter = new MeshFormatter();
            _formatter.Initialize();

            DebugLog.Log("Initialized", "Application");
        }

        ~MainViewModel()
        {
            _formatter.Shutdown();

            DebugLog.Log("Shutdown", "Application");
        }

        private void Meshes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("EnableSaveAllButton");
        }

        #endregion

        #region COMMANDS

        public RelayCommand<IList> OnSelectionChangedCommand => new RelayCommand<IList>(OnSelectionChanged);

        private void OnSelectionChanged(IList list)
        {
            _selectedMeshes.Clear();
            foreach (object o in list)
                _selectedMeshes.Add(o as FluxMesh);

            ProperyChanged();
            Messenger.Default.Send<MvvmMessage, MeshRendererViewModel>(new MvvmMessage(MessageType.MeshUpdate, _selectedMeshes.Count > 0 ? _selectedMeshes[0] : null));
            Messenger.Default.Send<MvvmMessage, MeshRendererViewModel>(new MvvmMessage(MessageType.PhysicsMeshUpdate, null));
        }

        private void ProperyChanged()
        {
            RaisePropertyChanged("WritePositions");
            RaisePropertyChanged("WriteIndices");
            RaisePropertyChanged("WriteNormals");
            RaisePropertyChanged("WriteTangents");
            RaisePropertyChanged("WriteColors");
            RaisePropertyChanged("WriteTexcoords");

            RaisePropertyChanged("EnablePositions");
            RaisePropertyChanged("EnableIndices");
            RaisePropertyChanged("EnableNormals");
            RaisePropertyChanged("EnableTangents");
            RaisePropertyChanged("EnableUVs");
            RaisePropertyChanged("EnableColors");

            RaisePropertyChanged("CookTriangleMesh");
            RaisePropertyChanged("CookConvexMesh");

            RaisePropertyChanged("IsSingleSelected");
            RaisePropertyChanged("EnableSaveAllButton");
            RaisePropertyChanged("HasSelection");
            RaisePropertyChanged("EnableRemoveOrSaveButton");

            RaisePropertyChanged("TotalVertexCount");
            RaisePropertyChanged("TotalTriangleCount");
            RaisePropertyChanged("SelectedMeshCount");

            RaisePropertyChanged("EnableLoadNormalTextureButton");
            RaisePropertyChanged("EnableLoadDiffuseTextureButton");

            RaisePropertyChanged("EnableAnimationSection");
        }

        public RelayCommand ImportMeshCommand => new RelayCommand(ImportMeshes);
        public RelayCommand SaveSelectedMeshCommand => new RelayCommand(SaveSelected);
        public RelayCommand SaveAllMeshesCommand => new RelayCommand(SaveAll);
        public RelayCommand RemoveSelectedCommand => new RelayCommand(RemoveSelected);

        void RemoveSelected()
        {
            while(_selectedMeshes.Count > 0)
                Meshes.Remove(_selectedMeshes[0]);
            _selectedMeshes.Clear();
            Messenger.Default.Send<MvvmMessage, MeshRendererViewModel>(new MvvmMessage(MessageType.MeshUpdate, null));
            ProperyChanged();

            DebugLog.Log("Removed meshes", "Application");
        }

        void ImportMeshes()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Meshes (*.obj;*.dae;*.3ds;*.x)|*.obj;*.dae;*.3ds;*.x";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == false)
                return;

            foreach (var filePath in dialog.FileNames)
            {
                if (Meshes.Any(a => a.Name == Path.GetFileNameWithoutExtension(filePath)))
                {
                    MessageBox.Show("Already exists!");
                    continue;
                }
                Meshes.Add(_formatter.LoadMesh(filePath));
            }
        }

        void SaveSelected()
        {
            SaveMeshes(_selectedMeshes.ToArray());
        }

        void SaveAll()
        {
            SaveMeshes(Meshes.ToArray());
        }

        void SaveMeshes(FluxMesh[] meshes)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            MeshConvertRequest request = new MeshConvertRequest();
            request.SaveDirectory = dialog.SelectedPath;
            request.MeshQueue = new Queue<FluxMesh>(meshes);
            _formatter.ExportMeshesAsync(request);
        }

        public RelayCommand LoadDiffuseTextureCommand => new RelayCommand(LoadDiffuseTexture);

        void LoadDiffuseTexture()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Images (*.jpg;*.png;*.dds)|*.jpg;*.png;*.dds|All files (*.*)|*.*";
            if (dialog.ShowDialog() == false)
                return;
            Messenger.Default.Send<MvvmMessage>(new MvvmMessage(MessageType.MeshLoadDiffuseTexture, dialog.FileName));
        }

        public RelayCommand LoadNormalextureCommand => new RelayCommand(LoadNormalexture);
        void LoadNormalexture()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Images (*.jpg;*.png;*.dds)|*.jpg;*.png;*.dds|All files (*.*)|*.*";
            if (dialog.ShowDialog() == false)
                return;
            Messenger.Default.Send<MvvmMessage>(new MvvmMessage(MessageType.MeshLoadNormalTexture, dialog.FileName));
        }

        public RelayCommand ShowDebugLogCommand => new RelayCommand(ShowDebugLog);

        private void ShowDebugLog()
        {
            DebugLogView view = new DebugLogView();
            view.ShowDialog();
        }
        public RelayCommand ShowHelpWindowCommand => new RelayCommand(ShowHelpWindow);

        private void ShowHelpWindow()
        {
            HelpWindowView view = new HelpWindowView();
            view.ShowDialog();
        }

        public RelayCommand ShowConvexMeshCommand => new RelayCommand(ShowConvexMesh);

        private void ShowConvexMesh()
        {
            FluxMesh mesh = _selectedMeshes[0];
            if (mesh.ConvexMesh == null)
                _formatter.LoadConvexMeshData(ref mesh);
            Messenger.Default.Send<MvvmMessage>(new MvvmMessage(MessageType.PhysicsMeshUpdate, mesh.ConvexMesh));
        }

        public RelayCommand ShowTriangleMeshCommand => new RelayCommand(ShowTriangleMesh);

        private void ShowTriangleMesh()
        {
            FluxMesh mesh = _selectedMeshes[0];
            if(mesh.TriangleMesh == null)
                _formatter.LoadTriangleMeshData(ref mesh);
            Messenger.Default.Send<MvvmMessage>(new MvvmMessage(MessageType.PhysicsMeshUpdate, mesh.TriangleMesh));
        }

        #endregion
    }
}