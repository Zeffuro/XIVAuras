using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.JobGauge.Types;
using ImGuiNET;
using XIVAuras.Helpers;
using CharacterStruct = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

namespace XIVAuras.Config
{
    public class ResourceTrigger : TriggerOptions
    {
        [JsonIgnore] private static readonly string[] _sourceOptions = Enum.GetNames<TriggerSource>();
        [JsonIgnore] private static readonly string[] _umbralIceOptions = new[] { "In Umbral Ice", "Not in Umbral Ice" };
        [JsonIgnore] private static readonly string[] _astralFireOptions = new[] { "In Astral Fire", "Not in Astral Fire" };
        [JsonIgnore] private string _elementTimeValueInput = string.Empty;
        [JsonIgnore] private string _umbralHeartsValueInput = string.Empty;
        [JsonIgnore] private string _repertoireValueInput = string.Empty;

        public TriggerSource TriggerSource = TriggerSource.Player;

        public override TriggerType Type => TriggerType.Resource;
        public override TriggerSource Source => this.TriggerSource;
      
        public bool ElementTime = false;
        public TriggerDataOp ElementTimeOp = TriggerDataOp.GreaterThan;
        public float ElementTimeValue;
        public bool InUmbralIce;
        public int UmbralIceValue;
        public bool InAstralFire;
        public int AstralFireValue;
        public bool UmbralHearts;
        public TriggerDataOp UmbralHeartsOp = TriggerDataOp.GreaterThan;
        public float UmbralHeartsValue;
        public bool Repertoire;
        public TriggerDataOp RepertoireOp = TriggerDataOp.GreaterThan;
        public float RepertoireValue;

        public override bool IsTriggered(bool preview, out DataSource data)
        {
            data = new DataSource();
            BLMGauge blmGauge = Plugin.JobGauges.Get<BLMGauge>();
            BRDGauge brdGauge = Plugin.JobGauges.Get<BRDGauge>();
            
            if (preview)
            {
                return true;
            }

            GameObject? actor = Singletons.Get<ClientState>().LocalPlayer;
            
            if (actor is not null)
            {
                data.Name = actor.Name.ToString();
            }

            if (actor is Character chara)
            {
                data.ElementTime = blmGauge.ElementTimeRemaining / 1000f;
                data.MaxElementTime = blmGauge.ElementTimeRemaining;
                data.InUmbralIce = this.TriggerSource == TriggerSource.Player && blmGauge.InUmbralIce;
                data.InAstralFire = this.TriggerSource == TriggerSource.Player && blmGauge.InAstralFire;
                data.UmbralHearts = blmGauge.UmbralHearts;
                data.Repertoire = brdGauge.Repertoire;
            }

            return preview ||
                (!this.ElementTime || Utils.GetResult(data.ElementTime, this.ElementTimeOp, this.ElementTimeValue)) &&
                (!this.InUmbralIce || (this.UmbralIceValue == 0 ? data.InUmbralIce : !data.InUmbralIce)) &&
                (!this.InAstralFire || (this.AstralFireValue == 0 ? data.InAstralFire : !data.InAstralFire)) &&
                (!this.UmbralHearts || Utils.GetResult(data.UmbralHearts, this.UmbralHeartsOp, this.UmbralHeartsValue)) &&
                (!this.Repertoire || Utils.GetResult(data.Repertoire, this.RepertoireOp, this.RepertoireValue));
        }

        public override void DrawTriggerOptions(Vector2 size, float padX, float padY)
        {
            DrawHelpers.DrawSpacing(1);

            ImGui.Text("Trigger Conditions");
            string[] operatorOptions = TriggerOptions.OperatorOptions;
            float optionsWidth = 100 + padX;
            float opComboWidth = 55;
            float valueInputWidth = 45;
            float padWidth = 0;

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("Element Time", ref this.ElementTime);
            if (this.ElementTime)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##ElementTimeOpCombo", ref Unsafe.As<TriggerDataOp, int>(ref this.ElementTimeOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_elementTimeValueInput))
                {
                    _elementTimeValueInput = this.ElementTimeValue.ToString();
                }

                ImGui.PushItemWidth(valueInputWidth);
                if (ImGui.InputText("##ElementTimeValue", ref _elementTimeValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (float.TryParse(_elementTimeValueInput, out float value))
                    {
                        this.ElementTimeValue = value;
                    }

                    _elementTimeValueInput = this.ElementTimeValue.ToString();
                }
                
                ImGui.PopItemWidth();
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("Umbral Hearts", ref this.UmbralHearts);
            if (this.UmbralHearts)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##UmbralHeartsOp", ref Unsafe.As<TriggerDataOp, int>(ref this.UmbralHeartsOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_umbralHeartsValueInput))
                {
                    _umbralHeartsValueInput = this.UmbralHeartsValue.ToString();
                }

                ImGui.PushItemWidth(valueInputWidth);
                if (ImGui.InputText("##ElementTimeValue", ref _umbralHeartsValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (float.TryParse(_umbralHeartsValueInput, out float value))
                    {
                        this.UmbralHeartsValue = value;
                    }

                    _umbralHeartsValueInput = this.UmbralHeartsValue.ToString();
                }
                
                ImGui.PopItemWidth();
            }

            if (this.TriggerSource == TriggerSource.Player)
            {
                DrawHelpers.DrawNestIndicator(1);
                ImGui.Checkbox("In Umbral Ice", ref this.InUmbralIce);
                if (this.InUmbralIce)
                {
                    ImGui.SameLine();
                    padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                    ImGui.PushItemWidth(optionsWidth);
                    ImGui.Combo("##UmbralIce", ref this.UmbralIceValue, _umbralIceOptions, _umbralIceOptions.Length);
                    ImGui.PopItemWidth();
                }

                DrawHelpers.DrawNestIndicator(1);
                ImGui.Checkbox("In Astral Fire", ref this.InAstralFire);
                if (this.InAstralFire)
                {
                    ImGui.SameLine();
                    padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                    ImGui.PushItemWidth(optionsWidth);
                    ImGui.Combo("##AstralFire", ref this.AstralFireValue, _astralFireOptions, _astralFireOptions.Length);
                }
            }

            DrawHelpers.DrawNestIndicator(1);
            ImGui.Checkbox("Repertoire", ref this.Repertoire);
            if (this.Repertoire)
            {
                ImGui.SameLine();
                padWidth = ImGui.CalcItemWidth() - ImGui.GetCursorPosX() - optionsWidth + padX;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + padWidth);
                ImGui.PushItemWidth(opComboWidth);
                ImGui.Combo("##RepertoireOp", ref Unsafe.As<TriggerDataOp, int>(ref this.RepertoireOp), operatorOptions, operatorOptions.Length);
                ImGui.PopItemWidth();
                ImGui.SameLine();

                if (string.IsNullOrEmpty(_repertoireValueInput))
                {
                    _repertoireValueInput = this.RepertoireValue.ToString();
                }

                ImGui.PushItemWidth(valueInputWidth);
                if (ImGui.InputText("##RepertoireValue", ref _repertoireValueInput, 10, ImGuiInputTextFlags.CharsDecimal))
                {
                    if (float.TryParse(_repertoireValueInput, out float value))
                    {
                        this.RepertoireValue = value;
                    }

                    _repertoireValueInput = this.RepertoireValue.ToString();
                }
                
                ImGui.PopItemWidth();
            }
        }
    }
}