using DevExpress.Mvvm.UI.Interactivity;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using Expression = System.Linq.Expressions.Expression;

namespace Eka.Common.Wpf.Behaviors
{
    public abstract class BaseValidationBehavior<T> : Behavior<T> where T : DependencyObject
    {
        // ReSharper disable once StaticMemberInGenericType
        public static readonly DependencyProperty ValidatorFactoryProperty =
            DependencyProperty.Register("ValidatorFactory", typeof(IValidatorFactory), typeof(BaseValidationBehavior<T>), new FrameworkPropertyMetadata(ValidatorFactoryChangedCallback));

        private static void ValidatorFactoryChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var behavior = (BaseValidationBehavior<T>)dependencyObject;

            if (behavior.AttachOnFactorySet)
            {
                behavior.AttachEvent();
            }
        }

        // ReSharper disable once StaticMemberInGenericType
        public static readonly DependencyProperty ValidationServiceProperty =
            DependencyProperty.Register("ValidationService", typeof(IValidationService), typeof(BaseValidationBehavior<T>), new PropertyMetadata(null));

        protected IValidator Validator;

        // ReSharper disable once StaticMemberInGenericType
        protected static readonly MethodInfo ValidateMethodInfo;

        static BaseValidationBehavior()
        {
            ValidateMethodInfo = typeof(BaseValidationBehavior<T>).GetMethod("Validate", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public IValidatorFactory ValidatorFactory
        {
            get { return (IValidatorFactory)GetValue(ValidatorFactoryProperty); }
            set { SetValue(ValidatorFactoryProperty, value); }
        }

        public IValidationService ValidationService
        {
            get { return (IValidationService)GetValue(ValidationServiceProperty); }
            set { SetValue(ValidationServiceProperty, value); }
        }

        protected abstract bool AttachOnFactorySet { get; }

        protected abstract void AttachEvent();
        protected abstract void DetachEvent();
        protected abstract void ConfigureObject();

        protected sealed override void OnAttached()
        {
            base.OnAttached();

            if (!AttachOnFactorySet)
            {
                AttachEvent();
            }

            ConfigureObject();
        }

        protected sealed override void OnDetaching()
        {
            DetachEvent();
            base.OnDetaching();
        }

        protected LambdaExpression GetPropertyExpression(Type modelType, string propertyName)
        {
            var param = Expression.Parameter(modelType);
            var mbrExpr = Expression.Property(param, propertyName);
            var unaryExpr = Expression.Convert(mbrExpr, typeof(object));
            return Expression.Lambda(unaryExpr, param);
        }

        protected void SetValidator(Type modelType)
        {
            if (Validator != null) return;

            try
            {
                Validator = ValidatorFactory.GetValidator(modelType);

            }
            catch (Exception)
            {

                throw new Exception($"Validator was not found for model type: {modelType.Name}");
            }
        }

        protected ValidationResult Validate<TModel>(TModel model, Expression<Func<TModel, object>> propertyExpression)
        {
            return ((IValidator<TModel>)Validator).Validate(model, propertyExpression);
        }
    }
}