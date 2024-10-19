﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EZBlastButtons.Structures;
using RTCV;
using RTCV.Common;
using RTCV.CorruptCore;
using RTCV.NetCore;
using RTCV.UI;
using RTCV.UI.Components.EngineConfig.EngineControls;
using RTCV.UI.Modular;
//using RTCV.UI.Components.EngineConfig;

namespace EZBlastButtons
{
    public partial class EngineSettingsForm : ComponentForm, IColorize
    {
        //string curEngine = "None";
        CorruptionEngineForm settingsControl;

        //private CorruptionEngineForm mainSettings;
        public EngineSettings OutputSettings { get; private set; } = null;
        EngineSettings edit = null;
        //private MemoryDomainsForm myDomains;

        bool settingsPopoutAllowed = false;
        //Panel prevPanel = null;

        public EngineSettingsForm()
        {
            InitializeComponent();

            //TODO: get plugin engines too
            //previousPrecision = RtcCore.CurrentPrecision;
            bAdd.Enabled = false;

            Setup();

            Shown += ShownNoEdit;

            FormClosing += EngineSettingsForm_FormClosing;
        }

        private async void ShownNoEdit(object sender, EventArgs e)
        {
            //await Task.Delay(1500);
            //mySettings.cbCustomPrecision.SelectedIndex = C.PrecisionToIndex(previousPrecision);
            //SetPrecisionIndex(previousPrecision);
            settingsControl.cbSelectedEngine.SelectedIndex = C.NightmareEngineIndex;
            bAdd.Enabled = true;
        }

        public EngineSettingsForm(EngineSettings edit)
        {
            InitializeComponent();
            //previousPrecision = RtcCore.CurrentPrecision;
            //editPrecision = edit.Precision;
            bAdd.Enabled = false;

            Setup();
            this.edit = edit;

            //Add previous
            if (edit.Domains != null && edit.Domains.Length > 0)
            {
                foreach (var item in edit.Domains)
                {
                    if (!lbMemoryDomains.Items.Contains(item))
                    {
                        lbMemoryDomains.Items.Add(item);
                    }
                }
            }

            bAdd.Text = "Save";
            this.Shown += ShownEdit;
            FormClosing += EngineSettingsForm_FormClosing;
            //pSettings.Controls.Add(cSettings);
        }


        private async void ShownEdit(object sender, EventArgs e)
        {
            edit.ApplyPartial();
            settingsControl.cbSelectedEngine.SelectedIndex = edit.EngineIndex;
            settingsControl.ResyncAllEngines();
            bAdd.Enabled = true;
            nmIntensity.Value = (decimal)(edit.Percentage * 100.0);
            tbName.Text = edit.DisplayName ?? "";
            if (edit.ForcedIntensity > 0 && edit.ForcedIntensity < nmForcedIntensity.Maximum)
            {
                cbForceIntensity.Checked = true;
                nmForcedIntensity.Value = edit.ForcedIntensity;
            }

            lbMemoryDomains.ClearSelected();
            if (edit.Domains != null)
            {
                foreach (var dom in edit.Domains)
                {
                    lbMemoryDomains.SetSelected(lbMemoryDomains.Items.IndexOf(dom), true);
                }
            }
        }

        void Setup()
        {
            object maxIntensity = AllSpec.VanguardSpec[VSPEC.OVERRIDE_DEFAULTMAXINTENSITY];
            if (maxIntensity != null && maxIntensity is int maxintensity)
            {
                nmForcedIntensity.Maximum = maxintensity;
            }

            settingsControl = S.GET<CorruptionEngineForm>();
            settingsControl.cbSelectedEngine.SelectedIndexChanged += CheckEngineCompatability;

            //Detach from previous location
            //Anchor to previous idk
            //settingsControl.AnchorToPanel(settingsControl.Parent as Panel);
            settingsPopoutAllowed = settingsControl.popoutAllowed;
            settingsControl.popoutAllowed = false;

            //prevPanel = settingsControl.Parent as Panel;
            settingsControl.AnchorToPanel(pSettings);
            settingsControl.Show();
            UISideHooks.KillSwitchFired += UISideHooks_KillSwitchFired;

            imgWarning.Image = System.Drawing.SystemIcons.Warning.ToBitmap();
            ToolTip warningToolTip = new ToolTip();
            warningToolTip.ToolTipIcon = ToolTipIcon.Warning;
            warningToolTip.ToolTipTitle = "Engine not supported";
            warningToolTip.SetToolTip(imgWarning, "The currently selected engine is not supported");
            imgWarning.Visible = false;
            warningToolTip.AutoPopDelay = 1200000;


            imgDomainOverrideInfo.Image = System.Drawing.SystemIcons.Information.ToBitmap();
            ToolTip domainOverrideInfoToolTip = new ToolTip();
            domainOverrideInfoToolTip.ToolTipIcon = ToolTipIcon.Info;
            domainOverrideInfoToolTip.ToolTipTitle = "Domain Overrides";
            domainOverrideInfoToolTip.SetToolTip(imgDomainOverrideInfo, "If set, these selected domains will be used\r\ninstead of the domains selected in the\r\nmain window's \"Memory Domains\" panel.");
            domainOverrideInfoToolTip.AutoPopDelay = 1200000;

            imgIntensityInfo.Image = System.Drawing.SystemIcons.Information.ToBitmap();
            ToolTip intensityInfoToolTip = new ToolTip();
            intensityInfoToolTip.ToolTipIcon = ToolTipIcon.Info;
            intensityInfoToolTip.ToolTipTitle = "Intensity Settings";
            intensityInfoToolTip.SetToolTip(imgIntensityInfo, "Set the percentage of the intensity slider\r\nto be used with this engine setting.\r\nChecking Forced Intensity will override\r\nthe intensity slider.");
            intensityInfoToolTip.AutoPopDelay = 1200000;


            lbMemoryDomains.Items.AddRange(S.GET<MemoryDomainsForm>().lbMemoryDomains.Items);
        }

