﻿// -------------------------------------------------------------------------------------------------
// <copyright file="YesNoDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.ViewModels
{
    using System;
    using System.Windows.Input;
    using CDP4Composition.Navigation;
    using ReactiveUI;

    /// <summary>
    /// The view model for a yes-no confirmation dialog
    /// </summary>
    public class YesNoDialogViewModel : DialogViewModelBase
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="YesNoDialogViewModel"/> class
        /// </summary>
        public YesNoDialogViewModel(string title, string message)
        {
            this.Title = title;
            this.Message = message;
            this.YesCommand = ReactiveCommand.Create();
            this.YesCommand.Subscribe(_ => this.ExecuteYesCommand());

            this.NoCommand = ReactiveCommand.Create();
            this.NoCommand.Subscribe(_ => this.ExecuteNoCommand());
        }
        #endregion

        /// <summary>
        /// Gets the title for the dialog
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the message of the dialog
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to confirm
        /// </summary>
        public ReactiveCommand<object> YesCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to cancel
        /// </summary>
        public ReactiveCommand<object> NoCommand { get; private set; }

        /// <summary>
        /// Executes the <see cref="YesCommand"/>
        /// </summary>
        private void ExecuteYesCommand()
        {
            this.DialogResult = new BaseDialogResult(true);
        }

        /// <summary>
        /// Executes the <see cref="NoCommand"/>
        /// </summary>
        private void ExecuteNoCommand()
        {
            this.DialogResult = new BaseDialogResult(false);
        }
    }
}