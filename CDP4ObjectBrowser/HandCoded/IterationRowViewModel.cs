﻿// -------------------------------------------------------------------------------------------------
// <copyright file="IterationRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="IterationRowViewModel"/>
    /// </summary>
    public partial class IterationRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = "Iteration_"+this.Thing.IterationSetup.IterationNumber.ToString();
            this.ShortName = this.Thing.IterationSetup.Description;
        }
    }
}
