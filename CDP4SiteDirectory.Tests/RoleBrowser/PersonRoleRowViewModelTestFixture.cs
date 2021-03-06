﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonRoleRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.RoleBrowser
{
    using System;
    using System.Reactive.Concurrency;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class PersonRoleRowViewModelTestFixture
    {
        private Mock<IThingDialogNavigationService> thingDialogNavigation;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private SiteDirectory siteDir;
        private PersonRole personRole;
        private readonly Uri uri = new Uri("http://test.com");

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.thingDialogNavigation = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.personRole = new PersonRole(Guid.NewGuid(), null, this.uri){Name = "aa"};
            this.siteDir.PersonRole.Add(this.personRole);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var row = new PersonRoleRowViewModel(this.personRole, this.session.Object, null);

            Assert.That(row.ClassKind, Is.Not.Null.Or.Empty);
            Assert.That(row.Name, Is.Not.Null.Or.Empty);
            Assert.IsNotEmpty(row.ContainedRows);
        }
    }
}