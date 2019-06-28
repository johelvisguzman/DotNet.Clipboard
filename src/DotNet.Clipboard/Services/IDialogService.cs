namespace DotNet.Clipboard.Services
{
    using DotNetToolkit.Wpf.Mvvm;

    /// <summary>
    /// Represents a dialog service.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a dialog for the specified view model type.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type to show the dialog for.</typeparam>
        void Show<TViewModel>() where TViewModel : ViewModelBase;

        /// <summary>
        /// Shows a dialog for the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model to show the dialog for.</param>
        void Show(ViewModelBase viewModel);
    }
}
