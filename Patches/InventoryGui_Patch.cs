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
			var traverse = Traverse.Create(__instance);
			var craftUpgradeItem = traverse.Field("m_craftUpgradeItem").GetValue<ItemDrop.ItemData>();
			var craftRecipe = traverse.Field("m_craftRecipe").GetValue<Recipe>();
			var qualityLevel = (craftUpgradeItem == null) ? 1 : (craftUpgradeItem.m_quality + 1);
			if (player.HaveRequirements(craftRecipe, discover: false, qualityLevel)) {
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
			var traverse = Traverse.Create(__instance);
			var craftRecipe = traverse.Field("m_craftRecipe").GetValue<Recipe>();
			CraftAll.Debug("recipe? " + craftRecipe);
			if (craftRecipe == null) CraftAll.StopCraftingAll(false);
		}
	}
}
