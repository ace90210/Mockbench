using System.ComponentModel.DataAnnotations;

namespace Mockbench.Shared.Helper
{
    public class OptionalUrlAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return true;
            if (string.IsNullOrWhiteSpace(value.ToString())) return true;

            var urlAttr = new UrlAttribute();
            return urlAttr.IsValid(value);
        }
    }

}
