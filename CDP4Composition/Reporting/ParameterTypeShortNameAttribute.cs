namespace CDP4Composition.Reporting
{
    using System;

    public class ParameterTypeShortNameAttribute : Attribute
    {
        public ParameterTypeShortNameAttribute(string shortName)
        {
            this.ShortName = shortName;
        }

        public string ShortName { get; }
    }
}
