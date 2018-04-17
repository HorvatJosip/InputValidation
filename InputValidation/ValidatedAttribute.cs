using System;

namespace InputValidation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    class ValidatedAttribute : Attribute
    {
        /// <summary>
        /// Message used to display when the validator fails
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Creates an instance of <see cref="ValidatedAttribute"/> that is used to
        /// mark the property that it gets validated and provides the error message
        /// if the validation fails
        /// </summary>
        /// <param name="errorMessage"></param>
        public ValidatedAttribute(string errorMessage) => ErrorMessage = errorMessage;
    }
}