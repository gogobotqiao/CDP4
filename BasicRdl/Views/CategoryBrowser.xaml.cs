﻿// -------------------------------------------------------------------------------------------------
// <copyright file="CategoryBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Grid;
    using NLog;

    /// <summary>
    /// Interaction logic for <see cref="CategoryBrowser"/>
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class CategoryBrowser : IPanelView, IPanelFilterableDataGridView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryBrowser"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is called by MEF
        /// </remarks>
        public CategoryBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementUnitsBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public CategoryBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
                this.FilterableControl = this.CategoriesGridControl;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataControlBase"/> that is to be set up for filtering service.
        /// </summary>
        public DataControlBase FilterableControl { get; private set; }
    }
}