        private void CheckEngineCompatability(object sender, EventArgs e)
        {
            bool supported = C.IsEngineSupported(((ComboBox)sender).SelectedIndex);
            bAdd.Enabled = supported;
            imgWarning.Visible = !supported;
        }

        private void bAdd_Click(object sender, EventArgs e)
        {
            OutputSettings = null;

            switch (RtcCore.SelectedEngine)
            {
                case CorruptionEngine.NIGHTMARE:
                    OutputSettings = new EngineSettings(NightmareEngine.getDefaultPartial());
                    break;
                case CorruptionEngine.HELLGENIE:
                    OutputSettings = new EngineSettings(HellgenieEngine.getDefaultPartial());
                    break;
                case CorruptionEngine.DISTORTION:
                    OutputSettings = new EngineSettings(DistortionEngine.getDefaultPartial());
                    break;
                case CorruptionEngine.FREEZE:
                    OutputSettings = new EngineSettings(null);
                    break;
                case CorruptionEngine.PIPE:
                    OutputSettings = new EngineSettings(null);
                    break;
                case CorruptionEngine.VECTOR:
                    OutputSettings = new EngineSettings(VectorEngine.getDefaultPartial());
                    break;
                case CorruptionEngine.CLUSTER:
                    OutputSettings = new EngineSettings(ClusterEngine.getDefaultPartial());
                    break;
                default:
                    break;
            }

            if(OutputSettings == null)
            {
                MessageBox.Show("We aren't sure how you got here, but the selected engine is not supported for MultiEngine");
                return;
            }

            if (!string.IsNullOrWhiteSpace(tbName.Text)) { 
                OutputSettings.DisplayName = tbName.Text;
            }
            else
            {
                OutputSettings.DisplayName = C.EngineString(RtcCore.SelectedEngine);
            }

            if (cbForceIntensity.Checked)
            {
                OutputSettings.ForcedIntensity = (long)nmForcedIntensity.Value;
            }
            else
            {
                OutputSettings.Percentage = (double)nmIntensity.Value / 100.0;
            }

            OutputSettings.Extract(settingsControl);
            string[] domainList = new List<string>(lbMemoryDomains.SelectedItems.Cast<string>()).ToArray();
            OutputSettings.Domains = domainList;
            DialogResult = DialogResult.OK;
            
            Close();
        }

        
        private void EngineSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            settingsControl.cbCustomPrecision.SelectedIndexChanged -= CheckEngineCompatability;
            //Needed for restore to work
            this.Visible = false;
            //Give the settings control back to rtc window
            //settingsControl.RestoreToPreviousPanel();
            settingsControl.ParentComponentFormTitle.ReAnchorToPanel();
            settingsControl.popoutAllowed = settingsPopoutAllowed;
        }

        private void UISideHooks_KillSwitchFired()
        {
            UISideHooks.KillSwitchFired -= UISideHooks_KillSwitchFired;
            FormClosing -= EngineSettingsForm_FormClosing;
            settingsControl.cbCustomPrecision.SelectedIndexChanged -= CheckEngineCompatability;
            settingsControl.popoutAllowed = settingsPopoutAllowed;
            DialogResult = DialogResult.Abort;
            //settingsControl.AnchorToPanel();
            Close();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbMemoryDomains.Items.Count; i++)
            {
                lbMemoryDomains.SetSelected(i, true);
            }
        }

        private void btnUnselectDomains_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lbMemoryDomains.Items.Count; i++)
            {
                lbMemoryDomains.SetSelected(i, false);
            }
        }

        private void bAddDomain_Click(object sender, EventArgs e)
        {
            lbMemoryDomains.Items.Add(tbNewDomain.Text);
            lbMemoryDomains.SetSelected(lbMemoryDomains.Items.Count - 1, true);
            tbNewDomain.Clear();
        }

        private void cbForceIntensity_CheckedChanged(object sender, EventArgs e)
        {
            nmForcedIntensity.Enabled = cbForceIntensity.Checked;
            nmIntensity.Enabled = !cbForceIntensity.Checked;
        }
    }
}
