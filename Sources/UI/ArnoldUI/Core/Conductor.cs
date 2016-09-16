﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GoodAI.Arnold.Communication;
using GoodAI.Logging;

namespace GoodAI.Arnold.Core
{
    public interface IConductor : IDisposable
    {
        ICoreProxy CoreProxy { get; }

        event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;
        event EventHandler<StateChangedEventArgs> StateChanged;

        Task ConnectToCoreAsync(EndPoint endPoint = null);
        void Disconnect();
        Task ShutdownAsync();
        Task LoadBlueprintAsync(string blueprint);
        Task UpdateConfigurationAsync(Action<CoreConfiguration> updateConfig);
        Task StartSimulationAsync(uint stepsToRun = 0);
        Task PauseSimulationAsync();
        Task ClearBlueprintAsync();
        Task PerformBrainStepAsync();
        Task RunToBodyStepAsync();

        bool IsConnected { get; }

        CoreState CoreState { get; }

        CoreConfiguration CoreConfig { get; }

        IModelProvider ModelProvider { get; }
    }

    public class Conductor : IConductor
    {
        // Injected.
        public ILog Log { get; set; } = NullLogger.Instance;

        public event EventHandler<StateChangedEventArgs> StateChanged;
        public event EventHandler<StateChangeFailedEventArgs> StateChangeFailed;

        public ICoreProxy CoreProxy { get; private set; }
        public IModelProvider ModelProvider { get; }

        public bool IsConnected => CoreProxy != null;

        public CoreState CoreState => CoreProxy?.State ?? CoreState.Disconnected;

        public CoreConfiguration CoreConfig { get; } = new CoreConfiguration(new SystemConfiguration());

        private readonly ICoreControllerFactory m_coreControllerFactory;

        private readonly ICoreProcessFactory m_coreProcessFactory;
        private ICoreProcess m_process;

        private readonly ICoreLinkFactory m_coreLinkFactory;

        private readonly ICoreProxyFactory m_coreProxyFactory;
        private readonly IModelUpdaterFactory m_modelUpdaterFactory;

        public Conductor(ICoreProcessFactory coreProcessFactory, ICoreLinkFactory coreLinkFactory,
            ICoreControllerFactory coreControllerFactory, ICoreProxyFactory coreProxyFactory,
            IModelUpdaterFactory modelUpdaterFactory, IModelProviderFactory modelProviderFactory)
        {
            m_coreProcessFactory = coreProcessFactory;
            m_coreLinkFactory = coreLinkFactory;
            m_coreControllerFactory = coreControllerFactory;
            m_coreProxyFactory = coreProxyFactory;
            m_modelUpdaterFactory = modelUpdaterFactory;

            ModelProvider = modelProviderFactory.Create(this);
        }

        public async Task ConnectToCoreAsync(EndPoint endPoint = null)
        {
            if (CoreProxy != null)
            {
                Log.Error("Cannot connect to core, there is still a core proxy present");
                throw new InvalidOperationException(
                    "There is still a core proxy present");
            }

            if (endPoint == null)
            {
                if (m_process == null)
                {
                    // TODO(HonzaS): Move this elsewhere when we have finer local process control.
                    Log.Info("Starting a local core process");
                    m_process = m_coreProcessFactory.Create();
                }

                endPoint = m_process.EndPoint;
            }

            if (endPoint == null)
            {
                Log.Error("Endpoint not set up, cannot connect to core");
                throw new InvalidOperationException("Endpoint not set");
            }

            Log.Info("Connecting to Core running at {hostname:l}:{port}", endPoint.Hostname, endPoint.Port);
            ICoreLink coreLink = m_coreLinkFactory.Create(endPoint);
            // TODO(HonzaS): Check if the endpoint exists (handshake), await the response.


            // TODO(HonzaS): Move these inside the factory method.
            ICoreController coreController = m_coreControllerFactory.Create(coreLink);
            IModelUpdater modelUpdater = m_modelUpdaterFactory.Create(coreLink, coreController);

            CoreProxy = m_coreProxyFactory.Create(coreController, modelUpdater);

            RegisterCoreEvents();
        }

        private void RegisterCoreEvents()
        {
            CoreProxy.StateChanged += OnCoreStateChanged;
            CoreProxy.StateChangeFailed += OnCoreStateChangeFailed;
            CoreProxy.CommandTimedOut += OnCoreCommandTimedOut;
            CoreProxy.Disconnected += OnDisconnected;
        }

        private void UnregisterCoreEvents()
        {
            CoreProxy.StateChanged -= OnCoreStateChanged;
            CoreProxy.StateChangeFailed -= OnCoreStateChangeFailed;
            CoreProxy.CommandTimedOut -= OnCoreCommandTimedOut;
            CoreProxy.Disconnected -= OnDisconnected;
        }

