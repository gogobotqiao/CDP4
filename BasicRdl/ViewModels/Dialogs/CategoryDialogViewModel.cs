﻿// -------------------------------------------------------------------------------------------------
// <copyright file="CategoryDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

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
    /// The purpose of the <see cref="CategoryDialogViewModel"/> is to allow an <see cref="Category"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Category"/> will result in an <see cref="Category"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.Category)]
    public class CategoryDialogViewModel : CDP4CommonView.CategoryDialogViewModel, IThingDialogViewModel
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public CategoryDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDialogViewModel"/> class.
        /// </summary>
        /// <param name="category">
        /// The <see cref="Category"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="CategoryDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="CategoryDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> of the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public CategoryDialogViewModel(Category category, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(category, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.PermissibleClass).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.PopulateSuperCategory());
        }
        #endregion

        /// <summary>
        /// Gets or sets the possible permissible class
        /// </summary>
        public ReactiveList<ClassKind> PossiblePermissibleClasses { get; set; }

        /// <summary>
        /// Gets or sets the list of possible super categories
        /// </summary>
        public ReactiveList<Category> PossibleSuperCategories { get; set; }

        /// <summary>
        /// Initializes the list of this dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossiblePermissibleClasses = new ReactiveList<ClassKind>();
            this.PossibleSuperCategories = new ReactiveList<Category>();
        }

        /// <summary>
        /// Populates the <see cref="CategoryDialogViewModel.PermissibleClass"/> property
        /// </summary>
        protected override void PopulatePermissibleClass()
        {
            this.PossiblePermissibleClasses.Clear();

            var possiblePermissibleClasses = typeof(Thing).Assembly.GetTypes().Where(t => typeof(ICategorizableThing).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).OrderBy(x => x.Name).Select(x => (ClassKind)Enum.Parse(typeof(ClassKind), x.Name));

            foreach (var possiblePermissableClass in possiblePermissibleClasses)
            {
                this.PossiblePermissibleClasses.Add(possiblePermissableClass);

                if (this.Thing.PermissibleClass.Contains(possiblePermissableClass))
                {
                    this.PermissibleClass.Add(possiblePermissableClass);
                }
            }
        }

        /// <summary>
        /// Populates the <see cref="CategoryDialogViewModel.SuperCategory"/> property
        /// </summary>
        protected override void PopulateSuperCategory()
        {
            this.PossibleSuperCategories.Clear();

            foreach (var possibleSuperCategory in this.PopulatePossibleSuperCategories())
            {
                this.PossibleSuperCategories.Add(possibleSuperCategory);

                if (this.Thing.SuperCategory.Contains(possibleSuperCategory))
                {
                    this.SuperCategory.Add(possibleSuperCategory);
                }
            }
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.PermissibleClass.Any();
        }

        /// <summary>
        /// Get the possible ordered super-<see cref="Category"/>
        /// </summary>
        /// <returns>The ordered super <see cref="Category"/>s</returns>
        private IEnumerable<Category> PopulatePossibleSuperCategories()
        {
            var rdlContainer = this.Container as ReferenceDataLibrary;
            if (rdlContainer == null)
            {
                return Enumerable.Empty<Category>();
            }

            var allPossibleSuperCategories = new List<Category>(rdlContainer.DefinedCategory);
            allPossibleSuperCategories.Remove(this.Thing);

            foreach (var rdl in rdlContainer.GetRequiredRdls())
            {
                allPossibleSuperCategories.AddRange(rdl.DefinedCategory);
            }

            var possibleSuperCategories = allPossibleSuperCategories.ToList();

            // TODO Deal with Update of Category, what happens when container is changed?? is it allowed?
            if (this.dialogKind != ThingDialogKind.Create)
            {
                possibleSuperCategories = possibleSuperCategories.Except(this.GetRdlSubCategories(this.Thing)).ToList();
            }

            return possibleSuperCategories.OrderBy(c => c.ShortName);
        }

        /// <summary>
        /// Gets the Sub-categories of a <see cref="Category"/> in the <see cref="ReferenceDataLibrary"/> it is contained in
        /// </summary>
        /// <param name="category">The <see cref="Category"/></param>
        /// <returns>The list of sub-<see cref="Category"/></returns>
        private IEnumerable<Category> GetRdlSubCategories(Category category)
        {
            var subCategories = new List<Category>();
            var rdl = (ReferenceDataLibrary)category.Container;

            foreach (var cat in rdl.DefinedCategory)
            {
                // Get the sub-categories for the current sub-category
                if (!cat.SuperCategory.Contains(category))
                {
                    continue;
                }

                subCategories.AddRange(this.GetRdlSubCategories(cat));
                subCategories.Add(cat);
            }

            return subCategories;
        }
    }
}
