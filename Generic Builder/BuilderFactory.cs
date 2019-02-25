using System;
using System.Linq;
using System.Reflection;
using InterestingCodeCollection.GenericBuilder.Internal;

namespace InterestingCodeCollection.GenericBuilder
{
    /// <summary>
    /// Creates <see cref="Builder{T}"/>s for use in unit tests.
    /// </summary>
    public static class BuilderFactory
    {
		private static readonly MethodInfo CreateMethodInfo = typeof(BuilderFactory).GetMethods().Single(m => m.Name == nameof(Create) && m.IsGenericMethod && m.IsStatic);

        static BuilderFactory()
        {
            BuilderRegistrationsManager = Builders.BuilderRegistrationsManager.Instance;
        }

        internal static IBuilderRegistrationsManager BuilderRegistrationsManager { private get; set; }

        /// <summary>
        /// Creates a <see cref="Builder{T}"/> for the provided type, uses a Constructor Expression if one was registered with <see cref="Builders.BuilderRegistrationsManager"/>.
        /// </summary>
        /// <typeparam name="T">The type to be built.</typeparam>
        public static IBuilder<T> Create<T>()
        {
            return new Builder<T>(BuilderRegistrationsManager.GetConstructorFunc<T>(), BuilderRegistrationsManager.GetPostBuildAction<T>(), BuilderRegistrationsManager.GetValueConversionFuncs<T>());
        }

		/// <summary>
		/// Creates a <see cref="Builder{T}"/> for the provided type variable using reflection and will use a Constructor Expression if one was registered
		/// </summary>
		/// <param name="typeToBuild">The type to be built.</param>
        internal static object Create(Type typeToBuild)
        {
	        dynamic builder = CreateMethodInfo.MakeGenericMethod(typeToBuild).Invoke(null, new object[]{});
	        return builder.Build();
        }
    }
}
