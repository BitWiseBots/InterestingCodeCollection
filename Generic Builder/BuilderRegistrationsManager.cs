using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using InterestingCodeCollection.GenericBuilder.Internal;

namespace InterestingCodeCollection.GenericBuilder
{
    /// <summary>
    /// Manages the results of <see cref="BuilderRegistration"/>s so that specified constructors can be used by <see cref="BuilderFactory"/>.
    /// </summary>
    public class BuilderRegistrationsManager : IBuilderRegistrationsManager
    {
        private static readonly Lazy<BuilderRegistrationsManager> InstanceInitializer = new Lazy<BuilderRegistrationsManager>();

        internal readonly Dictionary<string, Delegate> BuilderRegistrations = new Dictionary<string, Delegate>();
        internal readonly Dictionary<string, Delegate> PostBuildActionRegistrations = new Dictionary<string, Delegate>();
		internal readonly Dictionary<string, Dictionary<(string sourceType, string destinationType), LambdaExpression>> ValueConversionRegistrations = new Dictionary<string, Dictionary<(string sourceType, string destinationType), LambdaExpression>>();

        internal static BuilderRegistrationsManager Instance => InstanceInitializer.Value;

        /// <summary>
        /// Instantiates an instance of <typeparamref name="TBuilderRegistration"/> and calls <see cref="AddBuilderRegistration(BuilderRegistration)"/>
        /// </summary>
        /// <typeparam name="TBuilderRegistration">A type inheriting from <see cref="BuilderRegistration"/>.</typeparam>
        public static void AddBuilderRegistration<TBuilderRegistration>() where TBuilderRegistration : BuilderRegistration, new()
        {
            AddBuilderRegistration(new TBuilderRegistration());
        }

        /// <summary>
        /// Instantiates an instance of <paramref name="builderRegistrationType"/> and calls <see cref="AddBuilderRegistration(BuilderRegistration)"/>
        /// </summary>
        /// <param name="builderRegistrationType">A type inheriting from <see cref="BuilderRegistration"/>.</param>
        /// <exception cref="InvalidCastException">When the provided type does not inherit <see cref="BuilderRegistration"/>.</exception>
        public static void AddBuilderRegistration(Type builderRegistrationType)
        {
            AddBuilderRegistration((BuilderRegistration)Activator.CreateInstance(builderRegistrationType));
        }

        /// <summary>
        /// Retrieves the <see cref="BuilderRegistration.BuilderRegistrations"/> for the provided <see cref="BuilderRegistration"/> and appends them to <see cref="BuilderRegistrations"/> and <see cref="PostBuildActionRegistrations"/>.
        /// </summary>
        public static void AddBuilderRegistration(BuilderRegistration builderRegistration)
        {
            AppendBuilderRegistrations(builderRegistration);
        }

        /// <summary>
        /// Scans the provided assemblies for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/> and <see cref="PostBuildActionRegistrations"/>.
        /// </summary>
        /// <param name="assembliesToScan">
        /// The assemblies to be scanned, as few as possible assemblies should be provided. IE. don't use <c>AppDomain.CurrentDomain.GetAssemblies()</c> or similar.
        /// </param>
        public static void AddBuilderRegistrations(IEnumerable<Assembly> assembliesToScan)
        {
            AddBuilderRegistrationsCore(assembliesToScan);
        }

        /// <summary>
        /// Scans the provided assemblies for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/> and <see cref="PostBuildActionRegistrations"/>.
        /// </summary>
        /// <param name="assembliesToScan">
        /// The assemblies to be scanned, as few as possible assemblies should be provided. IE. don't use <c>AppDomain.CurrentDomain.GetAssemblies()</c> or similar.
        /// </param>
        public static void AddBuilderRegistrations(params Assembly[] assembliesToScan)
        {
            AddBuilderRegistrationsCore(assembliesToScan);
        }

        /// <summary>
        /// Loads and scans the assemblies with the provided names for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/> and <see cref="PostBuildActionRegistrations"/>.
        /// </summary>
        /// <param name="assemblyNamesToScan">
        /// The assemblies names to be loaded and scanned.
        /// </param>
        public static void AddBuilderRegistrations(IEnumerable<string> assemblyNamesToScan)
        {
            AddBuilderRegistrationsCore(assemblyNamesToScan.Select(Assembly.Load));
        }

