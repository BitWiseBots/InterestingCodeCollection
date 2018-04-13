using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Builders
{
    /// <summary>
    /// Creates <see cref="Builder{T}"/> for use in unit tests.
    /// </summary>
    public static class BuilderFactory
    {
        private static readonly Dictionary<string, LambdaExpression> BuilderRegistrations = new Dictionary<string, LambdaExpression>();

       /// <summary>
       /// Finds all <see cref="IBuilderFactoryRegistration"/> implementations in the TestSupport assembly and calls <see cref="IBuilderFactoryRegistration.RegisterBuilders"/> on them.
       /// </summary>
       static BuilderFactory()
       {
           var builderRegistrations = Assembly.GetAssembly(typeof(BuilderFactory)).GetTypes().Where(t => typeof(IBuilderFactoryRegistration).IsAssignableFrom(t) && t.IsClass);

           foreach (var builderRegistration in builderRegistrations)
           {
               var registration = (IBuilderFactoryRegistration) Activator.CreateInstance(builderRegistration);
               registration.RegisterBuilders();
           }
       }

       /// <summary>
       /// Creates a <see cref="Builder{T}"/> for the provided type, uses a Constructor Expression if one was registered with <see cref="RegisterBuilder{T}"/>.
       /// </summary>
       /// <typeparam name="T">The type to be built.</typeparam>
       public static Builder<T> Create<T>()
       {
           return BuilderRegistrations.ContainsKey(typeof(T).Name)
               ? new Builder<T>((Expression<Func<Builder<T>, T>>)BuilderRegistrations[typeof(T).Name])
               : new Builder<T>();
       }

       /// <summary>
       /// Registers an expression to be used when constructing the <typeparamref name="T"/> instance.
       /// </summary>
       /// <typeparam name="T">The type to be built.</typeparam>
       /// <param name="constructorExpression">An expression that will create a <see cref="T"/>. The passed in <see cref="Builder{T}"/> can be used to retrieve values for use in the constructor.</param>
       /// <remarks>This method should be called from within a <see cref="IBuilderFactoryRegistration"/> implementation.</remarks>
       public static void RegisterBuilder<T>(Expression<Func<Builder<T>, T>> constructorExpression)
       {
           BuilderRegistrations[typeof(T).Name] = constructorExpression;
       }
    }
}
