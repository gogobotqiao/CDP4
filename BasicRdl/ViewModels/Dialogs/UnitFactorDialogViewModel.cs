﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitFactorDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="UnitFactorDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="UnitFactor"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.UnitFactor)]
    public class UnitFactorDialogViewModel : CDP4CommonView.UnitFactorDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The selected <see cref="ReferenceDataLibrary"/>
        /// </summary>
        private ReferenceDataLibrary selectedRdl;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitFactorDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public UnitFactorDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitFactorDialogViewModel"/> class.
        /// </summary>
        /// <param name="unitFactor">
        /// The <see cref="UnitFactor"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="UnitFactorDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="UnitFactorDialogViewModel"/> performs
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
        public UnitFactorDialogViewModel(UnitFactor unitFactor, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(unitFactor, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Initializes the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.selectedRdl = this.ChainOfContainer.OfType<ReferenceDataLibrary>().SingleOrDefault();
            if (this.selectedRdl == null)
            {
                throw new InvalidOperationException("There is no Reference Data Library in the chain of selected container.");
            }

            this.WhenAnyValue(vm => vm.SelectedUnit).Where(x => x != null).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Populate the <see cref="UnitFactorDialogViewModel.PossibleUnit"/> property
        /// </summary>
        protected override void PopulatePossibleUnit()
        {
            base.PopulatePossibleUnit();
            var units = new List<MeasurementUnit>(this.selectedRdl.Unit);
            foreach (var rdl in this.selectedRdl.GetRequiredRdls())
            {
                units.AddRange(rdl.Unit);
            }

            this.PossibleUnit.AddRange(units.OrderBy(x => x.ShortName));
        }

        /// <summary>
        /// Update the <see cref="UnitFactorDialogViewModel.OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedUnit != null;
        }
    }
}
