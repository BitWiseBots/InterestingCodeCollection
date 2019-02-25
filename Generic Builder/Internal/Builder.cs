using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace InterestingCodeCollection.GenericBuilder.Internal
{
    /// <summary>
    /// Implements the logic for building an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to be built.</typeparam>
    public sealed class Builder<T> : IBuilder<T>, IConstructorBuilder<T>
    {
        private readonly Dictionary<string, ExpressionRecord> _withExpressions;
        private readonly Func<IConstructorBuilder<T>, T> _constructorFunc;
        private readonly Action<T> _postBuildAction;
        private readonly Dictionary<(string sourceType, string destinationType), LambdaExpression> _valueConversionFuncs;

        internal Builder(Func<IConstructorBuilder<T>, T> constructorFunc, Action<T> postBuildAction, Dictionary<(string sourceType, string destinationType), LambdaExpression> valueConversionFuncs)
        {
            _withExpressions = new Dictionary<string, ExpressionRecord>();
            _constructorFunc = constructorFunc;
            _postBuildAction = postBuildAction;
            _valueConversionFuncs = valueConversionFuncs;
        }

        /// <inheritdoc />
        public IBuilder<T> With<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> expression, params IBuilder<TProperty>[] valueBuilders)
        {
            return With(expression, valueBuilders.Select(v => v.Build()).ToList());
        }

        /// <inheritdoc />
        public IBuilder<T> WithConversion<TProperty, TIntermediate>(Expression<Func<T, TProperty>> expression, params IBuilder<TIntermediate>[] values)
        {
	        return WithConversion(expression,values.Select(v => v.Build()).ToList());
        }

        /// <inheritdoc />
        public IBuilder<T> With<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> expression, params TProperty[] values)
        {
            return With(expression, values.ToList());
        }

        /// <inheritdoc />
        public IBuilder<T> WithConversion<TProperty, TIntermediate>(Expression<Func<T, TProperty>> expression, params TIntermediate[] values)
        {
	        return WithConversion(expression,values.ToList());
        }

        /// <inheritdoc />
        public IBuilder<T> With<TProperty>(Expression<Func<T, TProperty>> expression, IBuilder<TProperty> valueBuilder)
        {
            return With(expression, valueBuilder.Build());
        }

        /// <inheritdoc />
        public IBuilder<T> WithConversion<TProperty, TIntermediate>(Expression<Func<T, TProperty>> expression, IBuilder<TIntermediate> valueBuilder)
        {
	        return WithConversion(expression, valueBuilder.Build());
        }

        /// <inheritdoc />
        public IBuilder<T> With<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
        {
            _withExpressions.Add(GetPropertyPathRecursively(expression), new ExpressionRecord(expression, value));
            return this;
        }

        /// <inheritdoc />
        public IBuilder<T> WithConversion<TProperty, TIntermediate>(Expression<Func<T, TProperty>> expression, TIntermediate value)
        {
			_withExpressions.Add(GetPropertyPathRecursively(expression), new ExpressionRecord(expression, value));
			return this;
        }

        /// <inheritdoc />
        TProperty IConstructorBuilder<T>.From<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var propertyKey = GetPropertyPathRecursively(expression);

            if (!_withExpressions.ContainsKey(propertyKey))
            {
                return default;
            }

            var withExpression = _withExpressions[propertyKey];
            withExpression.HasBeenUsed = true;
            return (TProperty)withExpression.Value;
        }

        /// <inheritdoc />
        TProperty IConstructorBuilder<T>.From<TProperty>(Expression<Func<T, TProperty>> expression, IBuilder<TProperty> defaultValueBuilder)
        {
	        IConstructorBuilder<T> builder = this;
            return builder.From(expression, defaultValueBuilder.Build());
        }

        /// <inheritdoc />
        TProperty IConstructorBuilder<T>.From<TProperty>(Expression<Func<T, TProperty>> expression, TProperty defaultValue)
        {
            var propertyKey = GetPropertyPathRecursively(expression);

            if (!_withExpressions.ContainsKey(propertyKey))
            {
                return defaultValue;
            }

            var withExpression = _withExpressions[propertyKey];
            withExpression.HasBeenUsed = true;
            return (TProperty)withExpression.Value;
        }

        /// <inheritdoc />
        public T Build()
        {
            var builtObject = Create();

            // Loop through the property registrations and for any that haven't already been used (within the constructor expression) apply them to our object.
            foreach (var withExpression in _withExpressions.Values.Where(v => !v.HasBeenUsed))
            {
                DecideExpressionType<object>(
                    withExpression.Expression,
                    memberExpression =>
                    {
                        // Recurse over the expression chain and ensure all levels are instantiated
                        var obj = RecursivelyInstantiateExpressionChain(memberExpression.Expression, builtObject) ?? builtObject;

                        var memberInfo = (PropertyInfo) memberExpression.Member;

                        var memberValue = withExpression.Value;

                        // The types don't match so look to see if a conversion is available.
						if (!memberInfo.PropertyType.IsInstanceOfType(memberValue))
						{
							var registrationKey = (sourceType: memberValue.GetType().GetRegistrationKey(), destinationType: memberInfo.PropertyType.GetRegistrationKey());
	                        if (_valueConversionFuncs?.ContainsKey(registrationKey) ?? false)
	                        {
		                        memberValue = _valueConversionFuncs[registrationKey].Compile().DynamicInvoke(memberValue);
	                        }
	                        else
	                        {
		                        throw new ConfigurationErrorsException($"Unable to find a value conversion func for converting from '{registrationKey.sourceType}' to '{registrationKey.destinationType}'.\nEnsure a registration exists in an implementation of IBuilderFactoryRegistration.\nAnd that you have called BuilderFactory.RunBuilderRegistrationsFromAssemblies with the assembly or assemblies that contain your implementations.");
	                        }
                        }

						// Set the bottom level property with the provided value.
						memberInfo.SetValue(obj, memberValue);

                        return null;
                    },
                    indexExpression =>
                    {
                        // Recurse over the expression chain and ensure all levels are instantiated
                        var obj = RecursivelyInstantiateExpressionChain(indexExpression.Object, builtObject) ?? builtObject;

                        var memberInfo = indexExpression.Indexer;

                        var memberValue = withExpression.Value;

                        // The types don't match so look to see if a conversion is available.
                        if (!memberInfo.PropertyType.IsInstanceOfType(memberValue))
                        {
	                        var registrationKey = (sourceType: memberValue.GetType().GetRegistrationKey(), destinationType: memberInfo.PropertyType.GetRegistrationKey());
	                        if (_valueConversionFuncs?.ContainsKey(registrationKey) ?? false)
	                        {
		                        memberValue = _valueConversionFuncs[registrationKey].Compile().DynamicInvoke(memberValue);
	                        }
	                        else
	                        {
		                        throw new ConfigurationErrorsException($"Unable to find a value conversion func for converting from '{registrationKey.sourceType}' to '{registrationKey.destinationType}'.\nEnsure a registration exists in an implementation of IBuilderFactoryRegistration.\nAnd that you have called BuilderFactory.RunBuilderRegistrationsFromAssemblies with the assembly or assemblies that contain your implementations.");
	                        }
                        }

	                    var indexerArguments = GetIndexerArguments(indexExpression);

                        // Set the bottom level property with the provided value.
                        memberInfo.SetValue(obj, memberValue, indexerArguments);

                        return null;
                    });
            }

            _postBuildAction?.Invoke(builtObject);

            return builtObject;
        }

        /// <summary>
        /// Steps through the expression tree and ensures the property at every node is instantiated.
        /// </summary>
        /// <param name="expression">The expression to parse, should not include the bottom level</param>
        /// <param name="builtObject">The object being built.</param>
        /// <returns>Either the existing or new value for the current node of the tree.</returns>
        private object RecursivelyInstantiateExpressionChain(Expression expression, object builtObject)
        {
            return DecideExpressionType(
                expression,
                memberExpression =>
                {
                    var parent = RecursivelyInstantiateExpressionChain(memberExpression.Expression, builtObject) ?? builtObject;
                    var memberInfo = (PropertyInfo) memberExpression.Member;
                    return GetOrInstantiateLevel(memberInfo.PropertyType, () => memberInfo.GetValue(parent), value => { memberInfo.SetValue(parent, value); });

                },
                indexExpression =>
                {
                    var parent = RecursivelyInstantiateExpressionChain(indexExpression.Object, builtObject) ?? builtObject;

	                var indexerArguments = GetIndexerArguments(indexExpression);

                    var memberInfo = indexExpression.Indexer;
                    return GetOrInstantiateLevel(memberInfo.PropertyType, () =>
                    {
                        try
                        {
                            return memberInfo.GetValue(parent, indexerArguments);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }, value => { memberInfo.SetValue(parent, value, indexerArguments); });
                });
        }

        /// <summary>
        /// Provides the common logic for getting or retrieving a value, regardless of the expression node type.
        /// </summary>
        /// <param name="propertyType">The type of the property to be retrieved or instantiated.</param>
        /// <param name="valueGetter">A function that will attempt to get the existing value.</param>
        /// <param name="valueSetter">A function to set the value.</param>
        /// <returns></returns>
        private object GetOrInstantiateLevel(Type propertyType, Func<object> valueGetter, Action<object> valueSetter)
        {
            var memberValue = valueGetter();

            if (memberValue != null)
            {
                return memberValue;
            }

            try
            {
                memberValue = Activator.CreateInstance(propertyType);
            }
            catch (MissingMethodException)
            {
                throw new NotSupportedException($"Implicitly instantiated nested properties must have a parameter-less constructor. \n\tType: {propertyType}");
            }

            valueSetter(memberValue);

            return memberValue;
        }

        /// <summary>
        /// Recurses over an Expression tree to build the full property path string.
        /// </summary>
        private string GetPropertyPathRecursively(Expression expression)
        {
            return DecideExpressionType(
                expression,
                memberExpression =>
                {
                    var parent = GetPropertyPathRecursively(memberExpression.Expression);
                    var current = memberExpression.Member.Name;
                    return CombineKeyComponents(parent, current);
                },
                indexExpression =>
                {
					var indexerArguments = GetIndexerArguments(indexExpression);

	                var parent = GetPropertyPathRecursively(indexExpression.Object);
                    var current = $"{indexExpression.Indexer.Name}[{string.Join(",", indexerArguments)}]";
                    return CombineKeyComponents(parent, current);
                });
        }

	    /// <summary>
        /// Combines the provided values into a '.' separated string.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        private string CombineKeyComponents(string parent, string current)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(parent))
            {
                sb.Append($"{parent}.");
            }

            sb.Append(current);

            return sb.ToString();
        }

        /// <summary>
        /// Either executes the <see cref="_constructorFunc"/> if given, or attempts to create a <typeparamref name="T"/> instance using <see cref="Activator"/>.
        /// </summary>
        private T Create()
        {
            if (_constructorFunc != null)
            {
                return _constructorFunc(this);
            }

            if (typeof(T).GetConstructor(new Type[0]) != null)
            {
                return Activator.CreateInstance<T>();
            }

            throw new ConfigurationErrorsException($"No Parameter-less Constructor present on type {typeof(T).FullName}.\nEnsure a registration exists in an implementation of IBuilderFactoryRegistration.\nAnd that you have called BuilderFactory.RunBuilderRegistrationsFromAssemblies with the assembly or assemblies that contain your implementations.");
        }

        /// <summary>
        /// Provides the ability to switch the code branch based on the type of expression being parsed.
        /// </summary>
        /// <param name="expressionToParse">The expression to consider.</param>
        /// <param name="actionForMemberExpressions">The function to perform if <paramref name="expressionToParse"/> is a <see cref="MemberExpression"/>.</param>
        /// <param name="actionForIndexExpressions">The function to perform if <paramref name="expressionToParse"/> is a <see cref="IndexExpression"/>.</param>
        private TResult DecideExpressionType<TResult>(Expression expressionToParse, Func<MemberExpression,TResult> actionForMemberExpressions, Func<IndexExpression,TResult> actionForIndexExpressions)
        {
            var expressionNode = (expressionToParse as LambdaExpression)?.Body ?? expressionToParse;
            var memberExpression = GetMemberExpression(expressionNode);
            if (memberExpression != null)
            {
                return actionForMemberExpressions(memberExpression);
            }

            var indexedExpression = GetIndexExpression(expressionNode);
            if (indexedExpression != null)
            {
               return actionForIndexExpressions(indexedExpression);
            }

            var paramExpression = expressionNode as ParameterExpression;

            //If the next expression in the chain is not a ParameterExpression then it is some other expression that we don't support.
            if (paramExpression == null)
            {
                throw new NotSupportedException($"The provided expression contains an expression node type that is not supported: \n\tNode Type:\t{expressionNode.NodeType}\n\tBody:\t\t{expressionNode}.\nEnsure that the expression only contains property accessors.");
            }

            // We've reached the top of the tree.
            return default;
        }


        /// <summary>
        /// Gets a <see cref="MemberExpression"/> from the given <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The expression to dig into.</param>
        /// <returns>
        /// If <paramref name="expression"/> is a <see cref="MemberExpression"/> then <paramref name="expression"/>,
        /// If <paramref name="expression"/> is a <see cref="LambdaExpression"/> then <see cref="LambdaExpression.Body"/>,
        ///     this will also unwrap <see cref="UnaryExpression"/>s,
        /// Otherwise <c>null</c>.
        /// </returns>
        private static MemberExpression GetMemberExpression(Expression expression)
        {
            var member = expression as MemberExpression;
            var unary = expression as UnaryExpression;
            return member
                ?? unary?.Operand as MemberExpression;
        }

        /// <summary>
        /// Gets a <see cref="IndexExpression"/> from the given <see cref="Expression"/> by taking a <see cref="MethodCallExpression"/> to the Indexer's get and building a new <see cref="IndexExpression"/> to the underlying property.
        /// </summary>
        /// <param name="expression">A <see cref="LambdaExpression"/> to unwrap or a <see cref="MethodCallExpression"/> to use directly.</param>
        /// <returns>A new <see cref="IndexExpression"/> using the same indexer arguments provided in the original <see cref="MethodCallExpression"/>.</returns>
        private IndexExpression GetIndexExpression(Expression expression)
        {
            // Unwrap the expression if its a Lambda
            var lambda = expression as LambdaExpression;
            var methodCallExpression = lambda?.Body as MethodCallExpression ?? expression as MethodCallExpression;

            // Short circuit if the expression isn't a valid node, or doesn't have a parent expression.
            if (methodCallExpression?.Method.Name != "get_Item" || methodCallExpression.Object == null) return null;

            // Find the Indexer property info with the same signature as the one called in the MethodCallExpression
            var indexerProperty = (from p in methodCallExpression.Object.Type.GetDefaultMembers().OfType<PropertyInfo>()
                let q = p.GetIndexParameters()
                where q.Length > 0
                      && q.Length == methodCallExpression.Arguments.Count
                      && q.Select(t => t.ParameterType).All(t => methodCallExpression.Arguments.Any(mt => mt.Type == t))
                select p).SingleOrDefault();

            // If no Indexer Property was found then we can't find a matching signature.
            return indexerProperty != null
                ? Expression.MakeIndex(methodCallExpression.Object, indexerProperty, methodCallExpression.Arguments)
                : null;
        }

	    private static object[] GetIndexerArguments(IndexExpression indexExpression)
	    {
		    //Extract the indexer values from their expression.
		    var expressionArgs = indexExpression.Arguments
			    .Select(e => Expression.Lambda(e).Compile().DynamicInvoke());

		    return expressionArgs.ToArray();
	    }

        /// <summary>
        /// Tiny class that combines a given expression along with the value to be used when setting.
        /// </summary>
        private class ExpressionRecord
        {
            public ExpressionRecord(LambdaExpression expression, object value)
            {
                Expression = expression;
                Value = value;
                HasBeenUsed = false;
            }

            /// <summary>
            /// The Expression of the property to be set.
            /// </summary>
            public LambdaExpression Expression { get; }

            /// <summary>
            /// The value to be set to the property.
            /// </summary>
            public object Value { get; }

            /// <summary>
            /// Flag used to denote if the expression was used as part of a <see cref="IConstructorBuilder{T}"/> and thus should not be used by the <see cref="IBuilder{T}"/>.
            /// </summary>
            public bool HasBeenUsed { get; set; }
        }
    }
}
