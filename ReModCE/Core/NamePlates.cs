using ReMod.Core;
using ReModCE.EvilEyeSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ReModCE.Core
{
    class NamePlates : MonoBehaviour
    {
        public VRC.Player player;
        private byte frames;
        private byte ping;
        private int noUpdateCount = 0;
        private TextMeshProUGUI statsText;
        private ImageThreeSlice background;
        private string UserID = "";
        private Transform stats;

        public NamePlates(IntPtr ptr) : base(ptr)
        {
        }

        void Start()
        {
            stats = Instantiate<Transform>(base.gameObject.transform.Find("Contents/Quick Stats"), base.gameObject.transform.Find("Contents"));
            stats.transform.localScale = new Vector3(1f, 1f, 2f);
            stats.parent = base.gameObject.transform.Find("Contents");
            stats.gameObject.SetActive(true);
            statsText = stats.Find("Trust Text").GetComponent<TextMeshProUGUI>();
            statsText.color = Color.white;
            stats.Find("Trust Icon").gameObject.SetActive(false);
            stats.Find("Performance Icon").gameObject.SetActive(false);
            stats.Find("Performance Text").gameObject.SetActive(false);
            stats.Find("Friend Anchor Stats").gameObject.SetActive(false);

            frames = player._playerNet.field_Private_Byte_0;
            ping = player._playerNet.field_Private_Byte_1;
            UserID = player.GetAPIUser().id;
        }

        void Update()
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
            }
            else
            {
                stats.localPosition = new Vector3(0f, 42f, 0f);
            }

            frames = player._playerNet.field_Private_Byte_0;
            ping = player._playerNet.field_Private_Byte_1;
            string text = "<color=green>Stable</color>";
            string customrank = CustomRank(UserID);
            if (noUpdateCount > 200)
                text = "<color=yellow>Lagging</color>";
            if (noUpdateCount > 500)
                text = "<color=red>Crashed</color>";
            statsText.text = $"{customrank} [{player.GetPlatform()}] |" + $"{(player.GetIsMaster() ? " | [<color=#0352ff>HOST</color>] |" : "")}" + $" [{text}] |" + $" [FPS: {player.GetFramesColord()}] |" + $" [Ping: {player.GetPingColord()}] " + $" {(player.ClientDetect() ? "| [<color=red>ClientUser</color>]" : "")}";
        }

        string CustomRank(string id)
        {
            string rank;

            // hi requi! you care about me this much?
            // note to self: this will get really long but it isn't as bad as area51 so ¯\_(ツ)_/¯
            if (id == "usr_f0b2d38d-6f62-4d0e-9820-e0e741b574d4" || id == "usr_5232c391-d337-42b7-89dc-df2f1947c342")
            {
                rank = "[<color=#8F9CE6>Odious Staff</color>] |";
            }
            else
            {
                rank = "";
            }
            return rank;
        }
    }
}
