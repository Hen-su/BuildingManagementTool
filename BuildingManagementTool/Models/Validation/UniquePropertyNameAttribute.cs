using System.ComponentModel.DataAnnotations;

namespace BuildingManagementTool.Models.Validation
{
    public class UniquePropertyNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dbContext = (BuildingManagementToolDbContext)validationContext.GetService(typeof(BuildingManagementToolDbContext));
            var propertyExists = dbContext.Properties.Any(p => p.PropertyName == value.ToString());

            if (propertyExists) 
            {
                return new ValidationResult("A property with the same name already exists");
            }
            return ValidationResult.Success;
        }
    }
}
