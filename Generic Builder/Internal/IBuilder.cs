using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace InterestingCodeCollection.GenericBuilder.Internal
{
    /// <summary>
    /// Primary interface for interacting with a builder to set values on properties.
    /// </summary>
    /// <typeparam name="T">The type of object being built.</typeparam>
    public interface IBuilder<T> : IHideObjectMembers
    {
        /// <summary>
        /// Provides a collection of <see cref="IBuilder{T}"/> values to be built and set on the list property specified by <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T2">The type of the property being set.</typeparam>
        /// <param name="expression">An expression that accesses the property to be set.</param>
        /// <param name="valueBuilders">The collection of <see cref="IBuilder{T}"/> that will build the values to be added on the property.</param>
        [PublicAPI]
        IBuilder<T> With<T2>(Expression<Func<T, IEnumerable<T2>>> expression, params IBuilder<T2>[] valueBuilders);

        /// <summary>
        /// Provides a collection of values to be set on the list property specified by <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T2">The type of the property being set.</typeparam>
        /// <param name="expression">An expression that accesses the property to be set.</param>
        /// <param name="values">The collection of values to be added on the property.</param>
        [PublicAPI]
        IBuilder<T> With<T2>(Expression<Func<T, IEnumerable<T2>>> expression, params T2[] values);

        /// <summary>
        /// Provides the value as a <see cref="IBuilder{T}"/> to be set on the property specified by <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T2">The type of the property being set.</typeparam>
        /// <param name="expression">An expression that accesses the property to be set.</param>
        /// <param name="valueBuilder">The value to be set on the property.</param>
        [PublicAPI]
        IBuilder<T> With<T2>(Expression<Func<T, T2>> expression, IBuilder<T2> valueBuilder);

        /// <summary>
        /// Provides a the value to be set on the property specified by <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T2">The type of the property being set.</typeparam>
        /// <param name="expression">An expression that accesses the property to be set.</param>
        /// <param name="value">The value to be set on the property.</param>
        [PublicAPI]
        IBuilder<T> With<T2>(Expression<Func<T, T2>> expression, T2 value);

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> either using <see cref="Activator"/> or a provided constructor expression.
        /// </summary>
        [PublicAPI]
        T Build();
    }
}
