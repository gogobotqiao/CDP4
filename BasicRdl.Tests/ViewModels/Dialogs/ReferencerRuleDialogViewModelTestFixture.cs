﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ReferencerRuleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class ReferencerRuleDialogViewModelTestFixture
    {
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private SiteReferenceDataLibrary siteRdl;

        private Mock<IThingDialogNavigationService> thingDialogService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.thingDialogService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "testRDL", ShortName = "test" };

            var cat = new Category(Guid.NewGuid(), null, null) { Name = "category", ShortName = "cat" };
            cat.PermissibleClass.Add(ClassKind.ElementDefinition);
            var cat1 = new Category(Guid.NewGuid(), null, null) { Name = "category1", ShortName = "cat1" };

            this.siteRdl.DefinedCategory.Add(cat1);
            this.siteRdl.DefinedCategory.Add(cat);
            this.siteDir.SiteReferenceDataLibrary.Add(this.siteRdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesArePopulated()
        {
            var referencer = new ReferencerRule
            {
                Name = "name",
                ShortName = "shortname",
                MinReferenced = 1,
                MaxReferenced = 2
            };

            var vm = new ReferencerRuleDialogViewModel(referencer, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogService.Object);
            Assert.AreEqual(referencer.Name, vm.Name);
            Assert.AreEqual(referencer.ShortName, vm.ShortName);
            Assert.AreEqual(referencer.MinReferenced, vm.MinReferenced);
            Assert.AreEqual(referencer.MaxReferenced, vm.MaxReferenced);

            Assert.AreEqual(1, vm.PossibleContainer.Count);
            vm.Container = vm.PossibleContainer.Single();
            Assert.AreEqual(1, vm.PossibleReferencedCategory.Count);
            Assert.AreEqual(1, vm.PossibleReferencingCategory.Count);

            Assert.IsFalse(vm.OkCommand.CanExecute(null));
            vm.SelectedReferencingCategory = vm.PossibleReferencingCategory.First();
            vm.ReferencedCategory = new ReactiveList<Category>(vm.PossibleReferencedCategory);

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            var referencer = new ReferencerRule
            {
                Name = "name",
                ShortName = "shortname",
                MinReferenced = 1,
                MaxReferenced = 2
            };

            var vm = new ReferencerRuleDialogViewModel(referencer, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogService.Object);

            Assert.IsFalse(vm.OkCommand.CanExecute(null));
            vm.SelectedReferencingCategory = vm.PossibleReferencingCategory.First();
            Assert.IsFalse(vm.OkCommand.CanExecute(null));
            vm.ReferencedCategory = new ReactiveList<Category>(vm.PossibleReferencedCategory);
            Assert.IsTrue(vm.OkCommand.CanExecute(null));

            vm.MinReferenced = 3;
            Assert.IsFalse(vm.OkCommand.CanExecute(null));
            vm.MinReferenced = 2;
            Assert.IsFalse(vm.OkCommand.CanExecute(null));
            var cat2 = new Category(Guid.NewGuid(), null, null) { Name = "category2", ShortName = "cat2" };
            vm.ReferencedCategory.Add(cat2);
            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatInspectCommandsOpenDialogs()
        {
            var referencer = new ReferencerRule
            {
                Name = "name",
                ShortName = "shortname"
            };

            var vm = new ReferencerRuleDialogViewModel(referencer, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogService.Object);

            Assert.IsTrue(vm.InspectSelectedReferencingCategoryCommand.CanExecute(null));
            vm.InspectSelectedReferencingCategoryCommand.Execute(null);
            this.thingDialogService.Verify(x => x.Navigate(It.IsAny<Category>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.thingDialogService.Object, It.IsAny<Thing>(), null));

            vm.SelectedReferencingCategory = null;
            Assert.IsFalse(vm.InspectSelectedReferencingCategoryCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new ReferencerRuleDialogViewModel());
        }
    }
}