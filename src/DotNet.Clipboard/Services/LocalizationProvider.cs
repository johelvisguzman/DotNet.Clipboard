namespace DotNet.Clipboard.Services
{
    using System.Reflection;
    using WPFLocalizeExtension.Extensions;

    public static class LocalizationProvider
    {
        /// <summary>
        /// Gets a localized value.
        /// </summary>
        /// <typeparam name="TValue">The type of the returned value.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The resolved localized object.</returns>
        public static TValue GetLocalizedValue<TValue>(string key)
        {
            return LocExtension.GetLocalizedValue<TValue>(Assembly.GetCallingAssembly().GetName().Name + ":Resources:" + key);
        }
    }
}
