﻿using System;
using System.Numerics;
using System.Collections.Generic;

using ImGuiNET;

using Dalamud.Game.ClientState.Objects.Types;

using ColorEdit.Structs;

namespace ColorEdit.Interface.Components {
	internal class ActorList {
		private const int GPoseStartIndex = 201;

		private string SearchString = "";
		private Dictionary<string, ActorContainer> ActorNames = new();

		// Draw

		internal void Draw(GameObject? selected, Action<GameObject?> callback) {
			var width = Math.Min(ImGui.GetWindowSize().X * 1 / 3, 400);

			ImGui.BeginGroup();

			ImGui.SetNextItemWidth(width);
			ImGui.InputTextWithHint("##ActorSearch", "Search...", ref SearchString, 32);

			if (ImGui.BeginChildFrame(1, new Vector2(width, -1))) {
				DrawList(selected, callback);
				ImGui.EndChildFrame();
			}
			ImGui.EndGroup();
		}

		private void DrawList(GameObject? selected, Action<GameObject?> callback) {
			ActorNames.Clear();

			bool isSelectionValid = false;
			for (var i = 0; i < GPoseStartIndex + 200; i++) {
				var actor = Services.ObjectTable[i];
				if (actor == null) continue;

				unsafe {
					// TODO: ClientStructs PR
					if (Actor.GetActor(actor)->ModelId != 0) continue;
					if (Model.GetModel(actor) == null) continue;
				}

				var name = actor.Name.ToString();
				if (name.Length == 0) continue;

				if (selected == actor)
					isSelectionValid = true;

				var cont = new ActorContainer(i, actor);

				var exists = ActorNames.TryGetValue(name, out var _);
				if (exists) {
					if (i >= GPoseStartIndex) {
						if (selected != null && selected.Address == ActorNames[name].GameObject.Address)
							callback(actor);
						ActorNames[name] = cont;
					} else {
						var x = 2;
						while (exists) {
							name = $"{actor.Name} #{x}";
							exists = ActorNames.TryGetValue(name, out var _);
							x++;
						}
						ActorNames.Add(name, cont);
					}
				} else {
					ActorNames.Add(name, cont);
				}
			}

			if (!isSelectionValid)
				callback(null);

			var searchString = SearchString.ToLower();

			foreach (var (name, cont) in ActorNames) {
				var actor = cont.GameObject;

				var label = name;
				if (cont.IsGPoseActor)
					label += " (GPose)";

				if (searchString.Length > 0 && !label.ToLower().Contains(searchString))
					continue;

				if (ImGui.Selectable(label, selected != null && selected.Address == actor.Address))
					callback(actor);
			}
		}

		// ActorContainer

		private class ActorContainer {
			internal int Index;
			internal GameObject GameObject;

			internal ActorContainer(int index, GameObject obj) {
				Index = index;
				GameObject = obj;
			}

			internal bool IsGPoseActor => Index >= GPoseStartIndex && Index <= GPoseStartIndex + 48;
		}
	}
}