﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingParameter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Parameters
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// class to be used to define dynamic report parameters in the Code editor of <see cref="Views.ReportDesigner"/>.
    /// </summary>
    public class ReportingParameter : IReportingParameter
    {
        /// <summary>
        /// The name prefix that every report parameter gets in the report designer.
        /// </summary>
        public const string NamePrefix = "dyn_";

        /// <summary>
        /// Gets or sets the name of the <see cref="IReportingParameter"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the "calculated" parameter name to be used in the <see cref="Views.ReportDesigner"/>.
        /// </summary>
        public string ParameterName => $"{NamePrefix}{this.Name}";

        /// <summary>
        /// Gets or sets the <see cref="Type"/> of the parameter.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the report parameter.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey,TValue}"/> that contains lookup values for a drop down report parameter.
        /// </summary>
        public Dictionary<object, string> LookUpValues { get; } = new Dictionary<object, string>();

        /// <summary>
        /// Gets or sets the default value of the report parameter.
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the filterexpression to be used for this report parameter.
        /// </summary>
        public string FilterExpression { get; set; }

        /// <summary>
        /// Adds a lookup value to the <see cref="LookUpValues"/> property.
        /// </summary>
        /// <param name="value">
        /// The value. Could be any data type.
        /// </param>
        /// <param name="displayValue">
        /// The display value in the report designer.
        /// </param>
        /// <returns>
        /// The <see cref="IReportingParameter"/>.
        /// </returns>
        public IReportingParameter AddLookupValue(object value, string displayValue)
        {
            this.LookUpValues.Add(value, displayValue);

            return this;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ReportingParameter"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the parameter.
        /// </param>
        /// <param name="type">
        /// The data <see cref="Type"/> of the parameter.
        /// </param>
        /// <param name="defaultValue">
        /// The default value of the parameter.
        /// </param>
        /// <param name="filterExpression">
        /// A filter expression to be used on the report's DataSource.
        /// </param>
        public ReportingParameter(string name, Type type, object defaultValue, string filterExpression = "")
        {
            this.Name = name;
            this.Type = type;
            this.DefaultValue = defaultValue;
            this.FilterExpression = filterExpression;
        }
    }
}
