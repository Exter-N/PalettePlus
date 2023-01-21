﻿using System;
using System.Linq;
using System.Numerics;

using ImGuiNET;

using Dalamud.Interface;
using Dalamud.Interface.Components;

using PalettePlus.Palettes;
using PalettePlus.Interface.Dialog;

namespace PalettePlus.Interface.Components {
	// This shares a lot of common code with ActorList.
	// Should try to reduce this down to one common components.

	internal class PaletteList {
		internal string SearchString = "";

		internal Palette? Selected = null;

		// Draw

		internal bool Draw() {
			var result = false;

			var width = Math.Min(ImGui.GetWindowSize().X * 1 / 3, 400);

			ImGui.BeginGroup();

			ImGui.SetNextItemWidth(width);
			ImGui.InputTextWithHint("##SaveSearch", "Search...", ref SearchString, 32);

			var avail = ImGui.GetContentRegionAvail();
			var buttonY = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2 + 4;

			if (ImGui.BeginChildFrame(1, new Vector2(width, avail.Y - buttonY))) {
				result = DrawList();
				ImGui.EndChildFrame();

				if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus)) {
					var name = "";
					string? err = null;

					var popup = (PopupMenu)PluginGui.GetWindow<PopupMenu>();
					popup.Open(() => {
						ImGui.InputTextWithHint("##NewSaveName", "Palette Name", ref name, 100);
						if (ImGui.IsKeyDown(ImGuiKey.Enter) && name.Length > 0) {
							var exists = PalettePlus.Config.SavedPalettes.Any(p => p.Name == name);
							err = exists ? "a palette with this name already exists." : null;

							if (err == null) {
								popup.Close();

								var palette = new Palette(name);
								PalettePlus.Config.SavedPalettes.Add(palette);
							}
						}

						if (err != null) {
							ImGui.PushStyleColor(ImGuiCol.Text, 0xff3030ff);
							ImGui.Text($"Could not save: {err}");
							ImGui.PopStyleColor();
						}
					});
				}

				ImGui.SameLine();

				ImGui.BeginDisabled(Selected == null);
				if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash)) {
					PalettePlus.Config.SavedPalettes.Remove(Selected);
					Selected = null;
				}
				ImGui.EndDisabled();
			}
			ImGui.EndGroup();

			return result;
		}

		internal bool DrawList() {
			var palettes = PalettePlus.Config.SavedPalettes;
			if (SearchString.Length > 0) {
				var searchString = SearchString.ToLower();
				palettes = palettes.FindAll(p => p.Name.ToLower().Contains(searchString));
			}

			var result = false;
			foreach (var save in palettes) {
				if (ImGui.Selectable(save.Name, save == Selected)) {
					result = true;
					Selected = save;
				}
			}
			return result;
		}
	}
}