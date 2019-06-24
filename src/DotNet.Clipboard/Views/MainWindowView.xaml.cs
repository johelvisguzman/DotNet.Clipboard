namespace DotNet.Clipboard
{
    using DotNetToolkit.Repository;
    using Infrastructure;
    using NHotkey;
    using NHotkey.Wpf;
    using Repositories;
    using Services;
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using ViewModels;
    using WPFLocalizeExtension.Engine;

    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView
    {
        #region Fields

        private double _height;
        private double _width;
        private Brush _glowBrush;

        private IntPtr _hWnd;

        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private System.Windows.Forms.ContextMenu _notifyIconContextMenu;
        private System.Windows.Forms.MenuItem _toggleClipboardMonitorMenuItem;
        private System.Windows.Forms.MenuItem _exitMenuItem;
        private System.Windows.Forms.MenuItem _settingsMenuItem;
        private System.Windows.Forms.MenuItem _compactDbMenuItem;
        private bool _forceClose;
        private Point _mouseStart;

        private ClipboardMonitor _clipboardMonitor;
        private bool _isClipboardMonitorDisconnected;

        private readonly IAppSettingsService _applicationService;
        private readonly IClipboardRepository _clipboardRepo;

        #endregion

        #region Constructors

        public MainWindowView()
        {
            // Hides the window at startup
            Visibility = Visibility.Hidden;

            // Events
            Loaded += OnLoaded;
            Closed += OnClosed;
            PreviewKeyDown += new KeyEventHandler(OnPreviewKeyDown);

            _applicationService = RepositoryDependencyResolver.Current.Resolve<IAppSettingsService>();
            _clipboardRepo = RepositoryDependencyResolver.Current.Resolve<IClipboardRepository>();

            // if we have a hotkey saved, then register it
            var hotkey = Utils.ConvertStringToHotKey(_applicationService.HotKey);
            if (hotkey != null)
                HotkeyManager.Current.AddOrReplace(Properties.Resources.GlobalHotKey, hotkey.Item1, hotkey.Item2, OnHotKey);

            LocalizeDictionary.Instance.PropertyChanged += (sender, e) =>
            {
                // These are properties that are updated using the LocalizationProvider in the code-behind,
                // so they need to be updated when the culture is changed
                if (e.PropertyName.EndsWith(nameof(LocalizeDictionary.Culture)))
                {
                    var viewModel = (MainWindowViewModel)DataContext;

                    _exitMenuItem.Text = LocalizationProvider.GetLocalizedValue<string>(nameof(Properties.Resources.Trayicon_ExitLabel));
                    _settingsMenuItem.Text = LocalizationProvider.GetLocalizedValue<string>(nameof(Properties.Resources.Trayicon_SettingsLabel));
                    _compactDbMenuItem.Text = LocalizationProvider.GetLocalizedValue<string>(nameof(Properties.Resources.Trayicon_CompactDbLabel));

                    UpdateLocalizedToggleClipboardResource(viewModel);
                }
            };

            InitializeComponent();

            // Saves original values to later restore
            _height = Height;
            _width = Width;
            _glowBrush = GlowBrush;
        }

        #endregion

        #region Private Methods

        private void MinimizeWindow()
        {
            // Hids the window
            Hide();
            WindowState = WindowState.Minimized;
        }

        private void RestoreWindow(bool notifyIconClick)
        {
            // Restore size
            Height = _height;
            Width = _width;

            // Checks the windows bound
            EnsureWindowIsInBound(notifyIconClick);

            // Shows the window
            Show();
            WindowState = WindowState.Normal;

            // Focuses on the window when it is restored
            Activate();
            Topmost = true;  // important
            Topmost = false; // important
            Focus();         // important

            // Focuses on the list box
            OnListBoxFocus();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainWindowViewModel)DataContext;

            viewModel.ClipSelected += (s, args) =>
            {
                // Stop listening to the clipboard so that we are raising a
                // clipboard content changed event from this application
                _clipboardMonitor.Stop();

                // Copies the data to the clipboard
                if (args.Format == DataFormats.Bitmap)
                {
                    Clipboard.SetImage((BitmapSource)args.Data);
                }
                else
                {
                    Clipboard.SetDataObject(args.Data, true);

                    MinimizeWindow();

                    // Sends a paste clipboard key to the next active application
                    System.Windows.Forms.SendKeys.SendWait("^V");
                }

                // Start listening to the clipboard again
                _clipboardMonitor.Start();
            };
            viewModel.MinimizeWindowRequested += (s, args) => { MinimizeWindow(); };
            viewModel.Settings.HotKeyChanged += (s, hotkeyString) =>
            {
                // Updates the registry
                HotkeyManager.Current.Remove(Properties.Resources.GlobalHotKey);

                var hotkey = Utils.ConvertStringToHotKey(hotkeyString);

                if (hotkey != null)
                    HotkeyManager.Current.AddOrReplace(Properties.Resources.GlobalHotKey, hotkey.Item1, hotkey.Item2, OnHotKey);
            };

            if (!_applicationService.IsClipboardMonitoring)
                ToggleClipboardMonitor(false);
            else
                UpdateLocalizedToggleClipboardResource(viewModel);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            ((MainWindowViewModel)DataContext).Settings.CloseCommand.Execute(null);
        }

        private void OnHotKey(object sender, HotkeyEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                MinimizeWindow();
            else
                RestoreWindow(false);
        }

        private void EnsureWindowIsInBound(bool notifyIconClick)
        {
            var mousePosition = Utils.GetMousePosition();
            var tuppleRect = Utils.GetScreenRectFromPoint(mousePosition);
            var screenRect = tuppleRect.Item1;
            var screenWorkingAreaRect = tuppleRect.Item2;
            var taskbarRect = Utils.GetAppTaskBarRect();
            var taskbarLeftDockedWidth = Math.Abs((Math.Abs(screenRect.Left) - Math.Abs(screenWorkingAreaRect.Left)));
            var taskbarTopDockedHeight = Math.Abs((Math.Abs(screenRect.Top) - Math.Abs(screenWorkingAreaRect.Top)));
            var taskbarRightDockedWidth = ((screenRect.Width - taskbarLeftDockedWidth) - screenWorkingAreaRect.Width);
            var taskbarBottomDockedHeight = ((screenRect.Height - taskbarTopDockedHeight) - screenWorkingAreaRect.Height);
            var taskBarIsDockedLeft = taskbarLeftDockedWidth > 0;
            var taskBarIsDockedTop = taskbarTopDockedHeight > 0;
            var taskBarIsDockedRight = taskbarRightDockedWidth > 0;
            var taskBarIsDockedBottom = taskbarBottomDockedHeight > 0;

            if (!notifyIconClick)
            {
                if (taskBarIsDockedLeft)
                {
                    if (mousePosition.Y + Height > screenWorkingAreaRect.Height)
                        Top = screenWorkingAreaRect.Height - Height;
                    else
                        Top = mousePosition.Y;

                    if (mousePosition.X < taskbarRect.Left + taskbarRect.Width)
                        Left = taskbarRect.Left + taskbarRect.Width;
                    else if (mousePosition.X + Width > screenWorkingAreaRect.Right)
                        Left = screenWorkingAreaRect.Right - Width;
                    else
                        Left = mousePosition.X;
                }
                else if (taskBarIsDockedTop)
                {
                    if (mousePosition.Y < taskbarRect.Height)
                        Top = taskbarRect.Height;
                    else if (mousePosition.Y + Height > screenWorkingAreaRect.Height + taskbarRect.Height)
                        Top = screenWorkingAreaRect.Height + taskbarRect.Height - Height;
                    else
                        Top = mousePosition.Y;

                    if (mousePosition.X + Width > screenWorkingAreaRect.Right)
                        Left = screenWorkingAreaRect.Right - Width;
                    else
                        Left = mousePosition.X;
                }
                else
                {
                    Left = mousePosition.X + Width > screenWorkingAreaRect.Right
                        ? screenWorkingAreaRect.Right - Width
                        : mousePosition.X;
                    Top = mousePosition.Y + Height > screenWorkingAreaRect.Height
                        ? screenWorkingAreaRect.Height - Height
                        : mousePosition.Y;
                }
            }
            else
            {
                if (taskBarIsDockedLeft)
                {
                    Top = screenWorkingAreaRect.Height - Height;
                    Left = taskbarRect.Left + taskbarRect.Width;
                }
                else if (taskBarIsDockedTop)
                {
                    Top = taskbarRect.Height;
                    Left = screenWorkingAreaRect.Right - Width;
                }
                else
                {
                    Top = screenWorkingAreaRect.Bottom - Height;
                    Left = screenWorkingAreaRect.Right - Width;
                }
            }
        }

        private void OnClipboardContentChanged(object sender, EventArgs eventArgs)
        {
            if (!_clipboardMonitor.TryGetData(out object data, out string format))
                return;

            var viewModel = (MainWindowViewModel)DataContext;

            viewModel.AddCommand.Execute(new ClipViewModel(data, format));
            viewModel.Settings.RefreshDatabase();
        }

        private void OnNotifyIconClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                RestoreWindow(true);
        }

        private void OnExitMenuItemClick(object sender, EventArgs e)
        {
            _forceClose = true;
            Close();
        }

        private void OnSettingsMenuItemClick(object sender, EventArgs e)
        {
            ((MainWindowViewModel)DataContext).LaunchSettingsCommand.Execute(null);
        }

        private async void OnCompactDbMenuItemClick(object sender, EventArgs e)
        {
            await _clipboardRepo.CompactDatabaseAsync();

            ((MainWindowViewModel)DataContext).Settings.RefreshDatabase();
        }

        private void OnToggleClipboardMonitorMenuItemClick(object sender, EventArgs e)
        {
            ToggleClipboardMonitor(!_clipboardMonitor.IsActive);
        }

        private void ToggleClipboardMonitor(bool activate)
        {
            var viewModel = (MainWindowViewModel)DataContext;

            _isClipboardMonitorDisconnected = !activate;

            // Starts/stops the clipboard
            if (activate)
                _clipboardMonitor.Start();
            else
                _clipboardMonitor.Stop();

            // Update the application title and color when it is active or not
            _notifyIcon.Text = viewModel.DisplayName;

            UpdateLocalizedToggleClipboardResource(viewModel);

            // Change the glow of the window based on the monitor being active or not
            Application.Current.Resources["WindowGlowBrush"] = activate 
                ? _glowBrush
                : (SolidColorBrush)Application.Current.Resources["FlatColorSunFlower"];

            // Save the active settings
            _applicationService.IsClipboardMonitoring = activate;
            _applicationService.Save();
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseStart = e.GetPosition(null);
        }

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            var mpos = e.GetPosition(null);
            var diff = _mouseStart - mpos;

            // Try to drag a clip from the list out of the application on a new created file
            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (ListBox.SelectedItems.Count == 0)
                    return;

                var files = ListBox.SelectedItems
                    .Cast<ClipViewModel>()
                    .Select(Utils.CreateFile)
                    .ToArray();

                var dataFormat = DataFormats.FileDrop;
                var dataObject = new DataObject(dataFormat, files);

                DragDrop.DoDragDrop(ListBox, dataObject, DragDropEffects.Copy);
            }
        }

        private void ListBox_MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).DeleteCommand.Execute(ListBox.SelectedItems);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Hides the window
            if (e.Key == Key.Escape)
                MinimizeWindow();

            // (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ||
                Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt) ||
                Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                return;

            var ch = (char)KeyInterop.VirtualKeyFromKey(e.Key);

            // Focus on search box if a letter is pressed
            // Otherwise; if the search has data, and a backspace is pressed, then focus as well
            if (char.IsLetter(ch) || (!string.IsNullOrEmpty(SearchTextBox.Text) && e.Key == Key.Back))
                SearchTextBox.Focus();
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter)
                OnListBoxFocus();
        }

        private void OnListBoxFocus()
        {
            ListBox.Focus();

            if (ListBox.HasItems)
            {
                if (ListBox.SelectedItem == null)
                    ListBox.SelectedIndex = 0;

                ((ListBoxItem)ListBox
                    .ItemContainerGenerator
                    .ContainerFromItem(ListBox.SelectedItem))
                    .Focus();
            }
        }

        private void UpdateLocalizedToggleClipboardResource(MainWindowViewModel viewModel)
        {
            var toggleClipboardResourceKey = _isClipboardMonitorDisconnected
                ? nameof(Properties.Resources.Trayicon_ConnectToClipboardLabel)
                : nameof(Properties.Resources.Trayicon_DisconnectFromClipboardLabel);

            _toggleClipboardMonitorMenuItem.Text = LocalizationProvider.GetLocalizedValue<string>(toggleClipboardResourceKey);

            var mainDisplayNameResourceKey = _isClipboardMonitorDisconnected
                ? nameof(Properties.Resources.WindowDisconnectedTitle)
                : nameof(Properties.Resources.WindowTitle);

            viewModel.DisplayName = LocalizationProvider.GetLocalizedValue<string>(mainDisplayNameResourceKey);

            _notifyIcon.Text = viewModel.DisplayName;

            var settingsDisplayNameResourceKey = _isClipboardMonitorDisconnected
                    ? nameof(Properties.Resources.Settings_WindowDisconnectedTitle)
                    : nameof(Properties.Resources.Settings_WindowTitle);

            viewModel.Settings.DisplayName = LocalizationProvider.GetLocalizedValue<string>(settingsDisplayNameResourceKey);
        }

        #endregion

        #region Overrides of Window

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            MinimizeWindow();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Clipboard manager
            _clipboardMonitor = new ClipboardMonitor();
            _clipboardMonitor.ClipboardContentChanged += OnClipboardContentChanged;

            _hWnd = new WindowInteropHelper(this).Handle;

            // Notification menu items (when minimized)
            _exitMenuItem = new System.Windows.Forms.MenuItem(LocalizationProvider.GetLocalizedValue<string>(nameof(Properties.Resources.Trayicon_ExitLabel)));
            _settingsMenuItem = new System.Windows.Forms.MenuItem(LocalizationProvider.GetLocalizedValue<string>(nameof(Properties.Resources.Trayicon_SettingsLabel)));
            _compactDbMenuItem = new System.Windows.Forms.MenuItem(LocalizationProvider.GetLocalizedValue<string>(nameof(Properties.Resources.Trayicon_CompactDbLabel)));
            _toggleClipboardMonitorMenuItem = new System.Windows.Forms.MenuItem(LocalizationProvider.GetLocalizedValue<string>(nameof(Properties.Resources.Trayicon_DisconnectFromClipboardLabel)));

            _exitMenuItem.Click += OnExitMenuItemClick;
            _settingsMenuItem.Click += OnSettingsMenuItemClick;
            _compactDbMenuItem.Click += OnCompactDbMenuItemClick;
            _toggleClipboardMonitorMenuItem.Click += OnToggleClipboardMonitorMenuItemClick;

            _notifyIconContextMenu = new System.Windows.Forms.ContextMenu();
            _notifyIconContextMenu.MenuItems.Add(_settingsMenuItem);
            _notifyIconContextMenu.MenuItems.Add(_compactDbMenuItem);
            _notifyIconContextMenu.MenuItems.Add(_toggleClipboardMonitorMenuItem);
            _notifyIconContextMenu.MenuItems.Add("-");
            _notifyIconContextMenu.MenuItems.Add(_exitMenuItem);

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon(@"../../Resources/Clipboard.ico");
            _notifyIcon.MouseClick += OnNotifyIconClick;
            _notifyIcon.ContextMenu = _notifyIconContextMenu;
            _notifyIcon.Visible = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_forceClose)
            {
                _clipboardMonitor.ClipboardContentChanged -= OnClipboardContentChanged;
                _clipboardMonitor.Dispose();

                _notifyIcon.ContextMenu = null;
                _notifyIcon.Icon = null;
                _notifyIcon.MouseDown -= OnNotifyIconClick;
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();

                HotkeyManager.Current.Remove(Properties.Resources.GlobalHotKey);
            }
            else
            {
                e.Cancel = true;

                MinimizeWindow();
            }

            base.OnClosing(e);
        }

        #endregion
    }
}
