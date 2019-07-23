namespace Syncer.Entities
{
    using System.ComponentModel.DataAnnotations;
    public sealed class RequiredEnumAttribute : ValidationAttribute
    {
        /// <summary>
        /// The default enum value.
        /// </summary>
        private readonly int defaultEnumValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredEnumAttribute"/> class.
        /// </summary>
        /// <param name="defaultEnumValue">The default enum value.</param>
        public RequiredEnumAttribute(int defaultEnumValue)
        {
            this.defaultEnumValue = defaultEnumValue;
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="value">The value of the object to validate.</param>
        /// <returns>
        /// true if the specified value is valid; otherwise, false.
        /// </returns>
        public override bool IsValid(object value)
        {
            if ((int)value == this.defaultEnumValue)
            {
                return false;
            }

            return true;
        }
    }
}