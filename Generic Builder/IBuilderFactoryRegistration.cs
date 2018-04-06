namespace Builders
{
    public interface IBuilderFactoryRegistration
    {
        /// <summary>
        /// Calls <see cref="BuilderFactory.RegisterBuilder{T}"/> for each builder in the registration's scope.
        /// </summary>
        void RegisterBuilders();
    }
}
