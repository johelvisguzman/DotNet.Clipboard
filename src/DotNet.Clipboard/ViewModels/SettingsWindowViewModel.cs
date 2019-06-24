namespace DotNet.Clipboard.ViewModels
{
    using FluentValidation;
    using Infrastructure;
    using Properties;
    using Repositories;
    using Services;
    using System;
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// This class contains properties that the view can data bind to.
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public class SettingsWindowViewModel : FormViewModelBase<SettingsWindowViewModelValidator>
    {
        #region Fields

        private string _hotKey;
        private string _dbPath;
        private string _dbSize;
        private int _maxSavedCopiesCount;

        private readonly IClipboardRepository _clipboardRepository;
        private readonly IAppSettingsService _appSettingsService;

        #endregion

        #region Properties

        public string HotKey
        {
            get { return _hotKey; }
            set { SetProperty(ref _hotKey, value); }
        }

        public string DbPath
        {
            get { return _dbPath; }
            set { SetProperty(ref _dbPath, value); }
        }

        public string DbSize
        {
            get { return _dbSize; }
            set { SetProperty(ref _dbSize, value); }
        }

        public int MaxSavedCopiesCount
        {
            get { return _maxSavedCopiesCount; }
            set { SetProperty(ref _maxSavedCopiesCount, value); }
        }

        #endregion

        #region Events

        public event EventHandler<string> HotKeyChanged;

        #endregion

        #region Constructor

        public SettingsWindowViewModel(IClipboardRepository clipboardRepository, IAppSettingsService appSettingsService)
        {
            if (clipboardRepository == null)
                throw new ArgumentNullException(nameof(clipboardRepository));

            if (appSettingsService == null)
                throw new ArgumentNullException(nameof(appSettingsService));

            _clipboardRepository = clipboardRepository;
            _appSettingsService = appSettingsService;

            HotKey = appSettingsService.HotKey;
            MaxSavedCopiesCount = appSettingsService.MaxSavedCopiesCount;

            RefreshDatabase();

            Submitted += (sender, e) =>
            {
                if (!_appSettingsService.HotKey.Equals(HotKey))
                {
                    _appSettingsService.HotKey = HotKey;

                    HotKeyChanged?.Invoke(this, HotKey);
                }

                _appSettingsService.MaxSavedCopiesCount = MaxSavedCopiesCount;
                _appSettingsService.Culture = Thread.CurrentThread.CurrentCulture.Name;
                _appSettingsService.Save();
            };
        }

        #endregion

        #region Public Methods

        public void RefreshDatabase()
        {
            DbPath = _clipboardRepository.GetDatabasePath();

            if (!string.IsNullOrEmpty(DbPath))
                DbSize = Utils.GetFileSize(DbPath);
        }

        #endregion
    }

    public class SettingsWindowViewModelValidator : AbstractValidator<SettingsWindowViewModel>
    {
        public SettingsWindowViewModelValidator()
        {
            RuleFor(x => x.MaxSavedCopiesCount)
                .GreaterThan(0)
                .WithMessage(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Settings_FieldCannotBeLessThanOne,
                    Resources.Settings_MaxSavedCopiesCountLabel));
        }
    }
}
