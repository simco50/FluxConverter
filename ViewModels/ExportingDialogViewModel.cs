using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace FluxConverterTool.ViewModels
{
    public class ExportingDialogViewModel : ViewModelBase
    {
        private int _progress = 0;

        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged("Progress");
            }
        }

        private string _message = "Initializing export...";

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                RaisePropertyChanged("Message");
            }
        }

        private bool _enableOkButton = false;
        public bool EnableOkButton {
            get { return _enableOkButton; }
            set
            {
                _enableOkButton = true;
                RaisePropertyChanged("EnableOkButton");
            } }

        public void OnWorkerOnProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            Progress = args.ProgressPercentage;
            if(args.UserState != null)
                Message = args.UserState.ToString();
        }
    }
}
