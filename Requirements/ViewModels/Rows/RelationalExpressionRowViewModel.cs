﻿// -------------------------------------------------------------------------------------------------
// <copyright file="RelationalExpressionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Diagram;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    /// <summary>
    /// the row-view-model representing a <see cref="RelationalExpression"/>
    /// </summary>
    public class RelationalExpressionRowViewModel : CDP4CommonView.RelationalExpressionRowViewModel, IDropTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalExpressionRowViewModel"/> class
        /// </summary>
        /// <param name="notExpression">The <see cref="RelationalExpression"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public RelationalExpressionRowViewModel(RelationalExpression notExpression, ISession session, IViewModelBase<Thing> containerViewModel) : base(notExpression, session, containerViewModel)
        {
        }

        /// <summary>
        /// Gets the short string representation of the current RelationalExpression
        /// </summary>
        public string ShortName => this.Thing.StringValue;

        /// <summary>
        /// Gets the parameter type name of the current RelationalExpression
        /// </summary>
        public string Name => this.Thing.ParameterType?.Name;

        /// <summary>
        /// Gets the value of the current RelationalExpression
        /// </summary>
        public string Definition => string.Join(", ", this.Thing.Value);
        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is Parameter dropParameter && (dropParameter.ParameterType.Iid == this.Thing?.ParameterType.Iid))
            {
                dropInfo.Effects = DragDropEffects.Copy;

                return;
            }
            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is Parameter dropParameter && (dropParameter.ParameterType.Iid == this.Thing?.ParameterType.Iid))
            {
                MessageBox.Show("That hurts man!", "Ow", MessageBoxButton.OK);
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }
    }
}
