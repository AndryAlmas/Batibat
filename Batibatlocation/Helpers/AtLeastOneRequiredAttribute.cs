using System;
using System.ComponentModel.DataAnnotations;

namespace Batibatlocation.Helpers
{
    public class AtLeastOneRequiredAttribute : ValidationAttribute
    {
        private readonly string[] _properties;

        public AtLeastOneRequiredAttribute(params string[] properties)
        {
            _properties = properties;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            foreach (var property in _properties)
            {
                var propertyInfo = validationContext.ObjectType.GetProperty(property);
                if (propertyInfo == null)
                {
                    return new ValidationResult($"Propriété inconnue : {property}");
                }

                var propertyValue = propertyInfo.GetValue(validationContext.ObjectInstance, null);
                if (propertyValue != null && !string.IsNullOrEmpty(propertyValue.ToString()))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage ?? "Au moins une des propriétés doit être renseignée.");
        }
    }
}