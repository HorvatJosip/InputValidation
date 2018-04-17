using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace InputValidation
{
    /// <summary>
    /// Base class for view models. Provides the ability for properties to be validated and
    /// raise the <see cref="PropertyChanged"/> event.
    /// </summary>
    abstract class BaseViewModel : IDataErrorInfo, INotifyPropertyChanged
    {
        /// <summary>
        /// Contains validation data for a property.
        /// <para>Example: key is property named Number, value is a combination of validator (that checks if it
        /// is a positive number) and a string that represents an error message that is displayed if the
        /// validation fails.</para>
        /// </summary>
        private IDictionary<string, (Func<bool>, string)> errorData = new Dictionary<string, (Func<bool>, string)>();

        private string error;

        #region IDataErrorInfo Members

        /// <summary>
        /// Runs validation for a specified property.
        /// If property validation fails, an error message is returned (otherwise null).
        /// </summary>
        /// <param name="columnName">Name of the property to validate.</param>
        public string this[string columnName]
        {
            get
            {
                //If a property exists in the collection
                if (errorData.ContainsKey(columnName))
                {
                    //Extract a validator and the error message if the validator fails
                    (Func<bool> validator, string errorMessage) = errorData[columnName];

                    //if validator succeeds, return null, which means there is no error
                    if (validator?.Invoke() == true)
                    {
                        Error = "";
                        return null;
                    }

                    //otherwise, return the error message
                    else
                    {
                        Error = errorMessage;
                        return errorMessage;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the last validation error
        /// </summary>
        public string Error { get => error; private set => SetValue(ref error, value); }



        #endregion

        #region PropertyChanged Members

        /// <summary>
        /// Fired when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public BaseViewModel()
        {
            InitializeValidators();

            CheckValidators();
        }

        public BaseViewModel(params Func<bool>[] validators)
        {
            int count = 0;

            foreach (var property in GetType().GetProperties())
            {
                var attribute = property.GetCustomAttribute<ValidatedAttribute>();
                if (attribute != null)
                    AddValidator(property.Name, validators[count++]);
            }

            CheckValidators();
        }

        private void CheckValidators()
        {
            int numDecoratedProperties = GetType().GetProperties().SelectMany(prop => prop.GetCustomAttributes<ValidatedAttribute>()).Count();

            if (errorData.Count != numDecoratedProperties)
                throw new ArgumentOutOfRangeException("Validators", "Number of validators must match" +
                    " the number of properties decorated with " + nameof(ValidatedAttribute));
        }

        /// <summary>
        /// Sets the value of a property and raises <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="property">Property that has its value changed</param>
        /// <param name="value">New value for the property</param>
        protected void SetValue<T>(ref T property, object newValue, [CallerMemberName]string propertyName = null)
        {
            if (newValue is T value && !Equals(property, value))
            {
                property = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Adds a validator for a property
        /// </summary>
        /// <param name="propertyName">Name of the property (use nameof())</param>
        /// <param name="validator">Checks if the property is valid</param>
        /// <param name="errorMessage"></param>
        protected void AddValidator(string propertyName, Func<bool> validator)
        {
            if (propertyName == null || validator == null)
                throw new ArgumentNullException();

            foreach (var property in GetType().GetProperties())
                if (property.Name == propertyName)
                {
                    var attribute = property.GetCustomAttribute<ValidatedAttribute>();
                    if (attribute != null)
                    {
                        errorData.Add(propertyName, (validator, attribute.ErrorMessage));
                        return;
                    }
                }

            throw new Exception($"Property with name \"{propertyName}\" wasn't found.");
        }

        protected virtual void InitializeValidators() { }
    }
}
