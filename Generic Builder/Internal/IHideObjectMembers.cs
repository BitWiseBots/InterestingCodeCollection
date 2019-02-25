using System;
using System.ComponentModel;

namespace InterestingCodeCollection.GenericBuilder.Internal
{
    /// <summary>
    /// This interface is used to remove the System.Object members from intellisense to improve intellisense readability.
    /// </summary>
    /// <remarks>
    /// If using Resharper, then the setting 'Resharper' -> 'Options' -> 'Intellisense' -> 'Completion Appearance' -> `Filter members by [EditorBrowsable] attribute`
    /// should be enabled to support this interface.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IHideObjectMembers
    {
        /// <summary>
        /// Hides <see cref="Object.GetType()"/> from intellisense.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        /// <summary>
        /// Hides <see cref="Object.GetHashCode()"/> from intellisense.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        /// <summary>
        /// Hides <see cref="Object.ToString()"/> from intellisense.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();

        /// <summary>
        /// Hides <see cref="object.Equals(object)"/> from intellisense.
        /// </summary>
        /// <param name="obj"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);
    }
}
