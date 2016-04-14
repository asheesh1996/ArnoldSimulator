﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArnoldUI;
using GoodAI.Arnold.Simulation;
using GoodAI.Arnold.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace GoodAI.Arnold
{
    public partial class MainForm : Form
    {
        private readonly UIMain m_uiMain;
        public LogForm LogForm { get; }
        public GraphForm GraphForm { get; }
        public VisualizationForm VisualizationForm { get; set; }

        public MainForm()
        {
            InitializeComponent();

            m_uiMain = new UIMain();

            LogForm = new LogForm();
            LogForm.Show(dockPanel, DockState.DockBottom);

            GraphForm = new GraphForm();
            GraphForm.Show(dockPanel, DockState.Document);

            // TODO(HonzaS): The blueprint should be in the Designer later.
            GraphForm.AgentBlueprint = m_uiMain.AgentBlueprint;

            m_uiMain.SimulationStateUpdated += SimulationOnStateUpdated;
            m_uiMain.SimulationStateChangeFailed += SimulationOnStateChangeFailed;

            PopulateCoreTypes();

            UpdateButtons();
            //Simulation = new RemoteSimulation(new CoreLink(new ConverseProtoBufClient(new DummyConnector())));
        }

        private void UpdateButtons()
        {
            // TODO(HonzaS): Check proxy + sim state and update buttons.
        }

        private void PopulateCoreTypes()
        {
            coreTypeComboBox.Items.Add("Local");
            coreTypeComboBox.SelectedIndex = 0;
            // TODO(HonzaS): Add saved remotes.

            // TODO(HonzaS): Dialog for adding new remotes.
            //coreTypeComboBox.Items.Add("Add remote...");
        }


        private void SimulationOnStateUpdated(object sender, StateUpdatedEventArgs stateUpdatedEventArgs)
        {
        }

        private void SimulationOnStateChangeFailed(object sender, StateChangeFailedEventArgs e)
        {
        }

        private void VisualizationFormOnClosed(object sender, FormClosedEventArgs e)
        {
            VisualizationForm.FormClosed -= VisualizationFormOnClosed;
            m_uiMain.VisualizationClosed();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void StartSimulation()
        {
            m_uiMain.StartSimulation();

            if (VisualizationForm == null || VisualizationForm.IsDisposed)
                VisualizationForm = new VisualizationForm(m_uiMain.Conductor);

            VisualizationForm.Show();
            VisualizationForm.FormClosed += VisualizationFormOnClosed;
        }
    }
}
