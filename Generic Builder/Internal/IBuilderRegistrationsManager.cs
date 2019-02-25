using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace InterestingCodeCollection.GenericBuilder.Internal
{
    /// <summary>
    /// Provides functionality for retrieving registrations for typed builders.
    /// </summary>
    internal interface IBuilderRegistrationsManager
    {
        /// <summary>
        /// Attempts to get a constructor function for the given <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to find a constructor function for.</typeparam>
        /// <returns>A <c>Func{IConstructorBuilder{T},T}</c> if one was registered, otherwise null.</returns>
        Func<IConstructorBuilder<T>,T> GetConstructorFunc<T>();

        /// <summary>
        /// Attempts to get a post build action for the given <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to find a post build action for.</typeparam>
        /// <returns>A <c>Action{T}</c> if one was registered, otherwise null.</returns>
        Action<T> GetPostBuildAction<T>();

        /// <summary>
        /// Attempts to get any value conversion functions for the given <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to find value conversion functions for.</typeparam>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> of lambda functions keyed by a combination of the source and destination types, otherwise null.</returns>
        Dictionary<(string sourceType, string destinationType), LambdaExpression> GetValueConversionFuncs<T>();
    }
}
