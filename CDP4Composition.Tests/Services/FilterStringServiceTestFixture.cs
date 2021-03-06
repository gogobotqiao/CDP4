﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterStringServiceTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2019 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Services
{
    using NUnit.Framework;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using Moq;

    /// <summary>
    /// Suite of tests for the <see cref="FilterStringServiceTestFixture"/> class
    /// </summary>
    [TestFixture]
    public class FilterStringServiceTestFixture
    {
        private Mock<IDeprecatableToggleViewModel> deprecatableToggle;

        private Mock<IPanelView> goodView;
        private Mock<IPanelFilterableDataGridView> goodViewFilterable;
        private Mock<IPanelView> badView;
        private Mock<IPanelViewModel> goodViewModel;
        private Mock<IPanelViewModel> badViewModel;
        private Mock<IDeprecatableBrowserViewModel> goodViewModelDeprecatable;
        private Mock<IFavoritesBrowserViewModel> goodViewModelFavorable;

        [SetUp]
        public void SetUp()
        {
            this.deprecatableToggle = new Mock<IDeprecatableToggleViewModel>();
            this.goodView = new Mock<IPanelView>();
            this.badView = new Mock<IPanelView>();

            this.goodViewFilterable = this.goodView.As<IPanelFilterableDataGridView>();
            this.goodViewModel = new Mock<IPanelViewModel>();
            this.goodViewModelDeprecatable = this.goodViewModel.As<IDeprecatableBrowserViewModel>();
            this.goodViewModelFavorable = this.goodViewModel.As<IFavoritesBrowserViewModel>();

            this.badViewModel = new Mock<IPanelViewModel>();
        }

        [Test]
        public void Verify_that_registering_bad_view_does_not_work()
        {
            var filterStringService = new FilterStringService();

            Assert.AreEqual(0, filterStringService.OpenDeprecatedControls.Count);
            Assert.AreEqual(0, filterStringService.OpenFavoriteControls.Count);

            filterStringService.RegisterForService(this.badView.Object, this.badViewModel.Object);

            Assert.AreEqual(0, filterStringService.OpenDeprecatedControls.Count);
            Assert.AreEqual(0, filterStringService.OpenFavoriteControls.Count);
        }
    }
}