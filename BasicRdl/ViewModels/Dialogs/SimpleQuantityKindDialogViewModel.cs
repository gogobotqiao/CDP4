﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleQuantityKindDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="SimpleQuantityKindDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="SimpleQuantityKind"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.SimpleQuantityKind)]
    public class SimpleQuantityKindDialogViewModel : CDP4CommonView.SimpleQuantityKindDialogViewModel, IThingDialogViewModel
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleQuantityKindDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public SimpleQuantityKindDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleQuantityKindDialogViewModel"/> class.
        /// </summary>
        /// <param name="simpleQuantityKind">
        /// The Simple quantity Kind.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="SimpleQuantityKindDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="SimpleQuantityKindDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        /// <exception cref="ArgumentException">
        /// The container must be of type <see cref="ReferenceDataLibrary"/>.
        /// </exception>
        public SimpleQuantityKindDialogViewModel(SimpleQuantityKind simpleQuantityKind, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(simpleQuantityKind, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.PopulatePossiblePossibleScales());
            this.WhenAnyValue(vm => vm.SelectedDefaultScale).Subscribe(_ => this.UpdateOkCanExecute());
        }
        #endregion

        #region Properties and Commands

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDefaultScale"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedScaleCommand { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the <see cref="OkCanExecute"/> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.PossibleScale.Any() && this.SelectedDefaultScale != null;
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulatePossiblePossibleScales();
        }

        /// <summary>
        /// The populate all possible scales.
        /// </summary>
        private void PopulatePossiblePossibleScales()
        {
            this.PossiblePossibleScale.Clear();
            var containerRdl = this.Container as ReferenceDataLibrary;
            if (containerRdl != null)
            {
                var allScales = new List<MeasurementScale>(containerRdl.Scale);
                allScales.AddRange(containerRdl.GetRequiredRdls().SelectMany(rdl => rdl.Scale));
                this.PossiblePossibleScale.AddRange(allScales.OrderBy(c => c.ShortName));
            }
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedDefaultScaleCommand = this.WhenAny(vm => vm.SelectedDefaultScale, v => v.Value != null);
            this.InspectSelectedScaleCommand = ReactiveCommand.Create(canExecuteInspectSelectedDefaultScaleCommand);
            this.InspectSelectedScaleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefaultScale));
        }

        #endregion
    }
}