using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.Text.RegularExpressions;
using MelonLoader;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Loader;
using UnityEngine.UI;
using ReModCE.Config;
using ReModCE.Core;
using ReModCE.SDK;
using VRC.Core;
using UnityEngine;

namespace ReModCE.Components
{
    // jesus christ
    internal class AntiAvatarCrashComponent : ModComponent
    {
        public ConfigValue<bool> AntiShaderCrashEnabled;
        public ConfigValue<bool> AntiAudioCrashEnabled;
        public ConfigValue<bool> AntiMeshCrashEnabled;
        public ConfigValue<bool> AntiMaterialCrashEnabled;
        public ConfigValue<bool> AntiFinalIKCrashEnabled;
        public ConfigValue<bool> AntiClothCrashEnabled;
        public ConfigValue<bool> AntiParticleSystemCrashEnabled;
        public ConfigValue<bool> AntiDynamicBoneCrashEnabled;
        public ConfigValue<bool> AntiLightSourceCrashEnabled;
        public ConfigValue<bool> AntiPhysicsCrashEnabled;
        public ConfigValue<bool> AntiBlendShapeCrashEnabled;
        public ConfigValue<bool> AntiAvatarAudioMixerCrashEnabled;
        public ConfigValue<bool> AntiConstraintsCrashEnabled;

        public static ReMenuToggle AntiShaderCrashToggled;
        public static ReMenuToggle AntiAudioCrashToggled;
        public static ReMenuToggle AntiMeshCrashToggled;
        public static ReMenuToggle AntiMaterialCrashToggled;
        public static ReMenuToggle AntiFinalIKCrashToggled;
        public static ReMenuToggle AntiClothCrashToggled;
        public static ReMenuToggle AntiParticleSystemCrashToggled;
        public static ReMenuToggle AntiDynamicBoneCrashToggled;
        public static ReMenuToggle AntiLightSourceCrashToggled;
        public static ReMenuToggle AntiPhysicsCrashToggled;
        public static ReMenuToggle AntiBlendShapeCrashToggled;
        public static ReMenuToggle AntiAvatarAudioMixerCrashToggled;
        public static ReMenuToggle AntiConstraintsCrashToggled;
        public static ReMenuButton WhitelistByID;
        public static ReMenuButton BlacklistByID;


