﻿using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Newtonsoft.Json;
using XIVAuras.Helpers;
using System.Linq;
using System.Collections.Generic;

namespace XIVAuras.Config
{

    public class VisibilityConfig : IConfigPage
    {
        [JsonIgnore] private string _customJobInput = string.Empty;
        
        public string Name => "Visibility";

        public bool AlwaysHide = false;
        public bool HideInPvP = false;
        public bool HideOutsidePvP = false;
        public bool HideInCombat = false;
        public bool HideOutsideCombat = false;
        public bool ShowWhenWeaponDrawn = false;
        public bool ShowInDuty = false;
        public bool HideOutsideDuty = false;
        public bool HideWhilePerforming = false;
        public bool HideInGoldenSaucer = false;
        public bool HideWhenSheathed = false;
        public bool Clip = false;
        public bool HideOutsideEureka = false;

        public bool HideIfLevel = false;
        public TriggerDataOp HideIfLevelOp = TriggerDataOp.LessThan;
        public int HideIfLevelValue = 90;

        public JobType ShowForJobTypes = JobType.All;
        public string CustomJobString = string.Empty;
        public List<Job> CustomJobList = new List<Job>();

        public IConfigPage GetDefault() => new VisibilityConfig();

        public bool IsVisible(bool parentVisible)
        {
            if (this.AlwaysHide)
            {
                return false;
            }

            if (this.HideInPvP && CharacterState.IsInPvP())
            {
                return false;
            }

            if (this.HideOutsidePvP && !CharacterState.IsInPvP())
            {
                return false;
            }

            if (this.HideInCombat && CharacterState.IsInCombat())
            {
                return false;
            }

            if (this.HideOutsideCombat && !CharacterState.IsInCombat())
            {
                if (this.ShowWhenWeaponDrawn && CharacterState.IsWeaponDrawn())
                {
                    return true;
                }
                if (this.ShowInDuty && CharacterState.IsInDuty())
                {
                    return true;
                }
                return false;
            }

            if (this.HideOutsideDuty && !CharacterState.IsInDuty())
            {
                return false;
            }

            if (this.HideOutsideEureka && !CharacterState.IsInEureka())
            {
                return false;
            }

            if (this.HideWhilePerforming && CharacterState.IsPerforming())
            {
                return false;
            }

            if (this.HideInGoldenSaucer && CharacterState.IsInGoldenSaucer())
            {
                return false;
            }

            if (this.HideWhenSheathed && !CharacterState.IsWeaponDrawn())
            {
                return false;
            }

            if (this.HideIfLevel &&
                Utils.GetResult(CharacterState.GetCharacterLevel(), this.HideIfLevelOp, this.HideIfLevelValue))
            {
                return false;
            }

            return parentVisible && CharacterState.IsJobType(CharacterState.GetCharacterJob(), this.ShowForJobTypes, this.CustomJobList);
        }

        public void DrawConfig(IConfigurable parent, Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##VisibilityConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.Checkbox("Always Hide", ref this.AlwaysHide);
                ImGui.Checkbox("Hide In PvP", ref this.HideInPvP);
                ImGui.Checkbox("Hide Outside PvP", ref this.HideOutsidePvP);
                ImGui.Checkbox("Hide In Combat", ref this.HideInCombat);
                ImGui.Checkbox("Hide Outside Combat", ref this.HideOutsideCombat);
                if (this.HideOutsideCombat) {
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Checkbox("Show When Weapon Is Drawn", ref this.ShowWhenWeaponDrawn);
                    DrawHelpers.DrawNestIndicator(1);
                    ImGui.Checkbox("Show In Duty", ref this.ShowInDuty);
                }
                ImGui.Checkbox("Hide Outside Duty", ref this.HideOutsideDuty);
                ImGui.Checkbox("Hide While Performing", ref this.HideWhilePerforming);
                ImGui.Checkbox("Hide While Weapon Sheathed", ref this.HideWhenSheathed);
                
                DrawHelpers.DrawSpacing();
                ImGui.Checkbox("Hide In Golden Saucer", ref this.HideInGoldenSaucer);
                ImGui.Checkbox("Hide Outside Eureka", ref this.HideOutsideEureka);

                DrawHelpers.DrawSpacing();
                ImGui.Checkbox("Hide if Level", ref this.HideIfLevel);
                if (this.HideIfLevel)
                {
                    ImGui.SameLine();
                    string[] options = TriggerOptions.OperatorOptions;
                    ImGui.PushItemWidth(55);
                    ImGui.Combo("##HideIfLevelOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.HideIfLevelOp), options, options.Length);
                    ImGui.PopItemWidth();

                    ImGui.SameLine();
                    ImGui.PushItemWidth(45);
                    ImGui.InputInt(string.Empty, ref this.HideIfLevelValue, 0, 0);
                    ImGui.PopItemWidth();
                }
                
                DrawHelpers.DrawSpacing();
                string[] jobTypeOptions = Enum.GetNames(typeof(JobType));
                ImGui.Combo("Show for Jobs", ref Unsafe.As<JobType, int>(ref this.ShowForJobTypes), jobTypeOptions, jobTypeOptions.Length);

                if (this.ShowForJobTypes == JobType.Custom)
                {
                    if (string.IsNullOrEmpty(_customJobInput))
                    {
                        _customJobInput = this.CustomJobString.ToUpper();
                    }

                    if (ImGui.InputTextWithHint("Custom Job List", "Comma Separated List (ex: WAR, SAM, BLM)", ref _customJobInput, 100, ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        IEnumerable<string> jobStrings = _customJobInput.Split(',').Select(j => j.Trim());
                        List<Job> jobList = new List<Job>();
                        foreach (string j in jobStrings)
                        {
                            if (Enum.TryParse(j, true, out Job parsed))
                            {
                                jobList.Add(parsed);
                            }
                            else
                            {
                                jobList.Clear();
                                _customJobInput = string.Empty;
                                break;
                            }
                        }

                        _customJobInput = _customJobInput.ToUpper();
                        this.CustomJobString = _customJobInput;
                        this.CustomJobList = jobList;
                    }
                }

                if (parent is XIVAurasConfig)
                {
                    DrawHelpers.DrawSpacing();
                    ImGui.Checkbox("Enable Window Clipping", ref this.Clip);
                }
                
                ImGui.EndChild();
            }
        }
    }
}