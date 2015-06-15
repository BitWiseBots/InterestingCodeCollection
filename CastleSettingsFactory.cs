public static class SettingsFactory
{
    private static readonly DictionaryAdapterFactory Factory = new DictionaryAdapterFactory();

    public static T Create<T>()
    {
        return Factory.GetAdapter<T>(ConfigurationManager.AppSettings);
    }
}

// This would be located either in a static class or in the WindsorInstaller implementation
private static ComponentRegistration<T> RegisterSetting<T>() where T : class
{
    return Component.For<T>().UsingFactoryMethod(SettingsFactory.Create<T>);
}