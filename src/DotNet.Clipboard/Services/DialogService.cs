namespace DotNet.Clipboard.Services
{
    using DotNetToolkit.Wpf.Mvvm;
    using System;
    using System.Windows;

    public class DialogService : IDialogService
    {
        private static volatile DialogService _instance;
        private static readonly object _syncRoot = new object();

        public static DialogService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new DialogService();
                    }
                }

                return _instance;
            }
        }

        public void Show<TViewModel>() where TViewModel : ViewModelBase
        {
            var view = ViewLocator.LocateFor<TViewModel>();
            var window = view as Window;

            if (window != null)
            {
                window.ShowDialog();
            }
        }

        public void Show(ViewModelBase viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            var view = ViewLocator.LocateFor(viewModel);
            var window = view as Window;

            if (window != null)
            {
                window.ShowDialog();
            }
        }
    }
}
