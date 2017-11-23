﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingCreatorTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Utilities
{
    using System;
    using System.Collections.Concurrent;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Services;
    using CDP4Dal;    
    using CDP4EngineeringModel.Utilities;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ThingCreator"/> class
    /// </summary>
    [TestFixture]
    public class ThingCreatorTestFixture
    {
        private Mock<ISession> session;
        private Mock<ISession> sessionThatThrowsException;
        private ThingCreator thingCreator;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();            
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();
            this.sessionThatThrowsException = new Mock<ISession>();
            this.sessionThatThrowsException.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws<Exception>();

            this.thingCreator = new ThingCreator();
        }

        [Test]
        public void VerifyThatCreateParameterThrowsExceptionWhenElementDefinitionIsNull()
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, null);
            var group = new ParameterGroup(Guid.NewGuid(), this.cache, null);
            var parameterType = new BooleanParameterType(Guid.NewGuid(), this.cache, null);
            var scale = new RatioScale(Guid.NewGuid(), this.cache, null);
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, null);

            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateParameter(null, group, parameterType, scale, domainOfExpertise, this.session.Object));
            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateParameter(elementDefinition, group, null, scale, domainOfExpertise, this.session.Object));
            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateParameter(elementDefinition, group, parameterType, scale, null, this.session.Object));
            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateParameter(elementDefinition, group, parameterType, scale, domainOfExpertise, null));
        }
        
        [Test]
        public async void VerifyThatCreateParameterWriteExceptionsAreThrown()
        {
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);

            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, null);
            iteration.Element.Add(elementDefinition);

            var parameterType = new BooleanParameterType(Guid.NewGuid(), this.cache, null);
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, null);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(elementDefinition.Iid, iteration.Iid), new Lazy<Thing>(() => elementDefinition));
            Assert.Throws<Exception>(async () => await this.thingCreator.CreateParameter(elementDefinition, null, parameterType, null, domainOfExpertise, this.sessionThatThrowsException.Object));
        }

        [Test]
        public async void VerifyThatWriteIsExecuted()
        {
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);

            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, null);
            iteration.Element.Add(elementDefinition);

            var parameterType = new BooleanParameterType();
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, null);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(elementDefinition.Iid, iteration.Iid), new Lazy<Thing>(() => elementDefinition));
            await this.thingCreator.CreateParameter(elementDefinition, null, parameterType, null, domainOfExpertise, this.session.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }
        
        [Test]
        public void VerifyThatArgumentNullExceptionsAreThrowForCreateUserRuleVerification()
        {
            var binaryRelationshipRule = new BinaryRelationshipRule(Guid.NewGuid(), this.cache, null);
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, null);

            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateUserRuleVerification(null, null, null));
            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateUserRuleVerification(ruleVerificationList, null, null));
            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateUserRuleVerification(ruleVerificationList, binaryRelationshipRule, null));
        }
        
        [Test]
        public async void VerifyThatCreateUserRuleVerificationExecutesWrite()
        {
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, null);
            iteration.RuleVerificationList.Add(ruleVerificationList);

            var binaryRelationshipRule = new BinaryRelationshipRule(Guid.NewGuid(), this.cache, null);

            await this.thingCreator.CreateUserRuleVerification(ruleVerificationList, binaryRelationshipRule, this.session.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public  void VerifyThatCreateUserRuleVerificationExecutesWriteSessionThrowsException()
        {
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, null);
            iteration.RuleVerificationList.Add(ruleVerificationList);

            var binaryRelationshipRule = new BinaryRelationshipRule(Guid.NewGuid(), this.cache, null);

            Assert.Throws<Exception>(async() => await this.thingCreator.CreateUserRuleVerification(ruleVerificationList, binaryRelationshipRule, this.sessionThatThrowsException.Object));
        }

        [Test]
        public void VerifyThatArgumentNullExceptionsAreThrowForCreateBuiltInRuleVerificationWhenRuleNull()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, null);
            var binaryRelationshipRule = new BinaryRelationshipRule(Guid.NewGuid(), this.cache, null);

            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateBuiltInRuleVerification(null, null, null));
            Assert.Throws<ArgumentException>(async () => await this.thingCreator.CreateBuiltInRuleVerification(ruleVerificationList, null, null));
            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateBuiltInRuleVerification(ruleVerificationList, "test", null));
        }
        
        [Test]
        public async void VerifyThatCreateBuiltInRuleVerificationExecutesWrite()
        {
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, null);
            iteration.RuleVerificationList.Add(ruleVerificationList);

            await this.thingCreator.CreateBuiltInRuleVerification(ruleVerificationList, "testrule", this.session.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatCreateBuiltInRuleVerificationExecutesWriteSessionException()
        {
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, null);
            iteration.RuleVerificationList.Add(ruleVerificationList);

            Assert.Throws<Exception>(async () => await this.thingCreator.CreateBuiltInRuleVerification(ruleVerificationList, "testrule", this.sessionThatThrowsException.Object));
        }

        [Test]
        public async void VerifyThatCreateElementUsageExecutesWrite()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, null);
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);

            var elementDefinitionA = new ElementDefinition(Guid.NewGuid(), this.cache, null);
            var elementDefinitionB = new ElementDefinition(Guid.NewGuid(), this.cache, null);

            iteration.Element.Add(elementDefinitionA);
            iteration.Element.Add(elementDefinitionB);

            await this.thingCreator.CreateElementUsage(elementDefinitionA, elementDefinitionB, domainOfExpertise, this.session.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }
        
        [Test]
        public void VerifyThatArgumentNullExceptionsAreThrowOnCreateElementUsage()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, null);
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);

            var elementDefinitionA = new ElementDefinition(Guid.NewGuid(), this.cache, null);
            var elementDefinitionB = new ElementDefinition(Guid.NewGuid(), this.cache, null);

            iteration.Element.Add(elementDefinitionA);
            iteration.Element.Add(elementDefinitionB);

            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateElementUsage(null, elementDefinitionB, domainOfExpertise, this.session.Object));
            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateElementUsage(elementDefinitionA, null, domainOfExpertise, this.session.Object));
            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateElementUsage(elementDefinitionA, elementDefinitionB, null, this.session.Object));
            Assert.Throws<ArgumentNullException>(async () => await this.thingCreator.CreateElementUsage(elementDefinitionA, elementDefinitionB, domainOfExpertise, null));
        }

        [Test]
        public void VerifyThatExceptionIsThrownWhenCreateElementUsageFails()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, null);
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            engineeringModel.Iteration.Add(iteration);

            var elementDefinitionA = new ElementDefinition(Guid.NewGuid(), this.cache, null);
            var elementDefinitionB = new ElementDefinition(Guid.NewGuid(), this.cache, null);

            iteration.Element.Add(elementDefinitionA);
            iteration.Element.Add(elementDefinitionB);

            Assert.Throws<Exception>(async () => await this.thingCreator.CreateElementUsage(elementDefinitionA, elementDefinitionB, domainOfExpertise, this.sessionThatThrowsException.Object));
        }
    }
}