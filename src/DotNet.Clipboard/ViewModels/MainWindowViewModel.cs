namespace DotNet.Clipboard.ViewModels
{
    using DotNetToolkit.Wpf.Commands;
    using DotNetToolkit.Wpf.Mvvm;
    using Extensions;
    using Infrastructure;
    using Repositories;
    using Services;
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// This class contains properties that the view can data bind to.
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private string _search;
        private ObservableCollection<ClipViewModel> _clips = new ObservableCollection<ClipViewModel>();
        private ICollectionView _clipsCollectionView;
        private bool _isClipboardMonitorDisconnected;
        private SettingsWindowViewModel _settingsViewModel;

        private readonly IClipboardRepository _clipboardRepository;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IDialogService _dialogService;

        #endregion

        #region Properties

        public string Search
        {
            get { return _search; }
            set
            {
                SetProperty(ref _search, value);
                _clipsCollectionView.Refresh();
            }
        }

        public ICollectionView Clips
        {
            get { return _clipsCollectionView; }
            private set { SetProperty(ref _clipsCollectionView, value); }
        }

        public SettingsWindowViewModel Settings { get { return _settingsViewModel; } }

        #endregion

        #region Commands

        public RelayCommand LaunchSettingsCommand { get; set; }
        public RelayCommand<ClipViewModel> AddCommand { get; set; }
        public RelayCommand<ClipViewModel> SelectCommand { get; set; }
        public RelayCommand<IList> DeleteCommand { get; set; }

        #endregion

        #region Events

        public event EventHandler<ClipViewModel> ClipSelected;
        public event EventHandler MinimizeWindowRequested;

        #endregion

        #region Constructors

        public MainWindowViewModel(IClipboardRepository clipboardRepository, IAppSettingsService appSettingsService, IDialogService dialogService, SettingsWindowViewModel settingsViewModel)
        {
            if (clipboardRepository == null)
                throw new ArgumentNullException(nameof(clipboardRepository));

            if (appSettingsService == null)
                throw new ArgumentNullException(nameof(appSettingsService));

            if (dialogService == null)
                throw new ArgumentNullException(nameof(dialogService));

            if (settingsViewModel == null)
                throw new ArgumentNullException(nameof(settingsViewModel));

            _dialogService = dialogService;
            _clipboardRepository = clipboardRepository;
            _appSettingsService = appSettingsService;
            _settingsViewModel = settingsViewModel;

            LaunchSettingsCommand = new RelayCommand(OnLaunchSettings);
            AddCommand = new RelayCommand<ClipViewModel>(OnAdd);
            SelectCommand = new RelayCommand<ClipViewModel>(OnSelect);
            DeleteCommand = new RelayCommand<IList>(OnDelete);
        }

        #endregion

        #region Public Methods

        public void OnClosing()
        {
            _settingsViewModel.CloseCommand.Execute(null);
        }

        #endregion

        #region Private Methods

        private bool OnClipFilter(object o)
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            var clip = (ClipViewModel)o;

            if (clip.Format == DataFormats.Text)
                return ((string)clip.Data).Contains(Search, StringComparison.InvariantCultureIgnoreCase);

            if (clip.Format == DataFormats.Bitmap)
                return DataFormats.Bitmap.Contains(Search, StringComparison.InvariantCultureIgnoreCase);

            return true;
        }

        private void OnLaunchSettings()
        {
            MinimizeWindowRequested?.Invoke(this, EventArgs.Empty);

            _dialogService.Show(_settingsViewModel);
        }

        private async void OnAdd(ClipViewModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var count = await _clipboardRepository.CountAsync();

            if (_appSettingsService.MaxSavedCopiesCount > 0 && (count >= _appSettingsService.MaxSavedCopiesCount))
                return;

            var isDuplicate = _clips.Where(x => x.Format == model.Format).Any(x =>
            {
                if (x.Format == DataFormats.Text)
                    return ((string)model.Data).Equals((string)x.Data);

                if (x.Format == DataFormats.Bitmap)
                    return Utils.AreEqual((BitmapSource)model.Data, (BitmapSource)x.Data);

                return true;
            });

            if (isDuplicate)
                return;

            var clip = AppAutoMapper.Map(model);

            await _clipboardRepository.AddAsync(clip);
            model.Id = clip.Id;

            _clips.Insert(0, model);

            Refresh();
        }

        private async void OnSelect(ClipViewModel model)
        {
            if (model == null)
                return;

            model.LastUsedDate = DateTime.Now;

            var clipInDb = await _clipboardRepository.FindAsync(model.Id);

            AppAutoMapper.Map(model, clipInDb);

            await _clipboardRepository.UpdateAsync(clipInDb);

            // Moves the new clip to the top of the list and resets the list to re-arrange the index for each item
            _clips.Move(_clips.IndexOf(model), 0);

            Refresh();

            ClipSelected?.Invoke(this, model);
        }

        private async void OnDelete(IList items)
        {
            if (items == null)
                return;

            var models = items.Cast<ClipViewModel>().ToList();

            if (!models.Any())
                return;

            var clipsIdsToDelete = models.Select(x => x.Id);

            await _clipboardRepository.DeleteAsync(x => clipsIdsToDelete.Contains(x.Id));

            _clips.RemoveRange(models);

            Refresh();
        }

        private void Refresh()
        {
            var tempClips = _clips.ToList();
            _clips.Clear();
            _clips.AddRange(tempClips);
        }

        #endregion

        #region Overrides of ViewModelBase

        protected override async void OnInitialize()
        {
            var clipsInDb = await _clipboardRepository.FindAllAsync();
            var clips = new ObservableCollection<ClipViewModel>();

            foreach (var clip in clipsInDb)
            {
                var model = AppAutoMapper.Map(clip);

                clips.Add(model);
            }

            _clips = clips;

            Clips = CollectionViewSource.GetDefaultView(_clips);
            Clips.Filter += OnClipFilter;

            base.OnInitialize();
        }

        #endregion
    }
}
