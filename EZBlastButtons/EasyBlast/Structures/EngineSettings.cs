﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTCV.UI;
using RTCV.UI.Components.EngineConfig.EngineControls;
using Ceras;
using RTCV.CorruptCore;
using Newtonsoft.Json;
using RTCV.NetCore;
using RTCV.Common;

namespace EZBlastButtons.Structures
{
    [Serializable]
    [Ceras.MemberConfig(TargetMember.All)]
    public class EngineSettings
    {
        [JsonProperty]
        public string DisplayName { get; set; } = null;

        [JsonProperty]
        public long ForcedIntensity { get; set; } = -1;
        [JsonProperty]
        public double Percentage { get; set; } = 1.0;
        public int Alignment => cachedSpec.Get<int>(RTCSPEC.CORE_CURRENTALIGNMENT);
        public int Precision => cachedSpec.Get<int>(RTCSPEC.CORE_CURRENTPRECISION);
        public int EngineIndex => C.EngineToIndex(EngineType);

        [JsonProperty]
        public CorruptionEngine EngineType { get; private set; }
        [JsonProperty]
        public string[] Domains { get; set; } = null;

        protected string PercentageString { get { if (ForcedIntensity > 0) return $"[{ForcedIntensity,7}]"; else return $"[{Percentage * 100.0,6:0.00}%]"; } }
        [JsonProperty]
        public string[] ExtraData { get; set; } = null;


        [Ceras.Include]
        [JsonProperty]
#pragma warning disable IDE0044 // Add readonly modifier
        private PartialSpec cachedSpec = null;

#pragma warning restore IDE0044 // Add readonly modifier

        [Ceras.Exclude]
        public PartialSpec CachedSpec => cachedSpec;

        /// <summary>
        /// for ceras
        /// </summary>
        public EngineSettings()
        {

        }

        /// <summary>
        /// Please call base constructor
        /// </summary>
        public EngineSettings(PartialSpec template)
        {
            PartialSpec partial = new PartialSpec(C.TARGET_SPEC_NAME);
            if (template != null) partial.Insert(template);

            partial[RTCSPEC.CORE_CURRENTPRECISION] = RtcCore.CurrentPrecision;
            partial[RTCSPEC.CORE_CURRENTALIGNMENT] = RtcCore.Alignment;
            cachedSpec = partial;
            EngineType = RtcCore.SelectedEngine;

            //TODO: get limiter and value strings
            if (EngineType == CorruptionEngine.VECTOR)
            {
                var settingsControl = S.GET<CorruptionEngineForm>();

                ExtraData = new string[2];

                string lim = ((ComboBoxItem<string>)settingsControl.VectorEngineControl.cbVectorLimiterList.SelectedItem).Name;
                string val = ((ComboBoxItem<string>)settingsControl.VectorEngineControl.cbVectorValueList.SelectedItem).Name;
                ExtraData[0] = lim;
                ExtraData[1] = val;
            }
            else if (EngineType == CorruptionEngine.CLUSTER)
            {
                var settingsControl = S.GET<CorruptionEngineForm>();
                ExtraData = new string[1];
                string lim = ((ComboBoxItem<string>)settingsControl.ClusterEngineControl.cbClusterLimiterList.SelectedItem).Name;
                ExtraData[0] = lim;
            }

            UpdateCache();
        }

        public EngineSettings Duplicate()
        {
            var s = new EngineSettings();
            s.cachedSpec = new PartialSpec(cachedSpec.Name);
            s.cachedSpec.Insert(cachedSpec);
            if(ExtraData != null)
            {
                s.ExtraData = (string[])ExtraData.Clone();
            }
            s.EngineType = EngineType;
            s.Percentage = Percentage;
            s.ForcedIntensity = ForcedIntensity;
            s.DisplayName = DisplayName;
            s.Domains = (string[])Domains.Clone();
            return s;
        }

        public void Extract(EngineSettings s)
        {
            cachedSpec = new PartialSpec(s.cachedSpec.Name);
            cachedSpec.Insert(s.cachedSpec);
            if (s.ExtraData != null)
            {
                ExtraData = (string[])s.ExtraData.Clone();
            }
            else
            {
                ExtraData = null;
            }
            EngineType = s.EngineType;
            Percentage = s.Percentage;
            ForcedIntensity = s.ForcedIntensity;
            DisplayName = s.DisplayName;
            Domains = (string[])s.Domains.Clone();
        }


        /// <summary>
        /// Ensures it doesn't completely break when loading old multi engine files
        /// </summary>
        public void EnsureSpecUpdated()
        {
            PartialSpec partial = new PartialSpec(C.TARGET_SPEC_NAME);
            partial.Insert(this.cachedSpec);
            cachedSpec = partial;
        }
        /// <summary>
        /// Only override if using a different spec from <see cref="AllSpec.CorruptCoreSpec"/>. Updates without pushing
        /// </summary>
        public virtual void ApplyPartial()
        {
            AllSpec.CorruptCoreSpec.Update(cachedSpec, false, false);
        }

        private void UpdateCache()
        {
            cachedSpec.ExtractFrom(AllSpec.CorruptCoreSpec.GetPartialSpec());
        }

        public virtual void Extract(CorruptionEngineForm form)
        {
            EngineType = RtcCore.SelectedEngine;
            UpdateCache();
        }

        public override string ToString()
        {
            return $"{PercentageString} {DisplayName ?? "TODO"} " + GetTypeSpecificStr();
        }

        //TODO: fill out
        private string GetTypeSpecificStr()
        {
            switch (EngineType)
            {
                case CorruptionEngine.NIGHTMARE:
                    return $"";
                case CorruptionEngine.HELLGENIE:
                    return $"";
                case CorruptionEngine.DISTORTION:
                    return $"";
                case CorruptionEngine.FREEZE:
                    return $"";
                case CorruptionEngine.PIPE:
                    return $"";
                case CorruptionEngine.VECTOR:
                    return $"";
                case CorruptionEngine.CLUSTER:
                    return $"";
                default:
                    return "NOT SUPPORTED";
            }
        }

        //Switch
        public virtual BlastUnit[] GetBlastUnits(string domain, long address, int precision, int alignment)
        {
            switch (EngineType)
            {
                case CorruptionEngine.NIGHTMARE:
                    return new BlastUnit[] { NightmareEngine.GenerateUnit(domain, address, precision, alignment) };
                case CorruptionEngine.HELLGENIE:
                    return new BlastUnit[] { HellgenieEngine.GenerateUnit(domain, address, precision, alignment) };
                case CorruptionEngine.DISTORTION:
                    return new BlastUnit[] { DistortionEngine.GenerateUnit(domain, address, precision, alignment) };
                case CorruptionEngine.FREEZE:
                    return new BlastUnit[] { FreezeEngine.GenerateUnit(domain, address, precision, alignment) };
                case CorruptionEngine.PIPE:
                    return new BlastUnit[] { PipeEngine.GenerateUnit(domain, address, precision, alignment) };
                case CorruptionEngine.VECTOR:
                    return new BlastUnit[] { VectorEngine.GenerateUnit(domain, address, alignment) };
                case CorruptionEngine.CLUSTER:
                    var clusterResult = ClusterEngine.GenerateUnit(domain, address, alignment);
                    if (clusterResult == null || clusterResult.Length == 0)
                    {
                        return new BlastUnit[] { null };
                    }
                    else
                    {
                        return clusterResult;
                    }
                default:
                    return null;
            }
        }

        //public virtual void PreCorrupt() { }

    }
}
