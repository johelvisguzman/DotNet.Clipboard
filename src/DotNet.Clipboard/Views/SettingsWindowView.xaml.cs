namespace DotNet.Clipboard.Views
{
    using DotNetToolkit.Repository;
    using Services;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Input;
    using ViewModels;
    using WPFLocalizeExtension.Engine;

    /// <summary>
    /// Interaction logic for SettingsWindowView.xaml
    /// </summary>
    public partial class SettingsWindowView
    {
        public SettingsWindowView()
        {
            Loaded += OnLoaded;
            PreviewKeyDown += new KeyEventHandler(OnPreviewKeyDown);

            InitializeComponent();
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (SettingsWindowViewModel)DataContext;

            viewModel.Closed += (s, args) => Close();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Closes the window
            if (e.Key == Key.Escape)
                Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var appSettingsService = RepositoryDependencyResolver.Current.Resolve<IAppSettingsService>();

            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture
                = Thread.CurrentThread.CurrentUICulture
                    = new CultureInfo(appSettingsService.Culture);

            base.OnClosing(e);
        }
    }
}
