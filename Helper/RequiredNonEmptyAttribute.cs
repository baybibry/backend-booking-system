using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Helper;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class RequiredNonEmptyAttribute : ValidationAttribute
{
    public RequiredNonEmptyAttribute()
    {
        ErrorMessage = "The {0} field is required and cannot be empty or whitespace.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return false;
        if (value is string str) return !string.IsNullOrWhiteSpace(str);
        return true;
    }
}
