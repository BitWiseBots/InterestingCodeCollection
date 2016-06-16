namespace Eka.Common.Wpf.Behaviors
{
    public interface IValidationService
    {
        bool HasValidationError { get; }
        void UpdateValidStatus(string propertyName, bool isValid);
        void UpdateValidStatus(int row, string propertyName, bool isValid);
    }
}