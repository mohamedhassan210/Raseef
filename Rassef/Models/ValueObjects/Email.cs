using System.Text.RegularExpressions;

namespace Rassef.Models.ValueObjects
{
    public record Email
    {
        [Key]
        public Guid Id { get; set;  }
        public string Value { get; set; } = string.Empty;
        public Email(string value)
        {
            string pattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
            if (!Regex.IsMatch(value, pattern))
            {
                throw new DomainException("Invalid Email");
            }
            Value = value;
        }
        public static Email Create(string value)
        {
            return new Email(value);
        }

        public override string ToString() => Value;
    }
}
