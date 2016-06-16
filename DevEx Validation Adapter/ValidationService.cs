using DevExpress.Mvvm.UI;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Eka.Common.Wpf.Behaviors
{
    public class ValidationService : ServiceBase, IValidationService
    {
        public static readonly DependencyProperty HasValidationErrorProperty =
            DependencyProperty.Register("HasValidationError", typeof(bool), typeof(ValidationService), new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });

        private readonly HashSet<Tuple<int, string>> _invalidFields;

        public ValidationService()
        {
            _invalidFields = new HashSet<Tuple<int, string>>();
        }

        public bool HasValidationError
        {
            get { return (bool)GetValue(HasValidationErrorProperty); }
            set { SetValue(HasValidationErrorProperty, value); }
        }

        public void UpdateValidStatus(string propertyName, bool isValid)
        {
            UpdateValidStatus(new Tuple<int, string>(-1, propertyName), isValid);
        }

        public void UpdateValidStatus(int row, string propertyName, bool isValid)
        {
            UpdateValidStatus(new Tuple<int, string>(row, propertyName), isValid);
        }

        private void UpdateValidStatus(Tuple<int, string> tuple, bool isValid)
        {
            if (!isValid && !_invalidFields.Contains(tuple))
            {
                _invalidFields.Add(tuple);
            }
            else if (isValid && _invalidFields.Contains(tuple))
            {
                _invalidFields.Remove(tuple);
            }

            HasValidationError = _invalidFields.Any();
        }
    }
}