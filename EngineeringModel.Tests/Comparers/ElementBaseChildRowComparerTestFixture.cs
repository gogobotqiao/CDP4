﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementBaseChildRowComparerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Comparers
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.Comparers;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class ElementBaseChildRowComparerTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");

        private SiteDirectory siteDir;
        private EngineeringModel model;
        private Iteration iteration;
        private EngineeringModelSetup modelSetup;
        private IterationSetup iterationSetup;
        private Person person;
        private Participant participant;
        private Option option;
        private ElementDefinition elementDef;
        private ElementDefinition elementDef2;
        private DomainOfExpertise domain;
        private ElementUsage elementUsage;
        private ParameterType type;
        private ParameterValueSet valueSet;

        [SetUp]
        public void Setup()
        {
            this.permissionService = new Mock<IPermissionService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domain" };

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.participant = new Participant(Guid.NewGuid(), null, this.uri);
            this.option = new Option(Guid.NewGuid(), null, this.uri);
            this.elementDef = new ElementDefinition(Guid.NewGuid(), null, this.uri) { Owner = this.domain };
            this.type = new EnumerationParameterType(Guid.NewGuid(), null, this.uri) { Name = "a" };
            this.valueSet = new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                Published = new ValueArray<string>(new List<string> { "1" }),
                Manual = new ValueArray<string>(new List<string> { "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.elementDef2 = new ElementDefinition(Guid.NewGuid(), null, this.uri) { Owner = this.domain };
            this.elementUsage = new ElementUsage(Guid.NewGuid(), null, this.uri) { ElementDefinition = this.elementDef2, Owner = this.domain };

            this.siteDir.Person.Add(this.person);
            this.siteDir.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.participant.Person = this.person;

            this.model.Iteration.Add(this.iteration);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Option.Add(this.option);
            this.iteration.TopElement = this.elementDef;
            this.iteration.Element.Add(this.elementDef);
            this.iteration.Element.Add(this.elementDef2);
            this.elementDef.ContainedElement.Add(this.elementUsage);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRowsAreInsertedCorrectly()
        {
            var list = new ReactiveList<IRowViewModelBase<Thing>>();
            var comparer = new ElementBaseChildRowComparer();

            var parameter1 = new Parameter(Guid.NewGuid(), null, this.uri) { ParameterType = this.type, Owner = this.domain, Container = this.elementDef };
            parameter1.ValueSet.Add(this.valueSet);

            var parameterRow1 = new ParameterRowViewModel(parameter1, this.session.Object, null, false);

            var typeClone = this.type.Clone(false);
            typeClone.Name = "b";
            var paraClone = parameter1.Clone(false);
            paraClone.ParameterType = typeClone;

            var parameterRow2 = new ParameterRowViewModel(paraClone, this.session.Object, null, false) { Name = "b" };

            var group1 = new ParameterGroup(Guid.NewGuid(), null, this.uri) { Name = "a" };
            var groupRow1 = new ParameterGroupRowViewModel(group1, this.domain, this.session.Object, null);

            var group2 = new ParameterGroup(Guid.NewGuid(), null, this.uri) { Name = "b" };
            var groupRow2 = new ParameterGroupRowViewModel(group2, this.domain, this.session.Object, null);

            var usage1 = this.elementUsage.Clone(false);
            usage1.Name = "def";
            var usageRow1 = new ElementUsageRowViewModel(usage1, this.domain, this.session.Object, null);

            var usage2 = this.elementUsage.Clone(false);
            usage2.Name = "abc";
            var usageRow2 = new ElementUsageRowViewModel(usage2, this.domain, this.session.Object, null);

            var usage3 = this.elementUsage.Clone(false);
            usage3.Name = "ghi";
            var usageRow3 = new ElementUsageRowViewModel(usage3, this.domain, this.session.Object, null);

            list.SortedInsert(usageRow1, comparer);
            list.SortedInsert(usageRow3, comparer);

            list.SortedInsert(parameterRow1, comparer);
            list.SortedInsert(groupRow1, comparer);
            list.SortedInsert(parameterRow2, comparer);
            list.SortedInsert(groupRow2, comparer);

            list.SortedInsert(usageRow2, comparer);

            Assert.AreSame(parameterRow1, list[0]);
            Assert.AreSame(parameterRow2, list[1]);
            Assert.AreSame(groupRow1, list[2]);
            Assert.AreSame(groupRow2, list[3]);
            Assert.AreSame(usageRow2, list[4]);
            Assert.AreSame(usageRow1, list[5]);
            Assert.AreSame(usageRow3, list[6]);
        }

        [Test]
        public void VerifyThatParametersAreInRightOrder()
        {
            var parameter1 = new Parameter(Guid.NewGuid(), null, this.uri) { ParameterType = this.type, Owner = this.domain, Container = this.elementDef };
            parameter1.ValueSet.Add(this.valueSet);

            var cpt = new CompoundParameterType { Name = "B", ShortName = "B" };
            var cpt1 = new ParameterTypeComponent(Guid.NewGuid(), null, this.uri) { ParameterType = this.type };
            var cpt2 = new ParameterTypeComponent(Guid.NewGuid(), null, this.uri) { ParameterType = this.type };
            var cpt3 = new ParameterTypeComponent(Guid.NewGuid(), null, this.uri) { ParameterType = this.type };

            cpt.Component.Add(cpt1);
            cpt.Component.Add(cpt2);
            cpt.Component.Add(cpt3);
            var parameter2 = new Parameter(Guid.NewGuid(), null, this.uri) { ParameterType = cpt, Owner = this.domain, Container = this.elementDef };
            
            var values = new List<string> {"a", "b", "c"};
            var cptValueSet = new ParameterValueSet();
            cptValueSet.Manual = new ValueArray<string>(values);
            cptValueSet.Reference = new ValueArray<string>(values);
            cptValueSet.Computed = new ValueArray<string>(values);
            cptValueSet.Formula = new ValueArray<string>(values);
            cptValueSet.Published = new ValueArray<string>(values);

            parameter2.ValueSet.Add(cptValueSet);
            
            var type3 = new BooleanParameterType(Guid.NewGuid(), null, this.uri) { Name = "abc", ShortName = "abc"};
            var parameter3 = new Parameter(Guid.NewGuid(), null, this.uri) { ParameterType = type3, Owner = this.domain, Container = this.elementDef };
            parameter3.ValueSet.Add(this.valueSet);

            this.elementDef.Parameter.Add(parameter1);
            this.elementDef.Parameter.Add(parameter2);
            this.elementDef.Parameter.Add(parameter3);

            var elementDefRow = new ElementDefinitionRowViewModel(this.elementDef, this.domain, this.session.Object, null);

            Assert.AreEqual(4, elementDefRow.ContainedRows.Count);

            Assert.AreSame(parameter1, elementDefRow.ContainedRows[0].Thing);
            Assert.AreSame(parameter3, elementDefRow.ContainedRows[1].Thing);
            Assert.AreSame(parameter2, elementDefRow.ContainedRows[2].Thing);
        }
    }
}