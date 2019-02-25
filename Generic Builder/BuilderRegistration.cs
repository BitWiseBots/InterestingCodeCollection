using System;
using System.Collections.Generic;
using System.Configuration;
using InterestingCodeCollection.GenericBuilder.Internal;

namespace InterestingCodeCollection.GenericBuilder
{
    /// <summary>
    /// Provides the base functionality for registering constructors to be used by <see cref="BuilderFactory"/> when creating new <see cref="IBuilder{T}"/>.
    /// </summary>
    public abstract class BuilderRegistration : IHideObjectMembers
    {
        /// <summary>
        /// Store all the registrations in an <c>internal</c> collection to be collated by <see cref="BuilderRegistrationsManager"/>
        /// </summary>
        internal readonly Dictionary<string, Delegate> BuilderRegistrations = new Dictionary<string, Delegate>();

        /// <summary>
        /// Store all the post build registrations in an <c>internal</c> collection to be collated by <see cref="BuilderRegistrationsManager"/>
        /// </summary>
        internal readonly Dictionary<string, Delegate> BuilderPostBuildRegistrations = new Dictionary<string, Delegate>();

        /// <summary>
        /// Store all the value conversion registrations in an <c>internal</c> collection to be collated by <see cref="BuilderRegistrationsManager"/>
        /// </summary>
        internal readonly Dictionary<string, Dictionary<(string,string),Delegate>> BuilderValueConversionRegistrations = new Dictionary<string, Dictionary<(string,string),Delegate>>();

        /// <summary>
        /// Adds a constructor function for a type to the list of registrations.
        /// </summary>
        /// <typeparam name="T">The type of object to be constructed.</typeparam>
        /// <param name="constructorFunc">
        /// An expression that produces a new instance of <typeparamref name="T"/>.
        /// The expression is passed a <see cref="IConstructorBuilder{T}"/> that can be used to reference values set by <see cref="M:IBuilder{T}.With"/>.
        /// </param>
        protected void RegisterBuilder<T>(Func<IConstructorBuilder<T>, T> constructorFunc)
        {
            var registrationKey = typeof(T).GetRegistrationKey();
            if (BuilderRegistrations.ContainsKey(registrationKey))
            {
                throw new ConfigurationErrorsException($"A constructor func has already been registered for type '{registrationKey}'");
            }

            BuilderRegistrations[registrationKey] = constructorFunc;
        }

        /// <summary>
        /// Adds a post build action for a type to the list of registrations.
        /// </summary>
        /// <typeparam name="T">The type of object that was built.</typeparam>
        /// <param name="postBuildAction">
        /// An expression that produces performs additional work on the built <typeparamref name="T"/> instance after the builder is finished.
        /// </param>
        protected void RegisterPostBuildAction<T>(Action<T> postBuildAction)
        {
            var registrationKey = typeof(T).GetRegistrationKey();
            if (BuilderPostBuildRegistrations.ContainsKey(registrationKey))
            {
                throw new ConfigurationErrorsException($"A post build action has already been registered for type '{registrationKey}'");
            }

            BuilderPostBuildRegistrations[registrationKey] = postBuildAction;
        }

        /// <summary>
        /// Adds a value conversion fucntion for transforming an intermediate type to the target property's type.
        /// </summary>
        /// <typeparam name="T">The type of object that was built.</typeparam>
        /// <typeparam name="TDestination">The type of property that will be set on the built object.</typeparam>
        /// <typeparam name="TSource">The type of property that will be provided to the builder.</typeparam>
        /// <param name="valueConversionFunc">
        /// An expression that will transform a value of type <typeparamref name="TSource"/> to a value of <typeparamref name="TDestination"/>
        /// Where <typeparamref name="TDestination"/> can be set to a property on <typeparamref name="T"/>.
        /// </param>
        protected void RegisterValueConversion<T, TDestination, TSource>(Func<TSource, TDestination> valueConversionFunc)
        {
	        var typeKey = typeof(T).GetRegistrationKey();
	        var sourceKey = typeof(TSource).GetRegistrationKey();
	        var destinationKey = typeof(TDestination).GetRegistrationKey();

	        if (!BuilderValueConversionRegistrations.ContainsKey(typeKey))
	        {
		        BuilderValueConversionRegistrations[typeKey] = new Dictionary<(string, string), Delegate>();
	        }

	        if (BuilderValueConversionRegistrations[typeKey].ContainsKey((sourceKey, destinationKey)))
	        {
		        throw new ConfigurationErrorsException($"A value conversion function has already been registered for type '{typeKey}' that converts from type '{sourceKey}' to type '{destinationKey}'");
	        }

	        BuilderValueConversionRegistrations[typeKey][(sourceKey, destinationKey)] = valueConversionFunc;
        }
    }
}
