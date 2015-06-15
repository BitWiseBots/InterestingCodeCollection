/***********************************************************
 This class implements a general use builder pattern     
 It also has the added functionality of being able to 
 new up a property tree implicitly
 Eg. new DataBuilder<SomeClass>().SetValue(x=> x.FirstProperty.SecondProperty.ThirdProperty, "SomeValue")
 This will create a new instance and assign it for both the FirstProperty and SecondProperty values before
 assigning SomeValue to ThirdProperty
 The class will also cleanly handle list properties.
 Finally SetValue will also take in another DataBuilder so that multiple databuilders can be pieced together on the fly
 
 Improvement suggestion: The Databuilder(T data) constructor should likely perform a deep copy on data to avoid 
 issues of maintaining a reference to the original
***********************************************************/
public class DataBuilder<T>
    {
        private readonly T _data;

        public DataBuilder()
        {
            _data = Activator.CreateInstance<T>();
        }

        public DataBuilder(T data)
        {
            _data = data;
        }

        public DataBuilder<T> SetValue<T2>(Expression<Func<T, List<T2>>> expression, params T2[] values)
        {
            return SetValue(expression, values.ToList());
        }

        public DataBuilder<T> SetValue<T2>(Expression<Func<T, T2>> expression, T2 value)
        {
            var mExpr = expression.GetMemberExpression();
            
            var obj = Recurse(mExpr);
            var p = (PropertyInfo)mExpr.Member;
            p.SetValue(obj, value); 
            return this;
        }

        public DataBuilder<T> SetValue<T2>(Expression<Func<T, T2>> expression, DataBuilder<T2> valueBuilder)
        {
            return SetValue(expression, valueBuilder.Build());
        }

        public T Build()
        {
            return _data;
        }

        //This method recurses from the right side of the expression to the left, once it reaches the first property it will either instantiate
        //the property or get the existing one and pass it back up the call stack as the parent so that values can be set on it.
        private object Recurse(MemberExpression expr)
        {
            var pExpr = expr.Expression.GetMemberExpression();
            if (pExpr == null) return _data;

            var parent = Recurse(pExpr);

            var childInfo = (PropertyInfo) pExpr.Member;
            var child = childInfo.GetValue(parent);
            if (child != null) return child;

            child = Activator.CreateInstance(childInfo.PropertyType);
            childInfo.SetValue(parent, child);

            return child;
        }
    }
