using BitProtector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitProtector.MVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand PDFProtectorViewCommand { get; set; }
        public RelayCommand ZIPProtectorViewCommand { get; set; }
        public RelayCommand OfficeProtectorViewCommand { get; set; }
        public RelayCommand FileProtectorViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        public PDFProtectorViewModel PDFProtectorVM { get; set; }
        public ZIPProtectorViewModel ZIPProtectorVM { get; set; }
        public OfficeProtectorViewModel OfficeProtectorVM { get; set; }
        public FileProtectorViewModel FileProtectorVM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            HomeVM = new HomeViewModel();
            PDFProtectorVM = new PDFProtectorViewModel();
            ZIPProtectorVM = new ZIPProtectorViewModel();
            OfficeProtectorVM = new OfficeProtectorViewModel();
            FileProtectorVM = new FileProtectorViewModel();

            CurrentView = HomeVM;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVM;
            });
            PDFProtectorViewCommand = new RelayCommand(o =>
            {
                CurrentView = PDFProtectorVM;
            });
            ZIPProtectorViewCommand = new RelayCommand(o =>
            {
                CurrentView = ZIPProtectorVM;
            });
            OfficeProtectorViewCommand = new RelayCommand(o =>
            {
                CurrentView = OfficeProtectorVM;
            });
            FileProtectorViewCommand = new RelayCommand(o =>
            {
                CurrentView = FileProtectorVM;
            });
        }
    }
}
