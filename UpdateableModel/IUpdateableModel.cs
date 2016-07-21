using System.ComponentModel;

namespace Eka.Common.Core.Models
{
    public interface IUpdateableModel : INotifyPropertyChanged
    {
        ModelState State { get; set; }
        bool HasUnsavedChanges();
        void ResetState();
    }
}