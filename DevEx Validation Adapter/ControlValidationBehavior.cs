using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Editors.Validation;
using FluentValidation.Results;
using NHibernate.Util;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Eka.Common.Wpf.Behaviors
{
    [ExcludeFromCodeCoverage]
    public class ControlValidationBehavior : BaseValidationBehavior<BaseEdit>
    {
        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register("DataContext", typeof(object), typeof(ControlValidationBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(ControlValidationBehavior), new PropertyMetadata(null));

        public object DataContext
        {
            get { return GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        protected override bool AttachOnFactorySet => false;

        protected override void AttachEvent()
        {
            AssociatedObject.LostFocus += OnLostFocus;
        }

        private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            if (DataContext == null) throw new Exception("The DataContext DependencyProperty must be set.");

            var modelType = DataContext.GetType();
            SetValidator(modelType);

            var lambda = GetPropertyExpression(modelType, PropertyName);

            var genValidateCell = ValidateMethodInfo.MakeGenericMethod(modelType);

            var result = (ValidationResult)genValidateCell.Invoke(this, new[] { DataContext, lambda });

            ValidationService?.UpdateValidStatus(PropertyName, result.IsValid);

            if (!result.IsValid)
            {
                var edit = sender as BaseEdit;
                BaseEditHelper.SetValidationError(edit, new BaseValidationError(result.Errors.First()));
            }
        }

        protected override void DetachEvent()
        {
            AssociatedObject.LostFocus -= OnLostFocus;
        }

        protected override void ConfigureObject()
        {
            AssociatedObject.ValidateOnTextInput = false;
            AssociatedObject.InvalidValueBehavior = InvalidValueBehavior.AllowLeaveEditor;
        }
    }
}