﻿// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Services;

    using NLog;

    /// <summary>
    /// Interaction logic for Organization Browser
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class SiteRdlBrowser : IPanelView
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlBrowser"/> class
        /// </summary>
        public SiteRdlBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public SiteRdlBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
                FilterStringService.FilterString.AddGridControl(this.SiteRdlsGridControl);
            }
        }
    }
}