        /// <summary>
        /// Loads and scans the assemblies with the provided names for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/> and <see cref="PostBuildActionRegistrations"/>.
        /// </summary>
        /// <param name="assemblyNamesToScan">
        /// The assemblies names to be loaded and scanned.
        /// </param>
        public static void AddBuilderRegistrations(params string[] assemblyNamesToScan)
        {
            AddBuilderRegistrationsCore(assemblyNamesToScan.Select(Assembly.Load));
        }

        /// <summary>
        /// Scans the assemblies of the provided types for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/> and <see cref="PostBuildActionRegistrations"/>.
        /// </summary>
        /// <param name="typesFromAssembliesContainingRegistrations">
        /// The types whose assemblies should be scanned.
        /// </param>
        public static void AddBuilderRegistrations(IEnumerable<Type> typesFromAssembliesContainingRegistrations)
        {
            AddBuilderRegistrationsCore(typesFromAssembliesContainingRegistrations.Select(t => t.GetTypeInfo().Assembly));
        }

        /// <summary>
        /// Scans the assemblies of the provided types for implementations of <see cref="BuilderRegistration"/> and retrieves their <see cref="BuilderRegistration.BuilderRegistrations"/> and <see cref="PostBuildActionRegistrations"/>.
        /// </summary>
        /// <param name="typesFromAssembliesContainingRegistrations">
        /// The types whose assemblies should be scanned.
        /// </param>
        public static void AddBuilderRegistrations(params Type[] typesFromAssembliesContainingRegistrations)
        {
            AddBuilderRegistrationsCore(typesFromAssembliesContainingRegistrations.Select(t => t.GetTypeInfo().Assembly));
        }

        /// <inheritdoc />
        public Func<IConstructorBuilder<T>,T> GetConstructorFunc<T>()
        {
            var key = typeof(T).GetRegistrationKey();
            return BuilderRegistrations.ContainsKey(key) ? (Func<IConstructorBuilder<T>, T>)BuilderRegistrations[key] : null;
        }

        /// <inheritdoc />
        public Action<T> GetPostBuildAction<T>()
        {
            var key = typeof(T).GetRegistrationKey();
            return PostBuildActionRegistrations.ContainsKey(key) ? (Action<T>)PostBuildActionRegistrations[key] : null;
        }

        /// <inheritdoc />
        public Dictionary<(string sourceType, string destinationType), LambdaExpression> GetValueConversionFuncs<T>()
        {
	        var key = typeof(T).GetRegistrationKey();
	        return ValueConversionRegistrations.ContainsKey(key) ? ValueConversionRegistrations[key]: null;
        }

        /// <summary>
        /// Scans the provided assemblies for types inheriting from <see cref="BuilderRegistration"/> and calls <see cref="AddBuilderRegistration(Type)"/>.
        /// </summary>
        /// <param name="assembliesToScan">The assemblies to be scanned.</param>
        private static void AddBuilderRegistrationsCore(IEnumerable<Assembly> assembliesToScan)
        {
            var allTypes = assembliesToScan.Where(a => !a.IsDynamic).SelectMany(a => a.DefinedTypes);

            var registrations = allTypes.Where(t => typeof(BuilderRegistration).GetTypeInfo().IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .Select(t => t.AsType());

            foreach (var registration in registrations)
            {
                AddBuilderRegistration(registration);
            }
        }

        /// <summary>
        /// Add registrations from an individual <see cref="BuilderRegistration"/> to <see cref="BuilderRegistrations"/> and <see cref="PostBuildActionRegistrations"/>.
        /// </summary>
        private static void AppendBuilderRegistrations(BuilderRegistration builderRegistration)
        {
            foreach (var registration in builderRegistration.BuilderRegistrations)
            {
                if ( Instance.BuilderRegistrations.ContainsKey(registration.Key))
                {
                    throw new ConfigurationErrorsException($"A constructor func has already been registered for type '{registration.Key}'");
                }

                Instance.BuilderRegistrations.Add(registration.Key, registration.Value);
            }

            foreach (var registration in builderRegistration.BuilderPostBuildRegistrations)
            {
                if ( Instance.PostBuildActionRegistrations.ContainsKey(registration.Key))
                {
                    throw new ConfigurationErrorsException($"A post build action has already been registered for type '{registration.Key}'");
                }

                Instance.PostBuildActionRegistrations.Add(registration.Key, registration.Value);
            }
        }
    }
}
