using ReMod.Core;
using ReModCE.EvilEyeSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

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

        public NamePlates(IntPtr ptr) : base(ptr)
        {
        }

        void Start()
        {
            Transform stats = UnityEngine.Object.Instantiate<Transform>(gameObject.transform.Find("Contents/Quick Stats"), this.gameObject.transform.Find("Contents"));
            stats.parent = gameObject.transform.Find("Contents");
            stats.gameObject.SetActive(true);
            statsText = stats.Find("Trust Text").GetComponent<TextMeshProUGUI>();
            statsText.color = Color.white;
            stats.Find("Trust Icon").gameObject.SetActive(false);
            stats.Find("Performance Icon").gameObject.SetActive(false);
            stats.Find("Performance Text").gameObject.SetActive(false);
            stats.Find("Friend Anchor Stats").gameObject.SetActive(false);

            background = this.gameObject.transform.Find("Contents/Main/Background").GetComponent<ImageThreeSlice>();

            background._sprite = VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.Find("Player Nameplate/Canvas/Nameplate/Contents/Main/Glow").GetComponent<ImageThreeSlice>()._sprite;
            background.color = Color.black;

            frames = player._playerNet.field_Private_Byte_0;
            ping = player._playerNet.field_Private_Byte_1;
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
            frames = player._playerNet.field_Private_Byte_0;
            ping = player._playerNet.field_Private_Byte_1;
            string text = "<color=green>Stable</color>";
            if (noUpdateCount > 30)
                text = "<color=yellow>Lagging</color>";
            if (noUpdateCount > 150)
                text = "<color=red>Crashed</color>";
            statsText.text = $"[{player.GetPlatform()}] |" + $"{(player.GetIsMaster() ? " | [<color=#0352ff>HOST</color>] |" : "")}" + $" [{text}] |" + $" [FPS: {player.GetFramesColord()}] |" + $" [Ping: {player.GetPingColord()}] " + $" {(player.ClientDetect() ? " | [<color=red>ClientUser</color>]" : "")}";
        }
    }
}
