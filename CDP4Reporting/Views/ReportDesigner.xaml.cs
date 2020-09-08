﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportDesigner.xaml.cs" company="RHEA System S.A.">
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

namespace CDP4Reporting.Views
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Composition;
    using CDP4Composition.Attributes;

    using CDP4Reporting.ViewModels;

    using DevExpress.DataAccess.ObjectBinding;
    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Reports.UserDesigner;
    using DevExpress.XtraReports.Security;

    /// <summary>
    /// Interaction logic for ReportDesigner.xaml
    /// </summary>
    [PanelViewExport(RegionNames.EditorPanel)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class ReportDesigner : IPanelView
    {
        /// <summary>
        /// The <see cref="Task"/> that executes the compile command
        /// </summary>
        private Task compileTask;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> used to stop a compile task
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// The <see cref="CancellationToken"/> generated by the cancellationTokenSource in order to stop compile task
        /// </summary>
        protected CancellationToken cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesigner"/> class.
        /// </summary>
        public ReportDesigner()
        {
            this.InitializeComponent();

            ScriptPermissionManager.GlobalInstance = new ScriptPermissionManager(ExecutionMode.Deny);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesigner"/> class.
        /// </summary>
        /// <param name="initializeComponent">A value indicating whether the contained Components shall be loaded</param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ReportDesigner(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();

                this.cancellationTokenSource = new CancellationTokenSource();
                this.cancellationToken = this.cancellationTokenSource.Token;

                this.reportDesigner.ActiveDocumentChanged += this.ReportDesigner_ActiveDocumentChanged;
                this.textEditor.TextChanged += this.TextEditor_TextChanged;
            }
        }

        /// <summary>
        /// Trigger text changed editor event
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="EventArgs"/></param>
        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            var viewModel = this.DataContext as ReportDesignerViewModel;

            if (viewModel != null && !viewModel.IsAutoBuildEnabled)
            {
                return;
            }

            if (this.compileTask != null && this.compileTask.Status.Equals(TaskStatus.Running))
            {
                this.cancellationTokenSource.Cancel();
            }

            this.compileTask = Task.Run(async delegate
            {
                this.cancellationToken.ThrowIfCancellationRequested();

                Thread.Sleep(5 * 1000);

                if (viewModel != null)
                {
                    await viewModel.AutomaticBuildScript();
                }
            }, this.cancellationToken);
        }

        // TODO #480
        ///// <summary>
        ///// Dummy class for report row respresentation
        ///// </summary>
        //private class RowRepresentation : ReportingDataSourceRowRepresentation
        //{
        //}

        /// <summary>
        /// Trigger active document changed event, when a new report was loaded
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private void ReportDesigner_ActiveDocumentChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            var localReport = ((ReportDesignerDocument)e.NewValue).Report;
            var viewModel = this.DataContext as ReportDesignerViewModel;

            // Remove previous data sources attached
            foreach (var component in localReport.ComponentStorage.OfType<ObjectDataSource>().ToList())
            {
                localReport.ComponentStorage.Remove(component);
                localReport.Container?.Remove(component);
            }

            // Set new datasource
            if (viewModel != null && (viewModel.Thing == null || viewModel.BuildResult == null))
            {
                return;
            }

            if (viewModel.BuildResult.Errors.Count != 0)
            {
                return;
            }

            // TODO #480
            //var editorFullClassName = "CDP4Reporting.MassBudgetDataSource";
            //var dataSourceName = "MassBudgetDataSource";

            //var inst = viewModel.BuildResult.CompiledAssembly.CreateInstance(editorFullClassName) as IReportingDataSource<RowRepresentation>;

            //var dataSource = new ObjectDataSource
            //{
            //    DataSource = inst?.CreateDataSource(viewModel.Thing),
            //    Name = dataSourceName
            //};

            //this.reportDesigner.ActiveDocument.Report.DataSource = dataSource;
        }

        /// <summary>
        /// Trigger context menu action
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/></param>
        private void BarButtonItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            switch (e.Item.Content.ToString())
            {
                case "Copy":
                    this.OutputTextBox.Copy();
                    break;

                case "Clear":
                    ((ReportDesignerViewModel) this.DataContext).Output = string.Empty;
                    break;
            }
        }
    }
}