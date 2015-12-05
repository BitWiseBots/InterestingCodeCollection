using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InterestingCodeCollection
{
    public abstract class SmartEnum<T> where T : SmartEnum<T>
    {
        protected SmartEnum(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }

        public static IEnumerable<T> All
        {
            get
            {
                return typeof(T)
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Where(property => property.GetValue(typeof(T)) is T)
                    .Select(property => property.GetValue(typeof(T)) as T);
            }
        }
    }
}