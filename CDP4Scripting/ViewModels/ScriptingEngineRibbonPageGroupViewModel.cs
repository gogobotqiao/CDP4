﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ScriptingEngineRibbonPageGroupViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Scripting.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Dal;
    using CDP4Dal.Events;
    using DevExpress.XtraPrinting.Native;
    using Events;
    using Helpers;
    using Interfaces;
    using ReactiveUI;
    using Views;

    /// <summary>
    /// The view-model of the <see cref="ScriptingEngineRibbonPageGroup"/>
    /// </summary>
    [Export(typeof(ScriptingEngineRibbonPageGroupViewModel)), PartCreationPolicy(CreationPolicy.Shared)] 
    public class ScriptingEngineRibbonPageGroupViewModel : ReactiveObject
    {
        /// <summary>
        /// A <see cref="IScriptingProxy"/> object which perfoms the commands called in the scripts.
        /// </summary>
        private readonly IScriptingProxy scriptingProxy;

        /// <summary>
        /// The initial path when a dialog is opened.
        /// </summary>
        private readonly string initialDialogPath;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private IOpenSaveFileDialogService fileDialogService;

        /// <summary>
        /// Backing field for the <see cref="CollectionScriptPanelViewModels"/>
        /// </summary>
        private ObservableCollection<IScriptPanelViewModel> collectionScriptPanelViewModels;

        /// <summary>
        /// The file filters to use when a dialog is opened.
        /// </summary>
        public const string DialogFilters = "Lua Files (*.lua)|*.lua|Python Files (*.py)|*.py|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

        /// <summary>
        /// Backing field for <see cref="HasSession"/>
        /// </summary>
        private ObservableAsPropertyHelper<bool> hasSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingEngineRibbonPageGroupViewModel"/> class
        /// </summary>
        public ScriptingEngineRibbonPageGroupViewModel(IPanelNavigationService panelNavigationService, IOpenSaveFileDialogService fileDialogService, IScriptingProxy scriptingProxy)
        {
            
            if (panelNavigationService == null)
            {
                throw new ArgumentNullException(nameof(panelNavigationService));
            }

            if (fileDialogService == null)
            {
                throw new ArgumentNullException(nameof(fileDialogService));
            }

            if (scriptingProxy == null)
            {
                throw new ArgumentNullException(nameof(scriptingProxy));
            }

            this.PanelNavigationService = panelNavigationService;
            this.fileDialogService = fileDialogService;
            this.scriptingProxy = scriptingProxy;

            CDPMessageBus.Current.Listen<NavigationPanelEvent>()
                .Where(x => x.ViewModel.ToString().Contains("ScriptPanelViewModel") && x.PanelStatus == PanelStatus.Closed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.HandleClosedPanel);

            this.OpenSessions = new ReactiveList<ISession>();
            this.OpenSessions.ChangeTrackingEnabled = true;
            this.OpenSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession);
            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.initialDialogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "file.txt");
            this.PathScriptingFiles = new Dictionary<string, string>();
            this.CollectionScriptPanelViewModels = new ObservableCollection<IScriptPanelViewModel>();
            CDPMessageBus.Current.Listen<ScriptPanelEvent>().Subscribe(this.ScriptPanelEventHandler);

            this.NewPythonScriptCommand = ReactiveCommand.Create();
            this.NewPythonScriptCommand.Subscribe(_ => this.ExecuteCreateNewScript("python", ScriptingLaguageKindSupported.Python));

            this.NewLuaScriptCommand = ReactiveCommand.Create();
            this.NewLuaScriptCommand.Subscribe(_ => this.ExecuteCreateNewScript("lua", ScriptingLaguageKindSupported.Lua));

            this.NewTextScriptCommand = ReactiveCommand.Create();
            this.NewTextScriptCommand.Subscribe(_ => this.ExecuteCreateNewScript("text", ScriptingLaguageKindSupported.Text));

            this.OpenScriptCommand = ReactiveCommand.Create();
            this.OpenScriptCommand.Subscribe(_ => this.OpenScriptFile());

            this.SaveAllCommand = ReactiveCommand.Create();
            this.SaveAllCommand.Subscribe(_ => this.SaveAllScripts());
        }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> used to navigate to Panels.
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets and sets the tabitems
        /// </summary>
        public ObservableCollection<IScriptPanelViewModel> CollectionScriptPanelViewModels
        {
            get { return this.collectionScriptPanelViewModels; }
            set { this.RaiseAndSetIfChanged(ref this.collectionScriptPanelViewModels, value); }
        }

        /// <summary>
        /// Creates a new python tab.
        /// </summary>
        public ReactiveCommand<object> NewPythonScriptCommand { get; private set; }

        /// <summary>
        /// Creates a new Lua tab.
        /// </summary>
        public ReactiveCommand<object> NewLuaScriptCommand { get; private set; }

        /// <summary>
        /// Creates a new text tab.
        /// </summary>
        public ReactiveCommand<object> NewTextScriptCommand { get; private set; }

        /// <summary>
        /// Shows a dialog window to select a python file and import it into the texteditor
        /// </summary>
        public ReactiveCommand<object> OpenScriptCommand { get; private set; }

        /// <summary>
        /// Saves all the scripts currently open.
        /// </summary>
        public ReactiveCommand<object> SaveAllCommand { get; private set; }

        /// <summary>
        /// Gets or sets the list of the paths that correspond to the files open in the scripting engine.
        /// The key is the header of the tab item and the value is the path of the file associated to this tab item.
        /// </summary>
        public Dictionary<string, string> PathScriptingFiles { get; set; }

        /// <summary>
        /// Gets a list of open <see cref="ISession"/>s
        /// </summary>
        public ReactiveList<ISession> OpenSessions { get; private set; }

        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasSession
        {
            get { return this.hasSession.Value; }
        }

        /// <summary>
        /// Calls the <see cref="CreateNewScript"/> method and pass as title agrument, the language used and the number of <see cref="IScriptPanelViewModel"/> open.
        /// </summary>
        /// <param name="panelTitle">The title of the panel associated to this view model.</param>
        /// <param name="scriptingLanguage">The language of the new script.</param>
        private void ExecuteCreateNewScript(string panelTitle, ScriptingLaguageKindSupported scriptingLanguage)
        {
            var panelCounter = this.CollectionScriptPanelViewModels.Count - 1;
            bool captionExists = true;
            while (captionExists)
            {
                panelCounter++;
                captionExists = this.CollectionScriptPanelViewModels.Any(x => x.Caption == panelTitle + panelCounter);
            }

            this.CreateNewScript(panelTitle + panelCounter, scriptingLanguage);
        }

        /// <summary>
        /// Creates a new panel which contains a script.
        /// </summary>
        /// <param name="panelTitle">The title of the panel associated to this view model.</param>
        /// <param name="scriptingLanguage">The language of the new script.</param>
        private IScriptPanelViewModel CreateNewScript(string panelTitle, ScriptingLaguageKindSupported scriptingLanguage)
        {
            IScriptPanelViewModel scriptPanelViewModel;
            switch (scriptingLanguage)
            {
                case ScriptingLaguageKindSupported.Python:
                    scriptPanelViewModel = new PythonScriptPanelViewModel(panelTitle, this.scriptingProxy, this.OpenSessions);
                    break;
                case ScriptingLaguageKindSupported.Lua:
                    scriptPanelViewModel = new LuaScriptPanelViewModel(panelTitle, this.scriptingProxy, this.OpenSessions);
                    break;
                case ScriptingLaguageKindSupported.Text:
                    scriptPanelViewModel = new TextScriptPanelViewModel(panelTitle, this.scriptingProxy, this.OpenSessions);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The {0} is not supported", scriptingLanguage));
            }

            this.PanelNavigationService.Open(scriptPanelViewModel as IPanelViewModel, true);
            this.CollectionScriptPanelViewModels.Add(scriptPanelViewModel);
            return scriptPanelViewModel;
        }

        /// <summary>
        /// Open a new panel with the selected script file.
        /// </summary>
        private void OpenScriptFile()
        {
            // Open the dialog to open a file
            var filePaths = this.fileDialogService.GetOpenFileDialog(false, false, true, DialogFilters, "*.*", this.initialDialogPath, 4);
            if (filePaths == null || filePaths.IsEmpty())
            {
                return;
            }

            foreach (var filePath in filePaths)
            {
                // if the path is invalid got to the next one
                // if the file is already open, it cannot be open a second time 
                if (string.IsNullOrEmpty(filePath) || this.PathScriptingFiles.ContainsValue(filePath))
                {
                    continue;
                }

                var fileName = Path.GetFileName(filePath);
                var fileExtension = Path.GetExtension(filePath);

                // Check if the extension is supported and create the panel associated
                IScriptPanelViewModel scriptPanelViewModel;
                switch (fileExtension)
                {
                    case ".lua":
                        {
                            scriptPanelViewModel = this.CreateNewScript(fileName, ScriptingLaguageKindSupported.Lua);
                            break;
                        }
                    case ".py":
                        {
                            scriptPanelViewModel = this.CreateNewScript(fileName, ScriptingLaguageKindSupported.Python);
                            break;
                        }
                    case ".txt":
                        {
                            scriptPanelViewModel = this.CreateNewScript(fileName, ScriptingLaguageKindSupported.Text);
                            break;
                        }
                    default:
                        {
                            throw new NotSupportedException(string.Format("The filextension ({0}) is not supported", fileExtension));
                        }
                }

                scriptPanelViewModel.AvalonEditor.Text = File.ReadAllText(filePath);
                scriptPanelViewModel.IsDirty = false;
                this.PathScriptingFiles.Add(scriptPanelViewModel.Caption, filePath);
            }
        }

        /// <summary>
        /// Save the script contained in <see cref="scriptPanelViewModel"/>. 
        /// The first time it will show a dialog to save the script and the next times it will overwrite the data.
        /// </summary>
        /// <param name="scriptPanelViewModel">The <see cref="IScriptPanelViewModel"/> that contains the script.</param>
        private void SaveScript(IScriptPanelViewModel scriptPanelViewModel)
        {
            var contentScript = scriptPanelViewModel.AvalonEditor.Text;

            string header;
            // Check if the content of the Panel is dirty or not to not include the * in the name of the file saved 
            if (scriptPanelViewModel.Caption.EndsWith("*"))
            {
                header = scriptPanelViewModel.Caption.Remove(scriptPanelViewModel.Caption.Length - 1);
            }
            else
            {
                header = scriptPanelViewModel.Caption;
            }

            string filePath;
            if (this.PathScriptingFiles.TryGetValue(header, out filePath) && File.Exists(filePath))
            {
                File.WriteAllText(filePath, contentScript);
                scriptPanelViewModel.IsDirty = false;
                return;
            }

            // Open the dialog to save the file
            var extension = scriptPanelViewModel.FileExtension;
            var filterIndex = this.FindFilterIndex(extension);
            filePath = this.fileDialogService.GetSaveFileDialog(header, extension, DialogFilters, this.initialDialogPath, filterIndex);
            var fileName = Path.GetFileName(filePath);

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(String.Format("An error occured with the path or the name of the file, the script has not been saved. " +
                             "Please verify that the path '{0}' amd the name '{1}' are not empty.", filePath, fileName));
            }

            File.WriteAllText(filePath, contentScript);

            // Update the dictionnary which contains the paths of the open tabs 
            if (this.PathScriptingFiles.ContainsKey(header))
            {
                this.PathScriptingFiles.Remove(header);
            }

            scriptPanelViewModel.Caption = fileName;
            this.PathScriptingFiles.Add(fileName, filePath);
            scriptPanelViewModel.IsDirty = false;
        }

        /// <summary>
        /// Saves all the scripts currently open.
        /// </summary>
        private void SaveAllScripts()
        {
            foreach (var scriptPanelViewModel in CollectionScriptPanelViewModels)
            {
                this.SaveScript(scriptPanelViewModel);
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Session"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        private void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (sessionChange.Status == SessionStatus.Open)
            {
                this.OpenSessions.Add(sessionChange.Session);
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.OpenSessions.Remove(sessionChange.Session);
            }
        }

        /// <summary>
        /// Removes a <see cref="IScriptPanelViewModel"/> from the <see cref="CollectionScriptPanelViewModels"/> and its information stored in the <see cref="PathScriptingFiles"/>.
        /// </summary>
        /// <param name="navigationPanelEvent"> The payload of the event that is being handled.</param>
        private void HandleClosedPanel(NavigationPanelEvent navigationPanelEvent)
        {
            var scriptPanelViewModel = (IScriptPanelViewModel) navigationPanelEvent.ViewModel;
            var header = scriptPanelViewModel.Caption;
            if (this.PathScriptingFiles.ContainsKey(header))
            {
                this.PathScriptingFiles.Remove(header);
            }

            this.CollectionScriptPanelViewModels.Remove(scriptPanelViewModel);
        }

        /// <summary>
        /// The event handler that listens for updates on the <see cref="IScriptPanelViewModel"/>. 
        /// </summary>
        /// <param name="scriptPanelEvent">The payload of the event that is being handled.</param>
        private void ScriptPanelEventHandler(ScriptPanelEvent scriptPanelEvent)
        {
            if (scriptPanelEvent.Status == ScriptPanelStatus.Saved)
            {
                this.SaveScript(scriptPanelEvent.ScriptPanelViewModel);
            }
        }

        /// <summary>
        /// Finds the filter index associated to the extension in the <see cref="DialogFilters"/>.
        /// </summary>
        /// <param name="extension">The extension for which we want the index.</param>
        /// <returns>The filter index.</returns>
        public int FindFilterIndex(string extension)
        {
            var filterIndex = 1;
            extension = string.Concat("(", extension, ")");
            var filters = DialogFilters.Split('|');
            for (int i = 0; i < filters.Length; i++)
            {
                if (filters[i].Contains(extension))
                {
                    filterIndex = i / 2 + 1;
                    return filterIndex;
                }
            }

            return filterIndex;
        }
    }
}