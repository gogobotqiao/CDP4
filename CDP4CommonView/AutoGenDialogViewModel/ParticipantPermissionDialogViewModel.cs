﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantPermissionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ParticipantPermission"/>
    /// </summary>
    public partial class ParticipantPermissionDialogViewModel : DialogViewModelBase<ParticipantPermission>
    {
        /// <summary>
        /// Backing field for <see cref="AccessRight"/>
        /// </summary>
        private ParticipantAccessRightKind accessRight;

        /// <summary>
        /// Backing field for <see cref="ObjectClass"/>
        /// </summary>
        private ClassKind objectClass;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantPermissionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParticipantPermissionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantPermissionDialogViewModel"/> class
        /// </summary>
        /// <param name="participantPermission">
        /// The <see cref="ParticipantPermission"/> that is the subject of the current view-model. This is the object
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
        public ParticipantPermissionDialogViewModel(ParticipantPermission participantPermission, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(participantPermission, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ParticipantRole;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ParticipantRole",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the AccessRight
        /// </summary>
        public virtual ParticipantAccessRightKind AccessRight
        {
            get { return this.accessRight; }
            set { this.RaiseAndSetIfChanged(ref this.accessRight, value); }
        }

        /// <summary>
        /// Gets or sets the ObjectClass
        /// </summary>
        public virtual ClassKind ObjectClass
        {
            get { return this.objectClass; }
            set { this.RaiseAndSetIfChanged(ref this.objectClass, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public virtual bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.AccessRight = this.AccessRight;
            clone.ObjectClass = this.ObjectClass;
            clone.IsDeprecated = this.IsDeprecated;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.AccessRight = this.Thing.AccessRight;
            this.ObjectClass = this.Thing.ObjectClass;
            this.IsDeprecated = this.Thing.IsDeprecated;
        }
    }
}