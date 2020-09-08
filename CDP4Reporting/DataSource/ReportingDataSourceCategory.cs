﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingDataSourceCategory.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.DataSource
{
    using System.Data;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Abstract base class from which all category columns for a <see cref="ReportingDataSourceRow"/> need to derive.
    /// </summary>
    internal abstract class ReportingDataSourceCategory<T> : ReportingDataSourceColumn<T> where T : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// The associated <see cref="Category"/> short name.
        /// </summary>
        internal readonly string ShortName;

        /// <summary>
        /// Flag indicating whether the associated <see cref="Category"/> is present.
        /// </summary>
        protected bool Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDataSourceCategory{T}"/> class.
        /// </summary>
        protected ReportingDataSourceCategory()
        {
            this.ShortName = GetParameterAttribute(this.GetType())?.ShortName;
        }

        /// <summary>
        /// Initializes a reported category column based on the corresponding <see cref="ElementBase"/>
        /// within the associated <see cref="ReportingDataSourceNode{T}"/>.
        /// </summary>
        /// <param name="node">
        /// The associated <see cref="ReportingDataSourceNode{T}"/>.
        /// </param>
        internal override void Initialize(ReportingDataSourceNode<T> node)
        {
            this.Node = node;

            var definitionCategory = this.Node.ElementDefinition.Category
                .SingleOrDefault(x => x.ShortName == this.ShortName);

            if (definitionCategory != null)
            {
                this.Value = true;
            }

            var usageCategory = this.Node.ElementUsage?.Category
                .SingleOrDefault(x => x.ShortName == this.ShortName);

            if (usageCategory != null)
            {
                this.Value = true;
            }
        }

        /// <summary>
        /// Populates with data the <see cref="DataTable.Columns"/> associated with this object
        /// in the given <paramref name="row"/>.
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable"/> to which the <paramref name="row"/> belongs to.
        /// </param>
        /// <param name="row">
        /// The <see cref="DataRow"/> to be populated.
        /// </param>
        internal override void Populate(DataTable table, DataRow row)
        {
            // currently intentionally left empty
            // we could populate the value here, pending on what column name to use
            // row[?] = this.Value;
        }
    }
}
