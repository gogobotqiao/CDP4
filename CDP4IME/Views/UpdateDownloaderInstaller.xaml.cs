﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginInstaller.xaml.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-Plugin Installer Community Edition. 
//    The CDP4-Plugin Installer Community Edition is the RHEA Plugin Installer for the CDP4-IME Community Edition.
//
//    The CDP4-Plugin Installer Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-Plugin Installer Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Views
{
    using System.Windows;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for PluginInstaller.xaml
    /// </summary>
    [DialogViewExport(nameof(UpdateDownloaderInstaller), "Update Downloader and Installer")] 
    public partial class UpdateDownloaderInstaller : Window, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDownloaderInstaller"/> class
        /// </summary>
        public UpdateDownloaderInstaller()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDownloaderInstaller"/> class
        /// </summary>
        /// <param name="initializeComponent">Assert whether the view components should be initialized</param>
        public UpdateDownloaderInstaller(bool initializeComponent = true)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
