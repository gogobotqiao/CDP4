﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.RuleVerificationListBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using CDP4Common.CommonData;    
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;

    using Microsoft.Practices.ServiceLocation;

    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RuleVerificationListRowViewModel"/> class
    /// </summary>
    public class RuleVerificationListViewModelTestFixture
    {
        private PropertyInfo revision;

        private readonly Uri uri = new Uri("http://test.com");

        private Mock<IServiceLocator> serviceLocator;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        
        private Mock<IRuleVerificationService> ruleVerificationService;
        private List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>> builtInRules;
        private string builtInRuleName;
        private Mock<IBuiltInRule> builtInRule;
        private Mock<IBuiltInRuleMetaData> iBuiltInRuleMetaData;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            this.revision = typeof(Thing).GetProperty("RevisionNumber");

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.SetupIRuleVerificationService();

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };

            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.model.Iteration.Add(this.iteration);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);


        }

        /// <summary>
        /// Setup the mocked <see cref="IRuleVerificationService"/>
        /// </summary>
        private void SetupIRuleVerificationService()
        {
            this.ruleVerificationService = new Mock<IRuleVerificationService>();

            this.builtInRuleName = "shortnamerule";
            this.iBuiltInRuleMetaData = new Mock<IBuiltInRuleMetaData>();
            this.iBuiltInRuleMetaData.Setup(x => x.Author).Returns("RHEA");
            this.iBuiltInRuleMetaData.Setup(x => x.Name).Returns(this.builtInRuleName);
            this.iBuiltInRuleMetaData.Setup(x => x.Description).Returns("verifies that the shortnames are correct");

            this.builtInRule = new Mock<IBuiltInRule>();

            this.builtInRules = new List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>>();
            this.builtInRules.Add(new Lazy<IBuiltInRule, IBuiltInRuleMetaData>(() => this.builtInRule.Object, this.iBuiltInRuleMetaData.Object));

            this.ruleVerificationService.Setup(x => x.BuiltInRules).Returns(this.builtInRules);

            this.serviceLocator.Setup(x => x.GetInstance<IRuleVerificationService>()).Returns(this.ruleVerificationService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]        
        public void VerifyThatWhenParticipantIsNullArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                new RuleVerificationListBrowserViewModel(
                    this.iteration,
                    null,
                    this.session.Object,
                    this.thingDialogNavigationService.Object,
                    this.panelNavigationService.Object,
                    null));
        }

        [Test]
        public void VerifyThatViewModelPropertiesAreSet()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            Assert.AreEqual("Rule verification lists, iteration_0", viewmodel.Caption);
            Assert.AreEqual("model\nhttp://test.com/\n ", viewmodel.ToolTip);
            Assert.AreEqual("model", viewmodel.CurrentModel);
            Assert.AreEqual("domain []", viewmodel.DomainOfExpertise);
            Assert.AreEqual(this.participant, viewmodel.ActiveParticipant);
        }

        [Test]
        public void VerifyThatBrowserIsNotEmptyOnInitialLoad()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            CollectionAssert.IsNotEmpty(viewmodel.RuleVerificationListRowViewModels);
        }

        [Test]
        public void VerifyThatRuleIsAddedToViewModel()
        {
            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };

            this.iteration.RuleVerificationList.Add(ruleVerificationList);
            this.revision.SetValue(this.iteration, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            var row = viewmodel.RuleVerificationListRowViewModels.Single(x => x.Thing == ruleVerificationList);
            Assert.AreEqual(row.Owner, this.domain);

            this.iteration.RuleVerificationList.Remove(ruleVerificationList);
            this.revision.SetValue(this.iteration, 3);
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            Assert.IsEmpty(viewmodel.RuleVerificationListRowViewModels);
        }

        [Test]
        public void VerifyThatCreateCommandInvokesNavigationService()
        {
            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            Assert.IsTrue(viewmodel.CreateCommand.CanExecute(null));
            viewmodel.CreateCommand.Execute(null);

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<RuleVerificationList>(), It.IsAny<ThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyThatDragOverWorks()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });

            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            viewmodel.DragOver(dropinfo.Object);
            droptarget.Verify(x => x.DragOver(dropinfo.Object));
        }

        [Test]
        public void VerifyThatOnDragDragEffectIsNoneWhenParticipantSelectedDomainISNull()
        {


            var dropinfo = new Mock<IDropInfo>();
            dropinfo.SetupProperty(d => d.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;
            
            var droptarget = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(null, null)}
            });

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            viewmodel.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatDropWorks()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });

            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            viewmodel.Drop(dropinfo.Object);
            droptarget.Verify(x => x.Drop(dropinfo.Object));
        }

        [Test]
        public void VerifyThatOnDropDragEffectsIsNoneWhenParticipantSelectedDomainIsNull()
        {


            var dropinfo = new Mock<IDropInfo>();
            dropinfo.SetupProperty(d => d.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;

            var droptarget = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(null, null)}
            });

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            viewmodel.Drop(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatDropExceptionFeedbackIsSet()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.SetupProperty(d => d.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;

            var droptarget = new Mock<IDropTarget>();
            droptarget.Setup(x => x.Drop(dropinfo.Object)).Throws<Exception>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
            Assert.IsNullOrEmpty(viewmodel.Feedback);
            viewmodel.Drop(dropinfo.Object);

            Assert.IsNotNullOrEmpty(viewmodel.Feedback);
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            var vm = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, null, null, null);
            Assert.AreEqual("domain []", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(null, null) }
            });

            vm = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            vm = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatIfNothingIsSelectedThenVerifyCanNotBeExecuted()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);

            var vm = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, null, null, null);
            
            vm.SelectedThing = null;
            Assert.IsFalse(vm.VerifyRuleVerificationList.CanExecute(null)); 
        }

        [Test]
        public void VerifyThatIfRuleVerificationListIsSelecedTheRulesCanBeVerified()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });


            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);

            var vm = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, null, null, null);
            vm.SelectedThing = vm.RuleVerificationListRowViewModels.FirstOrDefault();
            vm.ComputePermission();

            Assert.IsTrue(vm.VerifyRuleVerificationList.CanExecute(null)); 
            vm.VerifyRuleVerificationList.Execute(null);

            this.ruleVerificationService.Verify(x => x.Execute(this.session.Object, ruleVerificationList));
        }
    }
}
