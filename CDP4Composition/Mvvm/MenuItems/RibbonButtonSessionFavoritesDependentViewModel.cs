﻿// -------------------------------------------------------------------------------------------------
// <copyright file="RibbonButtonSessionFavoritesDependentViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;
    using Services.FavoritesService;

    /// <summary>
    /// The base class representing Ribbon button that depends on <see cref="ISession"/> to open a <see cref="IPanelView"/>, with Favoriting functionality.
    /// </summary>
    public abstract class RibbonButtonSessionFavoritesDependentViewModel : ReactiveObject
    {
        /// <summary>
        /// The Function returning an instance of <see cref="IPanelViewModel"/>
        /// </summary>
        protected readonly Func<ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IFavoritesService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Backing field for <see cref="HasSession"/>
        /// </summary>
        private ObservableAsPropertyHelper<bool> hasSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonButtonSessionFavoritesDependentViewModel"/> class
        /// </summary>

        protected RibbonButtonSessionFavoritesDependentViewModel(Func<ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IFavoritesService, IPanelViewModel> instantiatePanelViewModel)
        {
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModel;
            this.OpenSessions = new ReactiveList<RibbonMenuItemViewModelBase>();
            this.OpenSessions.ChangeTrackingEnabled = true;

            this.OpenSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession);

            this.OpenSingleBrowserCommand = ReactiveCommand.Create();
            this.OpenSingleBrowserCommand.Subscribe(_ => this.ExecuteOpenSingleBrowser());

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);
        }

        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasSession
        {
            get { return this.hasSession.Value; }
        }

        /// <summary>
        /// Gets the open or close the browser
        /// </summary>
        public ReactiveCommand<object> OpenSingleBrowserCommand { get; private set; }

        /// <summary>
        /// Gets the list of open sessions
        /// </summary>
        public ReactiveList<RibbonMenuItemViewModelBase> OpenSessions { get; private set; }

        /// <summary>
        /// Removes the <see cref="RibbonMenuItemViewModelBase"/> associated with the closed <see cref="ISession"/>
        /// </summary>
        /// <param name="session">The closed <see cref="ISession"/></param>
        protected void RemoveOpenSession(ISession session)
        {
            var currentSession = this.OpenSessions.Single(x => x.Session == session);
            currentSession.IsChecked = false;
            currentSession.ShowOrClosePanelCommand.Execute(null);

            var sessionToRemove = this.OpenSessions.SingleOrDefault(x => x.Session == session);
            if (sessionToRemove != null)
            {
                this.OpenSessions.Remove(sessionToRemove);
            }
        }

        /// <summary>
        /// Executes the <see cref="OpenSingleBrowserCommand"/> command
        /// </summary>
        private void ExecuteOpenSingleBrowser()
        {
            if (this.OpenSessions.Count != 1)
            {
                return;
            }

            var status = this.OpenSessions.Single().IsChecked;
            this.OpenSessions.Single().IsChecked = !status;

            this.OpenSessions.Single().ShowOrClosePanelCommand.Execute(null);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="ISession"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        protected virtual void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (sessionChange.Status == SessionStatus.Open)
            {
                this.OpenSessions.Add(
                    new RibbonMenuItemSessionFavoritesDependentViewModel(sessionChange.Session.Name, sessionChange.Session, this.InstantiatePanelViewModelFunction));
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.RemoveOpenSession(sessionChange.Session);
            }
        }
    }
}