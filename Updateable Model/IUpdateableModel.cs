using System.ComponentModel;

namespace InterestingCodeCollection.UpdateableModel
{
    public interface IUpdateableModel : INotifyPropertyChanged
    {
        ModelState State { get; set; }
        bool HasUnsavedChanges();
        void ResetState();
    }
}