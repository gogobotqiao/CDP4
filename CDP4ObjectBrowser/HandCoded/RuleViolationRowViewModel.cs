﻿// -------------------------------------------------------------------------------------------------
// <copyright file="RuleRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="RuleViolation"/>
    /// </summary>
    public partial class RuleViolationRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Description;
            this.ShortName = string.Empty;
        }
    }
}
