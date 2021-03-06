﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TeamCompositionBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.Windows.Controls;
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for TeamCompositionBrowser XAML
    /// </summary>
    [PanelViewExport(RegionNames.EditorPanel)]
    public partial class TeamCompositionBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCompositionBrowser"/> class.
        /// </summary>
        public TeamCompositionBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCompositionBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public TeamCompositionBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
