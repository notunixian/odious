using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace ReModCE.Core
{
    internal class SettingsHandler
    {
        private static MelonPreferences_Entry<bool> _LogAvi;
        private static MelonPreferences_Entry<bool> _PhotonProtection;
        private static MelonPreferences_Entry<bool> _AvatarProtection;
        private static MelonPreferences_Entry<bool> _WorldSpoof;
        private static MelonPreferences_Entry<string> _WorldIdToSpoof;
        private static MelonPreferences_Entry<string> _InstanceIdToSpoof;
        private static MelonPreferences_Entry<bool> _WorldSpoofWarn;
        private static MelonPreferences_Entry<bool> _WorldSpoofEnabled;
        private static MelonPreferences_Entry<bool> _HWIDSpoof;

        public static void Register()
        {
            var category = MelonPreferences.GetCategory("ReModCE");

            // general toggles, avatar logging, photon antis, avatar antis, etc
            _LogAvi = category.CreateEntry("LogAvi", false, "Excessive Avatar Logging",
                "When enabled, will excessively log avatars. Useful for identifying crashers or yoinking avatars from people.",
                true);
            _PhotonProtection = category.CreateEntry("PhotonProtection", false, "Photon Event Protections",
                "When enabled, will try to prevent malicious Photon exploits from affecting you. (freezing/crashing you.)",
                true);
            _AvatarProtection = category.CreateEntry("AvatarProtection", false, "Avatar Protections",
                "When enabled, will try to prevent malicious avatars from crashing you.",
                true);
            _WorldSpoof = category.CreateEntry("WorldSpoof", false, "World Spoofing",
                "When enabled, will spoof the world to a selected World ID (by default, VRChat Home World with instance id 1337.) Configurable under WorldIdToSpoof and InstanceIdToSpoof.",
                true);
            _WorldIdToSpoof = category.CreateEntry("WorldIdToSpoof", "wrld_4432ea9b-729c-46e3-8eaf-846aa0a37fdd", "World Spoofing",
                "World ID to spoof, WorldSpoof must be enabled for this to work.",
                true);
            _InstanceIdToSpoof = category.CreateEntry("InstanceIdToSpoof", "1337", "World Spoofing",
                "Instance ID to spoof, WorldSpoof must be enabled for this to work.",
                true);
            _WorldSpoofWarn = category.CreateEntry("WorldSpoofWarn", false, "World Spoofing",
                "Nags you about the world you are spoofing, just incase. WorldSpoof must be enabled for this to work.",
                true);
            _HWIDSpoof = category.CreateEntry("HWIDSpoof", false, "HWID Spoofing",
                "Spoofs your hardware identifier to prevent bans from VRChat.", true);
        }
    }
}
