﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="ParameterOverride"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ParameterOverride)]
    public class ParameterOverrideDialogViewModel : CDP4CommonView.ParameterOverrideDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsOwnerReadonly"/>
        /// </summary>
        private bool isOwnerReadonly;

        /// <summary>
        /// Backing field for the <see cref="ModelCode"/> property.
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterOverrideDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideDialogViewModel"/> class
        /// </summary>
        /// <param name="parameterOverride">
        /// The parameterOverride.
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
        /// The Container <see cref="Thing"/> of the created <see cref="MultiRelationshipRule"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ParameterOverrideDialogViewModel(ParameterOverride parameterOverride, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(parameterOverride, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.SelectedOwner).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.ValueSet).Subscribe(_ => this.PopulateValueSet());

            this.IsNameVisible = this.Thing.ParameterType is CompoundParameterType || this.Thing.IsOptionDependent || this.Thing.StateDependence != null;
            this.CheckValueValidation();
        }
        
        /// <summary>
        /// Gets a value indicating if the scale shall be made visible
        /// </summary>
        public bool IsScaleVisible => this.Thing.ParameterType is CompoundParameterType;

        /// <summary>
        /// Gets a value indicating whether is owner visible.
        /// </summary>
        public bool IsOwnerVisible
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is name visible.
        /// </summary>
        public bool IsNameVisible { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the owner is read-only
        /// </summary>
        public bool IsOwnerReadonly
        {
            get { return this.isOwnerReadonly; }
            private set { this.RaiseAndSetIfChanged(ref this.isOwnerReadonly, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterOverrideValueSet"/>
        /// </summary>
        public DisposableReactiveList<Dialogs.ParameterOverrideRowViewModel> ValueSet { get; protected set; }

        /// <summary>
        /// Gets the reference parameter.
        /// </summary>
        public string ReferenceParameter
        {
            get
            {
                return string.Format("{0} ({1})", this.SelectedParameter.ParameterType.Name, ((ElementDefinition)this.SelectedParameter.Container).Name);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the parameter override is state dependent.
        /// </summary>
        public bool IsStateDependent
        {
            get
            {
                return this.SelectedParameter.StateDependence != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the parameter is option dependent.
        /// </summary>
        public bool IsOptionDependent
        {
            get { return this.SelectedParameter.IsOptionDependent; }
        }
        
        /// <summary>
        /// Gets or sets a value that represents the ModelCode of the current <see cref="ElementDefinition"/>
        /// </summary>
        public string ModelCode
        {
            get { return this.modelCode; }
            set { this.RaiseAndSetIfChanged(ref this.modelCode, value); }
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.ValueSet = new DisposableReactiveList<Dialogs.ParameterOverrideRowViewModel>();
            this.IsOwnerReadonly = !this.Thing.Parameter.AllowDifferentOwnerOfOverride;
        }

         /// <summary>
         /// Update the properties
         /// </summary>
         protected override void UpdateProperties()
         {
             base.UpdateProperties();
             this.SelectedParameterType = this.Thing.Parameter.ParameterType;
             this.PopulateValueSet();

             this.ModelCode = this.Thing.ModelCode();
         }

        /// <summary>
        /// Populates the <see cref="ValueSet"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected void PopulateValueSet()
        {
            this.ValueSet.ClearAndDispose();
            var row = new Dialogs.ParameterOverrideRowViewModel(this.Thing, this.Session, this, this.IsReadOnly);

            this.ValueSet.Add(row);
         }

        /// <summary>
        /// Updates the <see cref="OkCanExecute"/> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedOwner != null;
        }

        /// <summary>
        /// Populates the <see cref="PossibleOwner"/> property
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();
            var model = this.Container.Container.Container.Container as EngineeringModel;
            if (model == null)
            {
                return;
            }

            if (this.SelectedOwner == null)
            {
                this.SelectedOwner = this.Thing.Parameter.Owner;
            }

            if (this.Thing.Parameter.AllowDifferentOwnerOfOverride)
            {
                this.PossibleOwner.AddRange(model.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name));   
            }
            else
            {
                this.PossibleOwner.Add(this.Thing.Parameter.Owner);   
            }
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            for (int i = 0; i < this.Thing.ValueSet.Count; i++)
            {
                this.Thing.ValueSet[i] = this.Thing.ValueSet[i].Clone(false);
            }

            this.ValueSet.First().UpdateParameterOverrideValueSet(this.Thing);

            foreach (var parameterOverrideValueSet in this.Thing.ValueSet)
            {
                this.transaction.CreateOrUpdate(parameterOverrideValueSet);
            }
        }
        
        /// <summary>
        /// Check the validation status of the value rows
        /// </summary>
        private void CheckValueValidation()
        {
            foreach (var valueRow in this.ValueSet)
            {
                valueRow.CheckValues(this.Thing.Scale);
            }
        }
    }
}