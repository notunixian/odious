using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using ReModCE.Loader;
using UnityEngine;
using UnityEngine.Rendering;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using ReMod.Core;

namespace ReModCE.Config
{
    internal class Configuration : ModComponent
    {
        private static AvatarProtectionConfig antiCrashConfig;

        private static bool antiCrashConfigSaving = false;

        private static bool antiCrashConfigSavePending = false;

        internal static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CustomContractResolver("normalized")
        };

        public override void OnUpdate()
        {
            if (!antiCrashConfigSaving && antiCrashConfigSavePending)
            {
                SaveAvatarProtectionsConfig();
            }
        }

        // make a little universal thing incase i want to use this method of configs again
        internal static void LoadAllConfigs()
        {
            LoadAvatarProtectionsConfig();
        }

        internal static void LoadAvatarProtectionsConfig()
        {
            if (!File.Exists(AvatarProtectionConfig.ConfigLocation))
            {
                CreateAvatarProtectionsConfig();
                return;
            }

            try
            {
                antiCrashConfig = JsonConvert.DeserializeObject<AvatarProtectionConfig>(File.ReadAllText(AvatarProtectionConfig.ConfigLocation));
            }
            catch (Exception)
            {
                ReLogger.Error("Corrupt AvatarProtection config! Settings will be their default values.");
            }
		}

        internal static void CreateAvatarProtectionsConfig()
        {
            antiCrashConfig = new AvatarProtectionConfig();
            SaveAvatarProtectionsConfig();
        }

        internal static void SaveAvatarProtectionsConfig(bool forceSave = false)
        {
            if (antiCrashConfigSaving && !forceSave)
            {
                antiCrashConfigSavePending = true;
                return;
            }
            antiCrashConfigSavePending = false;
            antiCrashConfigSaving = true;

            try
            {
                File.WriteAllText(AvatarProtectionConfig.ConfigLocation, JsonConvert.SerializeObject(antiCrashConfig, Formatting.Indented, jsonSerializerSettings));
            }
            catch (Exception e2)
            {
                ReLogger.Error("SaveAntiCrashLimitsConfig - Save", e2);
            }
            antiCrashConfigSaving = false;
        }

        internal static AvatarProtectionConfig GetAvatarProtectionsConfig()
        {
            if (antiCrashConfig == null)
            {
                CreateAvatarProtectionsConfig();
            }
            return antiCrashConfig;
        }
    }

    internal class CustomContractResolver : DefaultContractResolver
    {
        private readonly string propertyNameToExclude;

        internal CustomContractResolver(string propertyNameToExclude)
        {
            this.propertyNameToExclude = propertyNameToExclude;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> source = base.CreateProperties(type, memberSerialization);
            return source.Where((JsonProperty p) => string.Compare(p.PropertyName, propertyNameToExclude, StringComparison.OrdinalIgnoreCase) != 0).ToList();
        }
    }
}
