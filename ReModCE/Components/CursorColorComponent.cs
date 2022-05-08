using MelonLoader;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.Unity;
using ReMod.Core.VRChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE.Components
{
	// i orignally lost the code for this while doing this so i had to decompile it to get it back, sorry for the messy code below
    internal class CursorColorComponent : ModComponent
    {
		public ConfigValue<bool> _CursorColorEnabled;
		private static ReMenuToggle _CursorColorToggled;
		private ConfigValue<Color> CursorColor;
		private ReMenuButton _CursorColorButton;
        private GameObject CursorObject;
		public CursorColorComponent()
		{
			Color color = new Color(0f, 0.848f, 1f, 1f);
			this.CursorColor = new ConfigValue<Color>("CursorColor", color, null, null, false);
			this._CursorColorEnabled = new ConfigValue<bool>("_CursorColorEnabled", false, null, null, false);
			this._CursorColorEnabled.OnValueChanged += delegate ()
			{
				CursorColorComponent._CursorColorToggled.Toggle(this._CursorColorEnabled, true, false);
			};
			CursorObject = GameObject.Find("_Application/CursorManager/MouseArrow/VRCUICursorIcon");
        }
		public override void OnUiManagerInit(UiManager uiManager)
		{
			ReMenuCategory visualMenu = uiManager.MainMenu.GetCategoryPage("Visuals").GetCategory("Cursor");
			CursorColorComponent._CursorColorToggled = visualMenu.AddToggle("Cursor Color", "Allows you to set a VRChat custom cursor color.", delegate (bool b)
			{
				this._CursorColorEnabled.SetValue(b);
				this.PerformCursorColor(b);
			}, this._CursorColorEnabled);
			this._CursorColorButton = visualMenu.AddButton("<color=#" + this.CursorColor.Value.ToHex() + ">Cursor</color> Color", "Set your VRChat cursor color", delegate
			{
				this.PopupColorInput(this._CursorColorButton, this.CursorColor);
			}, ResourceManager.GetSprite("remodce.palette"));
		}

		private void PopupColorInput(ReMenuButton button, ConfigValue<Color> configValue)
		{
			VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Input hex color code",
				$"#{configValue.Value.ToHex()}", InputField.InputType.Standard, false, "Submit",
				(s, k, t) =>
				{
					if (string.IsNullOrEmpty(s))
						return;

					if (!ColorUtility.TryParseHtmlString(s, out var color))
						return;

					configValue.SetValue(color);

					button.Text = $"<color=#{configValue.Value.ToHex()}>Cursor</color> Color";
				}, null);
		}

		public void PerformCursorColor(bool value)
		{
			if (value)
			{
				GameObject gameObject = GameObject.Find("_Application/CursorManager/MouseArrow/VRCUICursorIcon");
				//MelonLogger.Log(gameObject.GetComponent<SpriteRenderer>().color.ToString());
				gameObject.GetComponent<SpriteRenderer>().color = this.CursorColor;
			}
			if (!value)
			{
				GameObject Cursor = GameObject.Find("_Application/CursorManager/MouseArrow/VRCUICursorIcon");
				//MelonLogger.Msg("[PerformColorCursor] cursor being reset!");
				//MelonLogger.Log("[PerformColorCursor] Color before: " + Cursor.GetComponent<SpriteRenderer>().color.ToString());
				Cursor.GetComponent<SpriteRenderer>().color = new Color(0f, 0.848f, 1f, 1f);
				//MelonLogger.Log("[PerformColorCursor] Color after reset: " + Cursor.GetComponent<SpriteRenderer>().color.ToString());
			}
		}

		// do this double check cause someone told me it doesn't set it sometimes at start
        public override void OnUpdate()
        {
            if (!this._CursorColorEnabled)
            {
                return;
            }

            if (CursorObject.GetComponent<SpriteRenderer>().color == this.CursorColor)
            {
                return;
            }
            else
            {
                CursorObject.GetComponent<SpriteRenderer>().color = this.CursorColor;
            }
        }


        public override void OnPreferencesSaved()
		{
			if (!this._CursorColorEnabled)
			{
				return;
			}
			GameObject gameObject = GameObject.Find("_Application/CursorManager/MouseArrow/VRCUICursorIcon");
			//MelonLogger.Msg("[OnPreferencesSaved] Setting color cursor since preferences have changed!");
			gameObject.GetComponent<SpriteRenderer>().color = this.CursorColor;
		}
	}
}
