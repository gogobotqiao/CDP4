﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="MultiRelationshipRowViewModel"/> row
    /// </summary>
    public class MultiRelationshipRowViewModel : CDP4CommonView.MultiRelationshipRowViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="Name"/> property.
        /// </summary>
        private string name;

        /// <summary>
        /// Disctionary to map the related things and the related observables to be able to dispose them
        /// </summary>
        private Dictionary<Thing, IDisposable> oldRelatedThingSubcriptions = new Dictionary<Thing, IDisposable>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipRowViewModel"/> class
        /// </summary>
        /// <param name="relationship">The <see cref="MultiRelationship"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase<Thing>"/></param> container
        public MultiRelationshipRowViewModel(MultiRelationship relationship, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(relationship, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            // I look for the new and old elements
            var oldRelatedThings = this.oldRelatedThingSubcriptions.Keys.ToList();
            var newElements = this.Thing.RelatedThing.Except(oldRelatedThings);
            var oldElements = oldRelatedThings.Except(this.Thing.RelatedThing);

            //I remove old elements
            foreach (var element in oldElements)
            {
                this.Disposables.Remove(this.oldRelatedThingSubcriptions[element]);
                this.oldRelatedThingSubcriptions[element].Dispose();
                this.oldRelatedThingSubcriptions.Remove(element);
            }

            //In case there are new elements I create new name subcriptions and add them
            foreach (var element in newElements)
            {
                var elementSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(element)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateName());

                this.oldRelatedThingSubcriptions.Add(element, elementSubscription);
                this.Disposables.Add(elementSubscription);
            }

            this.UpdateName();
        }

        /// <summary>
        /// Update the relationship name
        /// </summary>
        protected void UpdateName()
        {
            bool first = true;

            var text = "";
            foreach (var thing in this.Thing.RelatedThing)
            {
                if (!first)
                {
                    text = text + ", ";
                }

                var thingName = thing is INamedThing ? (thing as INamedThing).Name : thing.ClassKind.ToString();
                text = text + thingName;

                first = false;
            }

            this.Name = text;
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="MultiRelationship"/> that is represented by the current row-view-model
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.name, value);
            }
        }

    }
}