﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceExport.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for DataSourceExport.xaml
    /// </summary>
    [DialogViewExport("DataSourceExport", "The export Dal dialog")]
    public partial class DataSourceExport : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceExport"/> class.
        /// </summary>
        public DataSourceExport()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceExport"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public DataSourceExport(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}