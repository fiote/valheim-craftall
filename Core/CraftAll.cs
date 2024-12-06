using BepInEx;
using BepInEx.Logging;
using CraftAll.Patches;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * [0.0.4]
 * - updating mod to current game version.
 * 
 * [0.0.3]
 * - fixing epicLoot incompatibility.
 *
 * [0.0.2]
 * - stopping craftall flow on inventory show/hide to prevent it trying to craft when the crafting tab is closed.
 * - improving flow to avoid having the flow stopped after the first craft.
 *
 * [0.0.1]
 * - first version!
 */

namespace CraftAll {
	[BepInPlugin("fiote.mods.craftall", "CraftAll", "0.0.4")]

	public class CraftAll : BaseUnityPlugin {

		static bool debug = false;

		public static GameObject goCraftAll;
		public static Button btnCraftAll;
		public static TMP_Text txtCraftAll;
		public static bool isCraftingAll;

		private void Awake() {
			Debug($"Awake()");
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "fiote.mods.craftall");
		}

		public static void CreateCraftAllButton() {
			Debug($"CreateCraftAllButton()");

			var craftButton = InventoryGui.instance?.m_craftButton?.gameObject;
			if (craftButton == null) {
				Debug("craftButton not found");
				return;
			}

			if (goCraftAll != null) {
				Debug("craftAllButton already exists. destroy it.");
				Destroy(goCraftAll);
			}

			Debug("Instantiate");
			goCraftAll = Instantiate(craftButton);
			Debug("SetParent");
			goCraftAll.transform.SetParent(craftButton.transform.parent, false);
			Debug("SetName");
			goCraftAll.name = "craftAllCraftButton";

			Debug("get position");
			var position = goCraftAll.transform.position;
			position.x += 0f;
			position.y += -60f;
			Debug("set position");
			goCraftAll.transform.position = position;

			Debug("get rect");
			var rect = goCraftAll.GetComponent<RectTransform>();
			var size = rect.sizeDelta;
			size.x += -150;
			size.y += -20;
			rect.sizeDelta = size;

			Debug("get button");
			btnCraftAll = goCraftAll.GetComponentInChildren<Button>();
			btnCraftAll.interactable = true;
			btnCraftAll.onClick.AddListener(OnClickCraftAllButton);

			Debug("get text");
			txtCraftAll = goCraftAll.GetComponentInChildren<TMP_Text>();
			if (txtCraftAll != null) {
				txtCraftAll.text = "Craft All";
				txtCraftAll.autoSizeTextContainer = false;
				txtCraftAll.fontSize = 16;
			}

			Debug("set tooltip");
			goCraftAll.GetComponent<UITooltip>().m_text = "";
		}

		public static void OnClickCraftAllButton() {
			Debug("=====================================================");
			Debug("=====================================================");
			Debug("=====================================================");
			Debug($"OnClickCraftAllButton()");
			Debug($"isCraftingAll={isCraftingAll}");
			if (isCraftingAll) {
				StopCraftingAll(true);
			} else {
				StartCraftingAll();
			}
		}

		public static void StartCraftingAll() {
			Debug($"StartCraftingAll()");
			Debug($"isCraftingAll is now TRUE");
			isCraftingAll = true;
			txtCraftAll.text = "Stop Crafting";
			var gui = InventoryGui.instance;
			var traverse = Traverse.Create(gui);
			traverse.Method("OnCraftPressed").GetValue();
		}

		public static void StopCraftingAll(bool triggerCancel) {
			Debug($"StopCraftingAll({triggerCancel})");
			Debug($"isCraftingAll is now FALSE");
			isCraftingAll = false;
			if (txtCraftAll != null) txtCraftAll.text = "Craft All";
			if (triggerCancel) {
				Debug("forcing OnCraftCancelPressed()");
				InventoryGui.instance.OnCraftCancelPressed();
			}
		}

		public static void TryCraftingMore() {
			Debug($"TryCraftingMore()");
			Debug($"isCraftingAll={isCraftingAll}");
			if (!isCraftingAll) {
				Debug("cant craft more. we're not 'craft-all'ing.");
				return;
			}
			Debug("getting gui");
			var gui = InventoryGui.instance;
			Debug("getting selectedRecipe");
			var selectedRecipe = gui.m_selectedRecipe;
			Debug("getting selectedVariant");
			var selectedVariant = gui.m_selectedRecipe;
			Debug($"m_selectedRecipe={selectedRecipe.Recipe}");
			Debug($"m_selectedVariant={selectedVariant}");
			Debug($"m_craftRecipe.m_craftingStation={selectedRecipe.Recipe?.m_craftingStation}");
			gui.OnCraftPressed();
			Debug("done!");
		}

		#region LOG

		public static void Log(string message) {
			UnityEngine.Debug.Log("[CraftAll] " + message);
		}

		public static void Bar() {
			Log("=============================================================");
		}

		public static void Line() {
			Log("---------------------------");
		}

		public static void Debug(string message) {
			if (debug) Log(message);
		}

		#endregion
	}
}