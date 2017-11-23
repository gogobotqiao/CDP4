﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ParameterComponentValueRowViewModel = CDP4EngineeringModel.ViewModels.Dialogs.ParameterComponentValueRowViewModel;
    using ParameterOptionRowViewModel = CDP4EngineeringModel.ViewModels.Dialogs.ParameterOptionRowViewModel;
    using ParameterStateRowViewModel = CDP4EngineeringModel.ViewModels.Dialogs.ParameterStateRowViewModel;

    [TestFixture]
    internal class ParameterDialogViewModelTestFixture
    {
        private Uri uri;
        private ThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Parameter parameter;
        private Iteration iteration;
        private EngineeringModel model;
        private SiteDirectory sitedir;
        private SiteReferenceDataLibrary srdl;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private ElementDefinition elementDefinitionClone;

        private SimpleQuantityKind simpleQt;
        private RatioScale integerScale;
        private RatioScale realScale;
        private BooleanParameterType boolPt;

        private CompoundParameterType cptPt;
        private ParameterTypeComponent c1;

        private Option option1;
        private Option option2;

        private PossibleFiniteStateList psl;
        private PossibleFiniteState ps1;
        private PossibleFiniteState ps2;

        private ActualFiniteStateList asl;
        private ActualFiniteState as1;
        private ActualFiniteState as2;

        [SetUp]
        public void Setup()
        {
            this.uri = new Uri("http://www.rheagroup.com");
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            var testDomain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            var subscription = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri);
            var anotherDomain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "Other Domain" };
            subscription.Owner = anotherDomain;
            var paramValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            paramValueSet.Computed = new ValueArray<string>(new[] { "c" });
            paramValueSet.Manual = new ValueArray<string>(new[] { "m" });
            paramValueSet.Reference = new ValueArray<string>(new[] { "r" });
            paramValueSet.ValueSwitch = ParameterSwitchKind.COMPUTED;

            var testParamType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri);

            this.parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { Owner = testDomain, ParameterType = testParamType };
            this.parameter.ParameterSubscription.Add(subscription);
            this.parameter.ValueSet.Add(paramValueSet);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            elementDefinition.Parameter.Add(this.parameter);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.option1 = new Option(Guid.NewGuid(), this.cache, this.uri) { Name = "opt1", ShortName = "o1"};
            this.option2 = new Option(Guid.NewGuid(), this.cache, this.uri) { Name = "opt2", ShortName = "o2" };
            this.iteration.Option.Add(this.option1);
            this.iteration.Option.Add(this.option2);
            this.iteration.Element.Add(elementDefinition);

            this.psl = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.ps1 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) {Name = "1", ShortName = "1"};
            this.ps2 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) {Name = "2", ShortName = "2"};
            this.psl.PossibleState.Add(this.ps1);
            this.psl.PossibleState.Add(this.ps2);

            this.asl = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.as1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.as1.PossibleState.Add(this.ps1);
            this.as2 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.as2.PossibleState.Add(this.ps2);

            this.asl.PossibleFiniteStateList.Add(this.psl);

            this.asl.ActualState.Add(this.as1);
            this.asl.ActualState.Add(this.as2);
            this.iteration.ActualFiniteStateList.Add(this.asl);
            this.iteration.PossibleFiniteStateList.Add(this.psl);

            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            modelSetup.ActiveDomain.Add(testDomain);
            modelSetup.ActiveDomain.Add(anotherDomain);
            var testPerson = new Person(Guid.NewGuid(), null, null);
            var testParticipant = new Participant(Guid.NewGuid(), null, null) { Person = testPerson, SelectedDomain = testDomain };
            modelSetup.Participant.Add(testParticipant);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = modelSetup };
            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            var ratioScale = new RatioScale(Guid.NewGuid(), this.cache, this.uri);
            this.srdl.Scale.Add(ratioScale);
            testParamType.PossibleScale.Add(ratioScale);
            this.srdl.ParameterType.Add(testParamType);
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };
            this.model.EngineeringModelSetup.RequiredRdl.Add(mrdl);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);

            this.model.Iteration.Add(this.iteration);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(testPerson);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null),  new Lazy<Thing>(() => this.iteration));

            this.elementDefinitionClone = elementDefinition.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.elementDefinitionClone);

            this.integerScale = new RatioScale(Guid.NewGuid(), this.cache, this.uri)
            {
                NumberSet = NumberSetKind.INTEGER_NUMBER_SET,
                MaximumPermissibleValue = "5",
                MinimumPermissibleValue = "0"
            };

            this.realScale = new RatioScale(Guid.NewGuid(), this.cache, this.uri)
            {
                MaximumPermissibleValue = "50",
                MinimumPermissibleValue = "0",
                NumberSet = NumberSetKind.REAL_NUMBER_SET
            };

            this.simpleQt = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri);
            this.simpleQt.PossibleScale.Add(this.integerScale);
            this.simpleQt.PossibleScale.Add(this.realScale);

            this.cptPt = new CompoundParameterType(Guid.NewGuid(), this.cache, this.uri);
            this.c1 = new ParameterTypeComponent(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.simpleQt, Scale = this.integerScale
            };
            this.cptPt.Component.Add(this.c1);

            this.srdl.Scale.Add(this.integerScale);
            this.srdl.Scale.Add(this.realScale);

            this.srdl.ParameterType.Add(this.cptPt);
            this.srdl.ParameterType.Add(this.simpleQt);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

        }

        [Test]
        public void VerifyThatPropertiesArePopulated()
        {
            var vm = new ParameterDialogViewModel(this.parameter, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementDefinitionClone);
            
            Assert.IsNotNull(vm.SelectedParameterType);
            Assert.IsNotNull(vm.SelectedOwner);
            Assert.AreEqual(1, vm.ValueSet.Count);
            Assert.AreEqual(3, vm.PossibleParameterType.Count);
            Assert.AreEqual(2, vm.PossibleOwner.Count);
            Assert.AreEqual(1, vm.PossibleRequestedBy.Count);
            Assert.AreEqual(1, vm.PossibleStateDependence.Count);
            Assert.AreEqual(1, vm.PossibleScale.Count);
            var valueSet = vm.ValueSet.First();

            Assert.AreEqual("m", valueSet.Manual);
            Assert.AreEqual("r", valueSet.Reference);
            Assert.AreEqual("c", valueSet.Computed);
            Assert.AreEqual(ParameterSwitchKind.COMPUTED, valueSet.Switch);
            Assert.AreEqual("c", valueSet.Value);

            Assert.AreEqual(1, vm.ParameterSubscription.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new ParameterDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsReadOnly);
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            var vm = new ParameterDialogViewModel(this.parameter, this.thingTransaction, this.session.Object, true,
    ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementDefinitionClone);

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
            var owner = vm.SelectedOwner;

            vm.SelectedOwner = null;
            Assert.IsFalse(vm.OkCommand.CanExecute(null));
            vm.SelectedOwner = owner;
        }

        [Test]
        public void VerifyUpdateOkExecute()
        {
            var vm = new ParameterDialogViewModel(this.parameter, this.thingTransaction, this.session.Object, true,
    ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementDefinitionClone);

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
            vm.OkCommand.Execute(null);
        }

        [Test]
        public void VerifyInspectStateDependence()
        {
            var vm = new ParameterDialogViewModel(this.parameter, this.thingTransaction, this.session.Object, true,
    ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementDefinitionClone);
            Assert.IsNull(vm.SelectedStateDependence);

            vm.SelectedStateDependence = vm.PossibleStateDependence.First();
            Assert.IsTrue(vm.InspectSelectedStateDependenceCommand.CanExecute(null));
            vm.InspectSelectedStateDependenceCommand.Execute(null);
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<ActualFiniteStateList>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyThatMakingOptionOrStateDependentSetToFalse()
        {
            var vm = new ParameterDialogViewModel(this.parameter, this.thingTransaction, this.session.Object, true,
    ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementDefinitionClone);

            Assert.IsTrue(vm.IsValueSetEditable);

            vm.IsOptionDependent = true;
            Assert.IsFalse(vm.IsValueSetEditable);

            vm.SelectedStateDependence = vm.PossibleStateDependence.First();
            Assert.IsFalse(vm.IsValueSetEditable);

            vm.IsOptionDependent = false;
            Assert.IsFalse(vm.IsValueSetEditable);

            vm.SelectedStateDependence = null;
            Assert.IsTrue(vm.IsValueSetEditable);
        }

        [Test]
        public void VerifyThatWrongParameterValueDisableOkButton()
        {
            this.parameter.ParameterType = this.simpleQt;
            this.parameter.Scale = this.integerScale;
            var vm = new ParameterDialogViewModel(this.parameter, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementDefinitionClone);

            var row = vm.ValueSet.Single();
            row.Manual = "100";
            var error = row["Manual"];

            Assert.IsTrue(vm.ValidationErrors.Any(x => x.PropertyName == "Manual"));
            Assert.IsFalse(vm.OkCanExecute);

            row.Manual = "2";
            error = row["Manual"];

            Assert.IsFalse(vm.ValidationErrors.Any(x => x.PropertyName == "Manual"));
            Assert.IsTrue(vm.OkCanExecute);

            row.Reference = "100";
            error = row["Reference"];

            Assert.IsTrue(vm.ValidationErrors.Any(x => x.PropertyName == "Reference"));
            Assert.IsFalse(vm.OkCanExecute);

            row.Reference = "2";
            error = row["Reference"];

            Assert.IsFalse(vm.ValidationErrors.Any(x => x.PropertyName == "Reference"));
            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyThatWrongParameterOptionValueDisableOkButton()
        {
            var defaultList = new List<string> { "-" };

            this.parameter.ParameterType = this.simpleQt;
            this.parameter.Scale = this.integerScale;
            this.parameter.IsOptionDependent = true;

            this.parameter.ValueSet.Clear();
            var v1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { ActualOption = this.option1 };
            this.PopulateValueSet(v1, defaultList);

            var v2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { ActualOption = this.option2 };
            this.PopulateValueSet(v2, defaultList);

            this.parameter.ValueSet.Add(v1);
            this.parameter.ValueSet.Add(v2);

            var vm = new ParameterDialogViewModel(this.parameter, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementDefinitionClone);

            var row = vm.ValueSet.Single();

            var o1 = (ParameterOptionRowViewModel)row.ContainedRows.First();
            o1.Manual = "100";
            var error = o1["Manual"];

            Assert.IsTrue(vm.ValidationErrors.Any(x => x.PropertyName == "o1..0Manual"));
            Assert.IsFalse(vm.OkCanExecute);

            o1.Manual = "2";
            error = o1["Manual"];

            Assert.IsFalse(vm.ValidationErrors.Any(x => x.PropertyName == "o1..0Manual"));
            Assert.IsTrue(vm.OkCanExecute);

            o1.Reference = "100";
            error = o1["Reference"];

            Assert.IsTrue(vm.ValidationErrors.Any(x => x.PropertyName == "o1..0Reference"));
            Assert.IsFalse(vm.OkCanExecute);

            o1.Reference = "2";
            error = o1["Reference"];

            Assert.IsFalse(vm.ValidationErrors.Any(x => x.PropertyName == "o1..0Reference"));
            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyThatWrongParameterStateValueDisableOkButtonWhenScaleChanges()
        {
            var defaultList = new List<string> { "-" };

            this.parameter.ParameterType = this.simpleQt;
            this.parameter.Scale = this.realScale;
            this.parameter.IsOptionDependent = false;
            this.parameter.StateDependence = this.asl;

            this.parameter.ValueSet.Clear();
            var v1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { ActualState = this.as1 };
            this.PopulateValueSet(v1, defaultList);

            var v2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri) { ActualState = this.as2 };
            this.PopulateValueSet(v2, defaultList);

            this.parameter.ValueSet.Add(v1);
            this.parameter.ValueSet.Add(v2);

            var vm = new ParameterDialogViewModel(this.parameter, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementDefinitionClone);

            var row = vm.ValueSet.Single();

            var s1 = (ParameterStateRowViewModel)row.ContainedRows.First();
            s1.Manual = "5.12";
            var error = s1["Manual"];

            Assert.IsFalse(vm.ValidationErrors.Any(x => x.PropertyName == ".1.0Manual"));
            Assert.IsTrue(vm.OkCanExecute);

            vm.SelectedScale = this.integerScale;
            error = s1["Manual"];

            Assert.IsTrue(vm.ValidationErrors.Any(x => x.PropertyName == ".1.0Manual"));
            Assert.IsFalse(vm.OkCanExecute);
        }

        [Test]
        public void VerifyThatWrongParameterComponentValueDisableOkButton()
        {
            var defaultList = new List<string> { "-" };

            this.parameter.ParameterType = this.cptPt;
            this.parameter.IsOptionDependent = false;
            this.parameter.StateDependence = null;

            var vm = new ParameterDialogViewModel(this.parameter, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementDefinitionClone);

            var row = vm.ValueSet.Single();

            var c1 = (ParameterComponentValueRowViewModel)row.ContainedRows.First();
            
            c1.Manual = "5500000";
            var error = c1["Manual"];

            Assert.IsTrue(vm.ValidationErrors.Any(x => x.PropertyName == "..0Manual"));
            Assert.IsFalse(vm.OkCanExecute);

            c1.Manual = "4";
            error = c1["Manual"];

            Assert.IsFalse(vm.ValidationErrors.Any(x => x.PropertyName == "..0Manual"));
            Assert.IsTrue(vm.OkCanExecute);

            c1.Reference = "100";
            error = c1["Reference"];

            Assert.IsTrue(vm.ValidationErrors.Any(x => x.PropertyName == "..0Reference"));
            Assert.IsFalse(vm.OkCanExecute);

            c1.Reference = "2";
            error = c1["Reference"];

            Assert.IsFalse(vm.ValidationErrors.Any(x => x.PropertyName == "..0Reference"));
            Assert.IsTrue(vm.OkCanExecute);
        }

        private void PopulateValueSet(ParameterValueSet set, IEnumerable<string> values)
        {
            set.Manual = new ValueArray<string>(values);
            set.Computed = new ValueArray<string>(values);
            set.Reference = new ValueArray<string>(values);
            set.Published = new ValueArray<string>(values);
            set.Formula = new ValueArray<string>(values);
        }
    }
}