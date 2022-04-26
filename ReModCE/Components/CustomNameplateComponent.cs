using Newtonsoft.Json;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Core;
using ReModCE.Loader;
using ReModCE.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using ReModCE.EvilEyeSDK;
using TMPro;
using UnityEngine;
using VRC;

namespace ReModCE.Components
{
    public class CustomNameplate : MonoBehaviour
    {
        public VRC.Player player;
        private byte frames;
        private byte ping;
        private int noUpdateCount = 0;
        private TextMeshProUGUI statsText;
        private TextMeshProUGUI customText;
        private Transform stats;
        private Transform customStats;
        public CustomNameplate(IntPtr ptr) : base(ptr)
        {
        }
        void Start()
        {
            if (this.enabled)
            {
                stats = UnityEngine.Object.Instantiate<Transform>(this.gameObject.transform.Find("Contents/Quick Stats"), this.gameObject.transform.Find("Contents"));
                stats.parent = this.gameObject.transform.Find("Contents");
                stats.name = "Nameplate";
                stats.gameObject.SetActive(true);
                statsText = stats.Find("Trust Text").GetComponent<TextMeshProUGUI>();
                statsText.color = Color.white;
                statsText.isOverlay = true;

                stats.Find("Trust Icon").gameObject.SetActive(false);
                stats.Find("Performance Icon").gameObject.SetActive(false);
                stats.Find("Performance Text").gameObject.SetActive(false);
                stats.Find("Friend Anchor Stats").gameObject.SetActive(false);

                NameplateModel custom = IsCustom(player);
                if (custom != null)
                {
                    customStats = UnityEngine.Object.Instantiate<Transform>(this.gameObject.transform.Find("Contents/Quick Stats"), this.gameObject.transform.Find("Contents"));
                    customStats.parent = this.gameObject.transform.Find("Contents");
                    customStats.name = "StaffSetNameplate";
                    customStats.gameObject.SetActive(true);
                    customText = customStats.Find("Trust Text").GetComponent<TextMeshProUGUI>();
                    customText.color = Color.white;
                    customText.isOverlay = true;

                    customStats.Find("Trust Icon").gameObject.SetActive(false);
                    customStats.Find("Performance Icon").gameObject.SetActive(false);
                    customStats.Find("Performance Text").gameObject.SetActive(false);
                    customStats.Find("Friend Anchor Stats").gameObject.SetActive(false);
                }
                frames = player._playerNet.field_Private_Byte_0;
                ping = player._playerNet.field_Private_Byte_1;
            }
        }

        private NameplateModel IsCustom(VRC.Player player)
        {
            try
            {
                return ReModCE.nameplateModels.First(x => x.UserID == player.prop_APIUser_0.id && x.Active);
            }
            catch
            {
                return null;
            }
        }



        void Update()
        {
            if (this.enabled)
            {
                if (frames == player._playerNet.field_Private_Byte_0 && ping == player._playerNet.field_Private_Byte_1)
                {
                    noUpdateCount++;
                }
                else
                {
                    noUpdateCount = 0;
                }

                if (ReModCE.isQuickMenuOpen)
                {
                    stats.localPosition = new Vector3(0f, 62f, 0f);
                    if (customText != null) customStats.localPosition = new Vector3(0f, 100f, 0f);
                }
                else
                {
                    if (customText != null) customStats.localPosition = new Vector3(0f, 80f, 0f);
                    stats.localPosition = new Vector3(0f, 42f, 0f);
                }

                frames = player._playerNet.field_Private_Byte_0;
                ping = player._playerNet.field_Private_Byte_1;
                string text = "<color=green>Stable</color>";
                if (noUpdateCount > 100)
                    text = "<color=yellow>Lagging</color>";
                if (noUpdateCount > 350)
                    text = "<color=red>Crashed</color>";
                statsText.text = $"[{player.GetPlatform()}] |" + $" [{player.GetAvatarStatus()}] |" + $"{(player.GetIsMaster() ? " [<color=#0352ff>HOST</color>] |" : "")}" + $" [{text}] |" + $" [FPS: {player.GetFramesColord()}] |" + $" [Ping: {player.GetPingColord()}] ";
                if (customText != null)
                {
                    NameplateModel custom = IsCustom(player);
                    customText.text = custom.Text;
                }
            }
        }
        public void Dispose()
        {
            statsText.text = null;
            customText.text = null;
            statsText.OnDisable();
            customText.OnDisable();
            this.enabled = false;
        }
    }

    internal sealed class CustomNameplateComponent : ModComponent
    {
        private ConfigValue<bool> CustomNameplateEnabled;
        private ReMenuToggle _CustomNameplateEnabled;

        public CustomNameplateComponent()
        {
            CustomNameplateEnabled = new ConfigValue<bool>(nameof(CustomNameplateEnabled), true);
            CustomNameplateEnabled.OnValueChanged += () => _CustomNameplateEnabled.Toggle(CustomNameplateEnabled);
        }

        public override void OnPlayerJoined(VRC.Player player)
        {
            if (CustomNameplateEnabled)
            {
                CustomNameplate nameplate = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject.AddComponent<CustomNameplate>();
                nameplate.player = player;
            }
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);
            var menu = uiManager.MainMenu.GetCategoryPage("Visuals").GetCategory("Nametags");
            _CustomNameplateEnabled = menu.AddToggle("Custom Nameplates", "Enable/Disable custom nameplates (reload world to fully unload)", ToggleNameplates,
                CustomNameplateEnabled);
        }

        private void ToggleNameplates(bool value)
        {
            CustomNameplateEnabled.SetValue(value);
            try
            {
                if (value)
                {
                    try
                    {
                        foreach (Player player in UnityEngine.Object.FindObjectsOfType<Player>())
                        {

                            CustomNameplate nameplate = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject.AddComponent<CustomNameplate>();
                            nameplate.player = player;

                        }
                    }
                    catch { }
                }
                else
                {
                    foreach (Player player in UnityEngine.Object.FindObjectsOfType<Player>())
                    {
                        CustomNameplate disabler = player.transform.Find("Player Nameplate/Canvas/Nameplate").gameObject.GetComponent<CustomNameplate>();
                        disabler.Dispose();
                    }
                }
            }
            catch { }
        }
    }
}
