﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOrOverrideBaseRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;    
    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="ParameterOrOverrideBase"/>
    /// </summary>
    public abstract partial class ParameterOrOverrideBaseRowViewModel<T> : ParameterBaseRowViewModel<T> where T : ParameterOrOverrideBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOrOverrideBaseRowViewModel{T}"/> class
        /// </summary>
        /// <param name="parameterOrOverrideBase">The <see cref="ParameterOrOverrideBase"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected ParameterOrOverrideBaseRowViewModel(T parameterOrOverrideBase, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameterOrOverrideBase, session, containerViewModel)
        {
        }


	
    }
}