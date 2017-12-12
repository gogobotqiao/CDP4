﻿// -------------------------------------------------------------------------------------------------
// <copyright file="UnitFactorDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="UnitFactor"/>
    /// </summary>
    public partial class UnitFactorDialogViewModel : DialogViewModelBase<UnitFactor>
    {
        /// <summary>
        /// Backing field for <see cref="Exponent"/>
        /// </summary>
        private string exponent;

        /// <summary>
        /// Backing field for <see cref="SelectedUnit"/>
        /// </summary>
        private MeasurementUnit selectedUnit;


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
        /// Initializes a new instance of the <see cref="UnitFactorDialogViewModel"/> class
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
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
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
        public UnitFactorDialogViewModel(UnitFactor unitFactor, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(unitFactor, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as DerivedUnit;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type DerivedUnit",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Exponent
        /// </summary>
        public virtual string Exponent
        {
            get { return this.exponent; }
            set { this.RaiseAndSetIfChanged(ref this.exponent, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedUnit
        /// </summary>
        public virtual MeasurementUnit SelectedUnit
        {
            get { return this.selectedUnit; }
            set { this.RaiseAndSetIfChanged(ref this.selectedUnit, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="MeasurementUnit"/>s for <see cref="SelectedUnit"/>
        /// </summary>
        public ReactiveList<MeasurementUnit> PossibleUnit { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedUnit"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedUnitCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedUnitCommand = this.WhenAny(vm => vm.SelectedUnit, v => v.Value != null);
            this.InspectSelectedUnitCommand = ReactiveCommand.Create(canExecuteInspectSelectedUnitCommand);
            this.InspectSelectedUnitCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedUnit));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Exponent = this.Exponent;
            clone.Unit = this.SelectedUnit;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleUnit = new ReactiveList<MeasurementUnit>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Exponent = this.Thing.Exponent;
            this.SelectedUnit = this.Thing.Unit;
            this.PopulatePossibleUnit();
        }

        /// <summary>
        /// Populates the <see cref="PossibleUnit"/> property
        /// </summary>
        protected virtual void PopulatePossibleUnit()
        {
            this.PossibleUnit.Clear();
        }
    }
}
