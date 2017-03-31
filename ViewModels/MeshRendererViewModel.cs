﻿using System.Windows.Input;
using FluxConverterTool.ImageControl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using FluxConverterTool.Helpers;
using FluxConverterTool.Models;

namespace FluxConverterTool.ViewModels
{
    public class MeshRendererViewModel : ViewModelBase
    {
        public D3DViewport Viewport { get; set; } = new D3DViewport();

        public float CameraZoom
        {
            get
            {
                if (Viewport.Context.Camera != null)
                    return Viewport.Context.Camera.Zoom;
                return 0;
            }
            set
            {
                Viewport.Context.Camera.Zoom = value;
                RaisePropertyChanged("CameraZoom");
            }
        }

        public MeshRendererViewModel()
        {
            Messenger.Default.Register<MvvmMessage>(this, OnMessageReceived);
        }

        void OnMessageReceived(MvvmMessage message)
        {
            if (message.Type == MessageType.MeshUpdate && message.Data != null)
                Viewport.MeshRenderer.SetMesh((FluxMesh)message.Data);
        }

        public RelayCommand<MouseWheelEventArgs> OnScroll =>
            new RelayCommand<MouseWheelEventArgs>(
                (args) => Viewport.Context.Camera.Zoom += (float)args.Delta / 500.0f);

        public RelayCommand<MouseEventArgs> OnMouseDown => new RelayCommand<MouseEventArgs>((args) =>
        {
            if (args.LeftButton == MouseButtonState.Pressed)
                Viewport.Context.Camera.LeftMouseDown = true;
            if (args.MiddleButton == MouseButtonState.Pressed)
                Viewport.Context.Camera.MiddleMouseDown = true;
        });

        public RelayCommand<MouseEventArgs> OnMouseUp => new RelayCommand<MouseEventArgs>((args) =>
        {
            if (args.LeftButton == MouseButtonState.Released)
                Viewport.Context.Camera.LeftMouseDown = false;
            if (args.MiddleButton == MouseButtonState.Released)
                Viewport.Context.Camera.MiddleMouseDown = false;
        });

        public RelayCommand OnMouseLeave => new RelayCommand(() =>
        {
            Viewport.Context.Camera.LeftMouseDown = false;
            Viewport.Context.Camera.MiddleMouseDown = false;
        });

        public RelayCommand ZoomInCommand => new RelayCommand(() => Viewport.Context.Camera.Zoom += 0.2f);
        public RelayCommand ZoomOutCommand => new RelayCommand(() => Viewport.Context.Camera.Zoom -= 0.2f);
        public RelayCommand ResetCommand => new RelayCommand(() => Viewport.Context.Camera.Reset());
    }
}