        public void Disconnect()
        {
            if (CoreProxy == null)
                return;

            // TODO(HonzaS): We might want to ask the user if he wants to kill the local process when disconnecting.
            //if (m_process != null)
            //{
            //}

            FinishDisconnect();
        }

        public async Task ShutdownAsync()
        {
            if (!IsConnected)
            {
                Log.Warn("Cannot shut down, not connected.");
                return;
            }

            if (CoreProxy != null)
            {
                Log.Info("Shutting down the core");
                await CoreProxy.ShutdownAsync();
            }

            AfterShutdown();
        }

        private void OnCoreCommandTimedOut(object sender, TimeoutActionEventArgs args)
        {
            Log.Debug("Core command {command} timed out", args.Command);
            if (args.Command == CommandType.Shutdown)
            {
                args.Action = TimeoutAction.Cancel;

                AfterShutdown();
            }
        }

        private void OnDisconnected(object sender, EventArgs args)
        {
            AfterShutdown();
        }

        private void AfterShutdown()
        {
            KillLocalCore();
            FinishDisconnect();
        }

        private void KillLocalCore()
        {
            // For remote core this is already null, for local core we have lost network contact with it.
            m_process?.Dispose();
            m_process = null;
        }

        private void FinishDisconnect()
        {
            CoreState oldState = CoreProxy.State;

            UnregisterCoreEvents();

            CoreProxy.Dispose();
            CoreProxy = null;

            StateChanged?.Invoke(this, new StateChangedEventArgs(oldState, CoreState.Disconnected));
            Log.Info("Disconnected from core");
        }

        public async Task LoadBlueprintAsync(string blueprint)
        {
            Log.Info("Loading blueprint");
            await CoreProxy.LoadBlueprintAsync(blueprint);
        }

        private async Task SendConfigurationAsync(CoreConfiguration coreConfiguration)
        {
            Log.Info("Sending core configuration");
            await CoreProxy.SendConfigurationAsync(coreConfiguration);
        }

        public async Task UpdateConfigurationAsync(Action<CoreConfiguration> updateConfig)
        {
            // TODO(Premek): Backup settings and keep old value when request fails.
            updateConfig(CoreConfig);

            if (CoreState == CoreState.Disconnected || CoreState == CoreState.ShuttingDown)
                return;

            await SendConfigurationAsync(CoreConfig);
        }

        private async void OnCoreStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            Log.Debug("Core state changed: {previousState} -> {currentState}", stateChangedEventArgs.PreviousState, stateChangedEventArgs.CurrentState);

            StateChanged?.Invoke(this, stateChangedEventArgs);

            var newState = stateChangedEventArgs.CurrentState;
            if (stateChangedEventArgs.PreviousState == CoreState.Disconnected
                && (newState == CoreState.Empty || newState == CoreState.Paused || newState == CoreState.Running))
            {
                // TODO(Premek): Maybe do it only when some chenges are pending?
                Log.Debug("Updating core configuration after connecting");
                await SendConfigurationAsync(CoreConfig);
            }
        }

        private void OnCoreStateChangeFailed(object sender, StateChangeFailedEventArgs stateChangeFailedEventArgs)
        {
            Log.Warn("Core state change failed with: {error}", stateChangeFailedEventArgs.ErrorMessage);
            StateChangeFailed?.Invoke(this, stateChangeFailedEventArgs);
        }

        public async Task StartSimulationAsync(uint stepsToRun = 0)
        {
            if (CoreProxy == null)
            {
                Log.Error("Cannot start simulation, not connected to a core");
                throw new InvalidOperationException("Core proxy does not exist, cannot start");
            }

            Log.Info("Starting simulation");
            await CoreProxy.RunAsync(stepsToRun);
        }

        public async Task PauseSimulationAsync()
        {
            Log.Info("Pausing simulation");
            await CoreProxy.PauseAsync();
        }

        public async Task RunToBodyStepAsync()
        {
            Log.Info("Running to next body step");
            await CoreProxy.RunAsync(runToBodyStep: true);
        }

        public async Task ClearBlueprintAsync()
        {
            Log.Info("Clearing blueprint");
            await CoreProxy.ClearAsync();
        }

        public async Task PerformBrainStepAsync()
        {
            if (CoreProxy == null)
            {
                Log.Error("Cannot perform brain step, not connected to a core");
                throw new InvalidOperationException("Core proxy does not exist, cannot step");
            }

            Log.Info("Performing brain step");
            await CoreProxy.RunAsync(1);
        }

        public async void Dispose()
        {
            Log.Debug("Disposing conductor");
            if (m_process != null)
            {
                await ShutdownAsync();
                // Process will wait for a while to let the core process finish gracefully.
                // TODO(HonzaS): Wait for the core to finish before killing.
                // When the local simulation is auto-saving before closing, it might take a long time.
                m_process.Dispose();
            }
            else
            {
                Disconnect();
            }
        }
    }
}
