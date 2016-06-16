using DevExpress.Xpf.Grid;
using DevExpress.XtraEditors.DXErrorProvider;
using FluentValidation.Results;
using NHibernate.Util;
using System.Diagnostics.CodeAnalysis;

namespace Eka.Common.Wpf.Behaviors
{
    [ExcludeFromCodeCoverage]
    public class TableViewValidationBehavior : BaseValidationBehavior<TableView>
    {
        protected override bool AttachOnFactorySet => true;

        protected override void ConfigureObject()
        {
            AssociatedObject.AllowLeaveInvalidEditor = true;
        }

        protected override void AttachEvent()
        {
            AssociatedObject.ValidateCell += OnValidateCell;
        }

        protected override void DetachEvent()
        {
            AssociatedObject.ValidateCell -= OnValidateCell;
        }

        protected virtual void OnValidateCell(object sender, GridCellValidationEventArgs args)
        {
            var row = args.Row;
            if (row == null || args.RowHandle == DataControlBase.NewItemRowHandle) return;

            var modelType = row.GetType();
            SetValidator(modelType);

            var propertyName = GetPropertyName(args.Column.FieldName);

            var lambda = GetPropertyExpression(modelType, propertyName);

            var genValidateCell = ValidateMethodInfo.MakeGenericMethod(modelType);

            var result = (ValidationResult)genValidateCell.Invoke(this, new[] { row, lambda });

            ValidationService?.UpdateValidStatus(args.RowHandle, propertyName, result.IsValid);

            if (!result.IsValid)
            {
                args.IsValid = false;
                args.ErrorType = ErrorType.Critical;
                args.ErrorContent = result.Errors.First();
            }
        }

        private string GetPropertyName(string columnFieldName)
        {
            const string rowPath = "RowData.Row.";

            return columnFieldName.Remove(0, rowPath.Length);
        }
    }
}