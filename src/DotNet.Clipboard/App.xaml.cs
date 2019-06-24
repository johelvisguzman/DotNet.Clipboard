namespace DotNet.Clipboard
{
    using DotNetToolkit.Repository.Extensions.Unity;
    using DotNetToolkit.Wpf.Mvvm;
    using Services;
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Windows;
    using System.Windows.Markup;
    using Unity;
    using ViewModels;
    using WPFLocalizeExtension.Engine;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ConfigureConnectionString();
            ConfigureContainer();
        }

        private static void ConfigureContainer()
        {
            var container = new UnityContainer();

            container.RegisterType<MainWindowViewModel>();
            container.RegisterType<SettingsWindowViewModel>();
            container.RegisterInstance<IAppSettingsService>(AppSettingsService.Default);
            container.RegisterInstance<IDialogService>(DialogService.Instance);
            container.RegisterRepositories(options => options.UseConfiguration());

            ViewModelLocator.SetDefaultViewModelFactory(type => container.Resolve(type));
        }

        private static void ConfigureConnectionString()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Infrastructure.Utils.GetAppDataDirectory());
        }

        private static void InitialiseCultures()
        {
            var appSettingsService = AppSettingsService.Default;

            if (!string.IsNullOrEmpty(appSettingsService.Culture))
            {
                LocalizeDictionary.Instance.Culture
                    = Thread.CurrentThread.CurrentUICulture
                    = new CultureInfo(appSettingsService.Culture);
            }

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement), 
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitialiseCultures();

            new MainWindowView().Show();
        }
    }
}