        public AntiAvatarCrashComponent()
        {
            AntiShaderCrashEnabled = new ConfigValue<bool>(nameof(AntiShaderCrashEnabled), false);
            AntiShaderCrashEnabled.OnValueChanged += () => AntiShaderCrashToggled.Toggle(AntiShaderCrashEnabled);

            AntiAudioCrashEnabled = new ConfigValue<bool>(nameof(AntiAudioCrashEnabled), false);
            AntiAudioCrashEnabled.OnValueChanged += () => AntiAudioCrashToggled.Toggle(AntiAudioCrashEnabled);

            AntiMeshCrashEnabled = new ConfigValue<bool>(nameof(AntiMeshCrashEnabled), false);
            AntiMeshCrashEnabled.OnValueChanged += () => AntiMeshCrashToggled.Toggle(AntiMeshCrashEnabled);

            AntiMaterialCrashEnabled = new ConfigValue<bool>(nameof(AntiMaterialCrashEnabled), false);
            AntiMaterialCrashEnabled.OnValueChanged += () => AntiMaterialCrashToggled.Toggle(AntiMaterialCrashEnabled);

            AntiFinalIKCrashEnabled = new ConfigValue<bool>(nameof(AntiFinalIKCrashEnabled), false);
            AntiFinalIKCrashEnabled.OnValueChanged += () => AntiFinalIKCrashToggled.Toggle(AntiFinalIKCrashEnabled);

            AntiClothCrashEnabled = new ConfigValue<bool>(nameof(AntiClothCrashEnabled), false);
            AntiClothCrashEnabled.OnValueChanged += () => AntiClothCrashToggled.Toggle(AntiClothCrashEnabled);

            AntiParticleSystemCrashEnabled = new ConfigValue<bool>(nameof(AntiParticleSystemCrashEnabled), false);
            AntiParticleSystemCrashEnabled.OnValueChanged += () => AntiParticleSystemCrashToggled.Toggle(AntiParticleSystemCrashEnabled);

            AntiDynamicBoneCrashEnabled = new ConfigValue<bool>(nameof(AntiDynamicBoneCrashEnabled), false);
            AntiDynamicBoneCrashEnabled.OnValueChanged += () => AntiDynamicBoneCrashToggled.Toggle(AntiDynamicBoneCrashEnabled);

            AntiLightSourceCrashEnabled = new ConfigValue<bool>(nameof(AntiLightSourceCrashEnabled), false);
            AntiLightSourceCrashEnabled.OnValueChanged += () => AntiLightSourceCrashToggled.Toggle(AntiLightSourceCrashEnabled);

            AntiPhysicsCrashEnabled = new ConfigValue<bool>(nameof(AntiPhysicsCrashEnabled), false);
            AntiPhysicsCrashEnabled.OnValueChanged += () => AntiPhysicsCrashToggled.Toggle(AntiPhysicsCrashEnabled);

            AntiBlendShapeCrashEnabled = new ConfigValue<bool>(nameof(AntiBlendShapeCrashEnabled), false);
            AntiBlendShapeCrashEnabled.OnValueChanged += () => AntiBlendShapeCrashToggled.Toggle(AntiBlendShapeCrashEnabled);

            AntiAvatarAudioMixerCrashEnabled = new ConfigValue<bool>(nameof(AntiAvatarAudioMixerCrashEnabled), true);
            AntiAvatarAudioMixerCrashEnabled.OnValueChanged += () => AntiAvatarAudioMixerCrashToggled.Toggle(AntiAvatarAudioMixerCrashEnabled);

            AntiConstraintsCrashEnabled = new ConfigValue<bool>(nameof(AntiConstraintsCrashEnabled), false);
            AntiConstraintsCrashEnabled.OnValueChanged += () => AntiConstraintsCrashToggled.Toggle(AntiConstraintsCrashEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var menu = uiManager.MainMenu.GetCategoryPage("Safety").GetCategory("Avatars");

            var mainmenu = menu.AddMenuPage("Avatar Protections", "Settings related to Avatar Protections \n <color=red><b>WARNING:</b></color> A lot of these features are experimental, as this is my first time trying avatar anti-crash.", ResourceManager.GetSprite("remodce.arms-up"));

            AntiShaderCrashToggled = mainmenu.AddToggle("Shader Crash", "Prevents malicious shaders from crashing you.",
                b => { AntiShaderCrashEnabled.SetValue(b); ToggleShader(b); }, AntiShaderCrashEnabled);
            AntiAudioCrashToggled = mainmenu.AddToggle("Audio Crash", "Prevents avatars with corrupted/lots of audio sources from crashing you.",
                b => { AntiAudioCrashEnabled.SetValue(b); ToggleAudio(b); }, AntiAudioCrashEnabled);
            AntiMeshCrashToggled = mainmenu.AddToggle("Mesh Crash", "Prevents avatars with corrupted/lots of meshes from crashing you.",
                b => { AntiMeshCrashEnabled.SetValue(b); ToggleMesh(b); }, AntiMeshCrashEnabled);
            AntiMaterialCrashToggled = mainmenu.AddToggle("Material Crash", "Prevents avatars with corrupted/lots of materials from crashing you.",
                b => { AntiMaterialCrashEnabled.SetValue(b); ToggleMaterial(b); }, AntiMaterialCrashEnabled);
            AntiFinalIKCrashToggled = mainmenu.AddToggle("IK Crash", "Prevents avatars that abuse IK exploits to crash you.",
                b => { AntiFinalIKCrashEnabled.SetValue(b); ToggleIK(b); }, AntiFinalIKCrashEnabled);
            AntiClothCrashToggled = mainmenu.AddToggle("Cloth Crash", "Prevents malicious cloths from crashing you.",
                b => { AntiClothCrashEnabled.SetValue(b); ToggleCloth(b); }, AntiClothCrashEnabled);
            AntiParticleSystemCrashToggled = mainmenu.AddToggle("ParticleSystem Crash", "Prevents malicious particle systems from crashing you.",
                b => { AntiParticleSystemCrashEnabled.SetValue(b); ToggleParticle(b); }, AntiParticleSystemCrashEnabled);
            AntiDynamicBoneCrashToggled = mainmenu.AddToggle("DynBones Crash", "Prevents malicious dynamic bones from crashing you.",
                b => { AntiDynamicBoneCrashEnabled.SetValue(b); ToggleDynBone(b); }, AntiDynamicBoneCrashEnabled);
            AntiBlendShapeCrashToggled = mainmenu.AddToggle("Blendshape Crash", "Prevents a blendshape crash in malicious avatars.",
                b => { AntiBlendShapeCrashEnabled.SetValue(b); ToggleBlend(b); }, AntiBlendShapeCrashEnabled);
            AntiLightSourceCrashToggled = mainmenu.AddToggle("Lightsource Crash", "Prevents bad/too many lightsources from crashing you.",
                b => { AntiLightSourceCrashEnabled.SetValue(b); ToggleLight(b); }, AntiLightSourceCrashEnabled);
            AntiPhysicsCrashToggled = mainmenu.AddToggle("Physics Crash", "Prevents crashes in all physics related components.",
                b => { AntiPhysicsCrashEnabled.SetValue(b); TogglePhysic(b); }, AntiPhysicsCrashEnabled);
            AntiConstraintsCrashToggled = mainmenu.AddToggle("Constraints Crash", "Removes/prevents malicious constraints from crashing you.",
                b => { AntiConstraintsCrashEnabled.SetValue(b); ToggleMixer(b); }, AntiConstraintsCrashEnabled);

            WhitelistByID = menu.AddButton("Whitelist By ID", "Whitelist an avatar by the Avatar ID", () =>
            {
                PopupTextInput(WhitelistByID, false);
            });

            BlacklistByID = menu.AddButton("Blacklist By ID", "Blacklists an avatar by the Avatar ID", () =>
            {
                PopupTextInput(BlacklistByID, true);
            });
        }

        private void ToggleShader(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiShaderCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiShaderCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void ToggleAudio(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiAudioCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiAudioCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void ToggleMesh(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiMeshCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiMeshCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void ToggleMaterial(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiMaterialCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiMaterialCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void ToggleIK(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiFinalIKCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiFinalIKCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void ToggleCloth(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiClothCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiClothCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void ToggleParticle(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiParticleSystemCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiParticleSystemCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void ToggleDynBone(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiDynamicBoneCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiDynamicBoneCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void ToggleBlend(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiBlendShapeCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiBlendShapeCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void ToggleLight(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiLightSourceCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiLightSourceCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void TogglePhysic(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiPhysicsCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiPhysicsCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void ToggleMixer(bool enabled)
        {
            if (enabled)
            {
                Configuration.GetAvatarProtectionsConfig().AntiConstraintsCrash = true;
                Configuration.SaveAvatarProtectionsConfig();
            }
            else
            {
                Configuration.GetAvatarProtectionsConfig().AntiConstraintsCrash = false;
                Configuration.SaveAvatarProtectionsConfig();
            }
        }

        private void PopupTextInput(ReMenuButton button, bool blacklisting)
        {
            VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Input Avatar ID",
                $"", InputField.InputType.Standard, false, "Submit",
                (s, k, t) =>
                {

                    if (string.IsNullOrEmpty(s))
                        return;

                    if (!s.Contains("avtr_"))
                        return;

                    if (blacklisting)
                    {
                        Configuration.GetAvatarProtectionsConfig().BlacklistedAvatars.Add(s, true);
                        Configuration.SaveAvatarProtectionsConfig();
                    }
                    else
                    {
                        WhitelistAvatarByID(s, k, t);
                    }
                }, null);
        }

        private void WhitelistAvatarByID(string avatarId, Il2CppSystem.Collections.Generic.List<KeyCode> pressedKeys, Text text)
        {
            ApiAvatar apiAvatar = new ApiAvatar();
            apiAvatar.id = avatarId.Trim();
            apiAvatar.Get((Action<ApiContainer>)delegate (ApiContainer x)
            {
                AntiCrashUtils.ProcessAvatarWhitelist(x.Model.Cast<ApiAvatar>());
            }, (Action<ApiContainer>)delegate (ApiContainer x)
            {
                ReLogger.Msg("Whitelist failed with error: " + x.Error);
                GeneralWrapper.AlertPopup("Whitelist", "Whitelist failed with error: " + x.Error);
            });
        }
    }
}
