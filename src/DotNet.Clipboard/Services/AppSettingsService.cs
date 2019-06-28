namespace DotNet.Clipboard.Services
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Runtime.CompilerServices;

    public sealed class AppSettingsService : ApplicationSettingsBase, IAppSettingsService
    {
        private static readonly AppSettingsService _instance = (AppSettingsService)Synchronized(new AppSettingsService());

        [UserScopedSetting]
        [DefaultSettingValue("True")]
        public bool IsClipboardMonitoring
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        [UserScopedSetting]
        [DefaultSettingValue("Control + Q")]
        public string HotKey
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        [UserScopedSetting]
        [DefaultSettingValue("100")]
        public int MaxSavedCopiesCount
        {
            get { return Get<int>(); }
            set { Set(value); }
        }

        [UserScopedSetting]
        [DefaultSettingValue("en-US")]
        public string Culture
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        public bool IsDirty { get; private set; }

        public static IAppSettingsService Default => _instance;

        private AppSettingsService() { }

        private T Get<T>([CallerMemberName] string propertyName = null)
        {
            return (T)this[propertyName];
        }

        private void Set<T>(T newValue = default(T), [CallerMemberName] string propertyName = null)
        {
            var oldValue = (T)this[propertyName];

            if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
                return;

            this[propertyName] = newValue;

            IsDirty = true;
        }

        public override void Save()
        {
            if (!IsDirty)
                return;

            base.Save();

            IsDirty = false;
        }
    }
}
