using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CraftAll.Patches {

	public static class InventoryGui_Helper {
		public static bool isVisible = false;
		public static bool justCrafted;
	}


	[HarmonyPatch(typeof(InventoryGui), "Awake")]
	public static class InventoryGui_Awake_Patch {
		public static void Postfix(InventoryGui __instance) {
			CraftAll.Debug("[Postfix] InventoryGui.Awake()");
			CraftAll.CreateCraftAllButton();
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "Show")]
	public static class InventoryGui_Show_Patch {
		public static void Prefix(InventoryGui __instance, Container container) {
			CraftAll.Debug("[Prefix] InventoryGui.Show()");
			InventoryGui_Helper.isVisible = true;
			CraftAll.StopCraftingAll(true);
		}
	}


	[HarmonyPatch(typeof(InventoryGui), "Hide")]
	public static class InventoryGui_Hide_Patch {
		public static void Prefix(InventoryGui __instance) {
			if (!InventoryGui_Helper.isVisible) return;
			CraftAll.Debug("[Prefix] InventoryGui.Hide()");
			InventoryGui_Helper.isVisible = false;
			CraftAll.StopCraftingAll(true);
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnSelectedRecipe")]
	public static class InventoryGui_OnSelectedRecipe_Patch {
		public static void Postfix(InventoryGui __instance) {
			CraftAll.Debug("[Postfix] InventoryGui.OnSelectedRecipe()");
			CraftAll.StopCraftingAll(false);
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
	public static class InventoryGui_DoCrafting_Patch {
		public static void Postfix(InventoryGui __instance, Player player) {
			CraftAll.Debug("[Postfix] InventoryGui.DoCrafting()");
			InventoryGui_Helper.justCrafted = true;

		}
	}

	[HarmonyPatch(typeof(InventoryGui), "UpdateRecipe")]
	public static class InventoryGui_UpdateRecipe_Patch {
		public static void Postfix(InventoryGui __instance, Player player, float dt) {
			if (!InventoryGui_Helper.justCrafted) return;
			InventoryGui_Helper.justCrafted = false;

			CraftAll.Debug("[Postfix] InventoryGui.UpdateRecipe()");
			var qualityLevel = (__instance.m_craftUpgradeItem == null) ? 1 : (__instance.m_craftUpgradeItem.m_quality + 1);
			if (player.HaveRequirements(__instance.m_craftRecipe, discover: false, qualityLevel)) {
				CraftAll.TryCraftingMore();
			} else {
				CraftAll.StopCraftingAll(false);
			}
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnCraftCancelPressed")]
	public static class InventoryGui_OnCraftCancelPressed_Patch {
		public static void Prefix(InventoryGui __instance) {
			CraftAll.Debug("[Prefix] InventoryGui.OnCraftCancelPressed()");
			if (CraftAll.isCraftingAll) {
				System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
				if (t.ToString().Contains("EpicLoot.Crafting.TabController.OnInventoryChanged")) {
					CraftAll.Debug("-> not a real cancelPress. Ignoring...");
					return;
				}
			}
			CraftAll.StopCraftingAll(false);
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnCraftPressed")]
	public static class InventoryGui_OnCraftPressed_Patch {
		public static void Postfix(InventoryGui __instance) {
			CraftAll.Debug("[Postfix] InventoryGui.OnCraftPressed()");
			CraftAll.Debug("recipe? " + __instance.m_craftRecipe);
			if (__instance.m_craftRecipe == null) CraftAll.StopCraftingAll(false);
		}
	}
}