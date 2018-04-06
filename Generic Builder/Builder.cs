using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Builders
{
    public class Builder<T>
    {
        private T _data;
        private readonly Dictionary<string, ExpressionRecord> _withExpressions;
        private readonly Expression<Func<Builder<T>, T>> _constructorExpression;

        /// <summary>
        /// Only use this constructor if <see cref="T"/> has a parameterless constructor.
        /// </summary>
        public Builder()
        {
            _withExpressions = new Dictionary<string, ExpressionRecord>();
        }

        /// <summary>
        /// Use this constructor to be able to specify which constructor to call and pass it parameters that have been registered with the builder.
        /// </summary>
        /// <param name="constructorExpression">An expression that calls the constructor for <typeparamref name="T"/> and retrieves values from the builder</param>
        public Builder(Expression<Func<Builder<T>, T>> constructorExpression) : this()
        {
            _constructorExpression = constructorExpression;
        }

        /// <summary>
        /// Provides a collection of <see cref="Builder{T}"/> values to be built and set on the list property specified by <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T2">The type of the property being set.</typeparam>
        /// <param name="expression">An expression that accesses the property to be set.</param>
        /// <param name="valueBuilders">The collection of <see cref="Builder{T}"/> that will build the values to be added on the property.</param>
        public Builder<T> With<T2>(Expression<Func<T, List<T2>>> expression, params Builder<T2>[] valueBuilders)
        {
            return With(expression, valueBuilders.Select(v => v.Build()).ToList());
        }

        /// <summary>
        /// Provides a collection of values to be set on the list property specified by <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T2">The type of the property being set.</typeparam>
        /// <param name="expression">An expression that accesses the property to be set.</param>
        /// <param name="values">The collection of values to be added on the property.</param>
        public Builder<T> With<T2>(Expression<Func<T, List<T2>>> expression, params T2[] values)
        {
            return With(expression, values.ToList());
        }

        /// <summary>
        /// Provides a the value to be set on the property specified by <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T2">The type of the property being set.</typeparam>
        /// <param name="expression">An expression that accesses the property to be set.</param>
        /// <param name="valueBuilder">The value to be set on the property.</param>
        public Builder<T> With<T2>(Expression<Func<T, T2>> expression, Builder<T2> valueBuilder)
        {
            return With(expression, valueBuilder.Build());
        }

        /// <summary>
        /// Provides a the value to be set on the property specified by <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T2">The type of the property being set.</typeparam>
        /// <param name="expression">An expression that accesses the property to be set.</param>
        /// <param name="value">The value to be set on the property.</param>
        public Builder<T> With<T2>(Expression<Func<T, T2>> expression, T2 value)
        {
            _withExpressions.Add(GetPropertyPath(expression), new ExpressionRecord(expression, value));
            return this;
        }

        /// <summary>
        /// Use this method within <see cref="Builder{T}"/> to use values set using any of the <see cref="M:With"/> overloads within the constructor expression.
        /// </summary>
        /// <typeparam name="T2">The type of the property being accessed.</typeparam>
        /// <param name="expression">An expression that specifies which property is being set within the constructor.</param>
        /// <returns>The value that was set for a call to <see cref="M:With"/> using the same <paramref name="expression"/>.</returns>
        public T2 From<T2>(Expression<Func<T, T2>> expression)
        {
            var propertyKey = GetPropertyPath(expression);
            var withExpression = _withExpressions[propertyKey];
            withExpression.HasBeenUsed = true;
            return (T2)withExpression.Value;
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> either using <see cref="Activator"/> or a provided constructor expression.
        /// </summary>
        public T Build()
        {
            _data = Create();

            //Loop through the property registrations and for any that haven't already been used (within the constructor expression) apply them to our object.
            foreach (var expressionTuple in _withExpressions.Values.Where(v => !v.HasBeenUsed))
            {
                //Get the bottom level expression of the chain
                var mExpr = GetMemberExpression(expressionTuple.Expression);

                //Recurse over the expression chain and ensure all levels are instantiated
                var obj = RecursivelyInstantiateExpressionChain(mExpr);

                var p = (PropertyInfo)mExpr.Member;

                //Set the bottom level property with the provided value.
                p.SetValue(obj, expressionTuple.Value);
            }

            return _data;
        }

        //This method recurses from the right side of the expression to the left, once it reaches the first property it will either instantiate
        //the property or get the existing one and pass it back up the call stack as the parent so that values can be set on it.
        private object RecursivelyInstantiateExpressionChain(MemberExpression expr)
        {
            // Get the last member in the expression chain.
            var pExpr = GetMemberExpression(expr.Expression);

            //if null we are at the top of the chain so we can return the base object.
            if (pExpr == null) return _data;

            //otherwise move to the next level up the chain
            var parent = RecursivelyInstantiateExpressionChain(pExpr);

            //check if the member in the current step in the expression chain has been instantiated and if not attempt to instantiate it.
            var memberInfo = (PropertyInfo)pExpr.Member;
            var memberValue = memberInfo.GetValue(parent);
            if (memberValue != null) return memberValue;

            try
            {
                memberValue = Activator.CreateInstance(memberInfo.PropertyType);

            }
            catch (Exception)
            {
                throw new NotSupportedException("Implicitly instantiated nested properties must have a parameterless constructor");
            }

            //member was instantiated so set it on the parent and return.
            memberInfo.SetValue(parent, memberValue);

            return memberValue;
        }

        /// <summary>
        /// Takes in a property access lambda, unwraps it, and passes it to <see cref="GetPropertyPathRecursively"/> to build the property path string.
        /// </summary>
        private string GetPropertyPath(LambdaExpression expression)
        {
            var mExpr = GetMemberExpression(expression);
            return GetPropertyPathRecursively(mExpr);
        }

        /// <summary>
        /// Recurses over an Expression tree to build the full property path string.
        /// </summary>
        private string GetPropertyPathRecursively(MemberExpression expr)
        {
            var memberExpression = expr.Expression as MemberExpression;
            var parent = memberExpression != null ? GetPropertyPathRecursively(memberExpression) : string.Empty;

            var current = expr.Member.Name;
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(parent))
            {
                sb.Append($"{parent}.");
            }
            sb.Append(current);

            return sb.ToString();
        }

        private T Create()
        {
            if (_constructorExpression == null)
            {
                if (typeof(T).GetConstructor(new Type[0]) != null)
                {
                    return Activator.CreateInstance<T>();
                }
                throw new Exception("No Parameterless Constructor present, use new DataBuilder<T>(object => object.Param1) constructor.");
            }

            return _constructorExpression.Compile()(this);
        }

        private static MemberExpression GetMemberExpression(Expression expr)
        {
            var lambda = expr as LambdaExpression;
            return lambda?.Body as MemberExpression;
        }

        private class ExpressionRecord
        {
            public ExpressionRecord(LambdaExpression expression, object value)
            {
                Expression = expression;
                Value = value;
                HasBeenUsed = false;
            }

            public LambdaExpression Expression { get; }
            public object Value { get; }

            public bool HasBeenUsed { get; set; }
        }
    }
}
