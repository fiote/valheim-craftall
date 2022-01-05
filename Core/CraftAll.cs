using BepInEx;
using BepInEx.Logging;
using CraftAll.Patches;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/*
 * [0.0.2]
 * - stopping craftall flow on inventory show/hide to prevent it trying to craft when the crafting tab is closed.
 * - improving flow to avoid having the flow stopped after the first craft.
 *
 * [0.0.1]
 * - first version!
 */

namespace CraftAll {
	[BepInPlugin("fiote.mods.craftall", "CraftAll", "0.0.2")]

	public class CraftAll : BaseUnityPlugin {

		static bool debug = false;

		public static GameObject goCraftAll;
		public static Button btnCraftAll;
		public static Text txtCraftAll;
		public static bool isCraftingAll;

		private void Awake() {
			Debug($"Awake()");
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "fiote.mods.craftall");
		}

		public static void CreateCraftAllButton() {
			Debug($"CreateCraftAllButton()");
			var craftButton = InventoryGui.instance?.m_craftButton?.gameObject;
			if (craftButton == null) return;

			if (goCraftAll != null) Destroy(goCraftAll);

			goCraftAll = Instantiate(craftButton);
			goCraftAll.transform.SetParent(craftButton.transform.parent, false);
			goCraftAll.name = "craftAllCraftButton";

			var position = goCraftAll.transform.position;
			position.x += 0f;
			position.y += -60f;
			goCraftAll.transform.position = position;

			var rect = goCraftAll.GetComponent<RectTransform>();
			var size = rect.sizeDelta;
			size.x += -150;
			size.y += -10;
			rect.sizeDelta = size;

			btnCraftAll = goCraftAll.GetComponentInChildren<Button>();
			btnCraftAll.interactable = true;
			btnCraftAll.onClick.AddListener(OnClickCraftAllButton);

			txtCraftAll = goCraftAll.GetComponentInChildren<Text>();
			txtCraftAll.text = "Craft All";
			txtCraftAll.resizeTextForBestFit = false;
			txtCraftAll.fontSize = 20;

			goCraftAll.GetComponent<UITooltip>().m_text = "";
		}

		public static void OnClickCraftAllButton() {
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
			isCraftingAll = true;
			txtCraftAll.text = "Stop Crafting";
			InventoryGui.instance.OnCraftPressed();
		}

		public static void StopCraftingAll(bool triggerCancel) {
			Debug($"StopCraftingAll({triggerCancel})");
			isCraftingAll = false;
			if (txtCraftAll != null) txtCraftAll.text = "Craft All";
			if (triggerCancel) InventoryGui.instance.OnCraftCancelPressed();
		}

		public static void TryCraftingMore() {
			Debug($"TryCraftingMore()");
			Debug($"isCraftingAll={isCraftingAll}");
			if (!isCraftingAll) return;
			var gui = InventoryGui.instance;
			Debug($"m_selectedRecipe={gui.m_selectedRecipe.Key}");
			Debug($"m_selectedVariant={gui.m_selectedVariant}");
			Debug($"m_craftRecipe.m_craftingStation={gui.m_selectedRecipe.Key?.m_craftingStation}");
			gui.OnCraftPressed();
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