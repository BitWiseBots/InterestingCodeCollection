using System.Collections.Generic;
using System.Linq;

namespace Eka.Common.Core.Models
{
    public static class UpdateableModelExtensions
    {
        public static bool HasUnsavedChanges<T>(this IEnumerable<T> collection) where T : IUpdateableModel
        {
            return collection != null && collection.Any(x => x.HasUnsavedChanges());
        }

        public static void ResetItemStates<T>(this IList<T> collection) where T : IUpdateableModel
        {
            var itemsToRemove = collection.Where(x => x.State == ModelState.New).ToList();
            foreach (var item in itemsToRemove)
            {
                collection.Remove(item);
            }

            var itemsToReset = collection.Where(x => x.State != ModelState.New).ToList();
            foreach (var item in itemsToReset)
            {
                item.ResetState();
            }
        }
    }
}