using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReMod.Core;
using ReMod.Core.UI.QuickMenu;
using UnityEngine;
using System.IO;
using MelonLoader;
using UnityEngine.Networking;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Il2CppSystem.Linq;
using ReMod.Core.Managers;
using UnhollowerRuntimeLib;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Type = Il2CppSystem.Type;
using Harmony;
using ReModCE.Loader;
using Object = System.Object;

namespace ReModCE.Components
{
    internal class LoadingMusicComponent : ModComponent
    {
        private ConfigValue<bool> MusicEnabled;
        private static ReMenuToggle _MusicToggled;
        private readonly string url = "https://github.com/imxLucid/ReModX/blob/main/Resources/LoadingMusic.ogg?raw=true";
        internal static readonly string userAgent = "VRC.Core.BestHTTP";
        private AudioClip c;
        private List<AudioClip> MusicList = new List<AudioClip>();
        private AudioSource s1;
        private AudioSource s2;
        private GameObject bg;
        private GameObject bg2;
        public static Sprite OdiousIcon;
        private static bool foundlocal = false;

        public LoadingMusicComponent()
        {
            MusicEnabled = new ConfigValue<bool>(nameof(MusicEnabled), true);
            MusicEnabled.OnValueChanged += () => _MusicToggled.Toggle(MusicEnabled);

            MelonCoroutines.Start(Audio());
        }

        private System.Collections.IEnumerator InitScreen()
        {
            LoadingScreen();
            yield return null;
        }

        public override void OnUpdate()
        {
            //some checks so we don't get null reference exceptions
            //if (GameObject.Find("_Application/CursorManager/MouseArrow/") != null &&
            //    GameObject.Find("_Application/CursorManager/MouseArrow/").activeSelf)
            //{
            //    MelonCoroutines.Start(SetCursor());
            //}

            if (GameObject.Find("LoadingBackground_TealGradient_Music") != null && GameObject.Find("[Odious] Loading Music Player") == null)
            {
                MelonCoroutines.Start(Audio());
            }

            if (GameObject.Find("UserInterface/MenuContent/Screens/Title/Panel/Text") != null)
            {
                if (GameObject.Find("UserInterface/MenuContent/Screens/Title").activeSelf == false)
                {
                    return;
                }
                MelonCoroutines.Start(InitScreen());
            }
        }

        public override void OnLateUpdate()
        {
            if (GameObject.Find("UserInterface/MenuContent/Popups/LoadingPopup") == null)
            {
                return;
            }

            if (VRCPlayer.field_Internal_Static_VRCPlayer_0)
            {
                if (GameObject.Find("[Odious] Loading Music Player").GetComponent<AudioSource>().isPlaying == false)
                {
                    return;
                }
                GameObject.Find("[Odious] Loading Music Player").GetComponent<AudioSource>().Stop();
            }
        }

        //private System.Collections.IEnumerator SetCursor()
        //{
        //    GameObject cursorObject = GameObject.Find("_Application/CursorManager/MouseArrow/VRCUICursorIcon");
        //    while (cursorObject == null)
        //    {
        //        yield return null;
        //    }

        //    while (cursorObject.GetComponent<SpriteRenderer>().sprite.name == "mouse")
        //    {
        //        cursorObject.GetComponent<SpriteRenderer>().sprite = ResourceManager.GetSprite("remodce.cursor");
        //        yield return null;
        //    }
        //}

        private static void LoadingScreen()
        {
            UnityEngine.Object.Destroy(GameObject.Find("UserInterface/MenuContent/Screens/Title/LogoContainer/vrchatlogo2sided"));
            var textobj = GameObject.Find("UserInterface/MenuContent/Screens/Title/Panel/Text");
            textobj.GetComponent<Text>().m_Text = "Welcome to <color=#8c99e1>Odious</color>";
            textobj.GetComponent<Text>().text = "Welcome to <color=#8c99e1>Odious</color>";
            textobj.active = false;
            textobj.active = true;

            if (GameObject.Find("UserInterface/MenuContent/Screens/Title/LogoContainer/OdiousLogo") != null)
            {
                return;
            }

            GameObject gameObject = new GameObject("OdiousLogo");
            gameObject.transform.parent =
                GameObject.Find("UserInterface/MenuContent/Screens/Title/LogoContainer").transform;
            gameObject.AddComponent<Image>().sprite = ResourceManager.GetSprite("remodce.odious");
            gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        // this was a lot more complex than it should be
        // shoutout to frenchie client (karma) for the idea (creating a gameobject that has an audiosource attached to it that plays the audio)
        // yes, i know other clients had this before frenchie client (notorious to be exact) but i originally got the idea from them
        private System.Collections.IEnumerator Audio()
        {
            if (!MusicEnabled)
            {
                yield break;
            }

            string[] links = new string[]
            {
                "https://github.com/gifmafia/dGhlYmVuZHN3aXRobWU/raw/main/dark%20age.ogg",
            };

            while (bg == null)
            {
                bg = GameObject.Find("UserInterface/LoadingBackground_TealGradient_Music/SkyCube_Baked");
                yield return null;
            }
            bg.active = false;

            while (bg2 == null)
            {
                bg2 = GameObject.Find(
                    "UserInterface/MenuContent/Popups/LoadingPopup/3DElements/LoadingBackground_TealGradient/SkyCube_Baked");
                yield return null;
            }
            bg2.active = false;

            while (s1 == null)
            {
                s1 = GameObject.Find("LoadingBackground_TealGradient_Music/LoadingSound")?.GetComponent<AudioSource>();
                yield return null;
            }
            s1.clip = null;
            s1.volume = 0;

            while (s2 == null)
            {
                s2 = GameObject.Find("UserInterface/MenuContent/Popups/LoadingPopup/LoadingSound")
                    ?.GetComponent<AudioSource>();
                yield return null;
            }
            s2.clip = null;
            s2.volume = 0;

            if (GameObject.Find("[Odious] Loading Music Player"))
            {
                yield break;
            }

            GameObject sound = new GameObject();
            sound.name = "[Odious] Loading Music Player";
            sound.gameObject.AddComponent<AudioSource>();
            var audio = sound.GetComponent<AudioSource>();
            UnityEngine.Object.DontDestroyOnLoad(sound);
            audio.volume = 60f;

            FileInfo[] files = new DirectoryInfo(Environment.CurrentDirectory).GetFiles();
            System.Random rand = new System.Random();
            int randomis = rand.Next(links.Length);
            string getrandomLink = links[UnityEngine.Random.Range(0, links.Length)];

            foreach (FileInfo list in files)
            {
                if (list.Name.Contains(".wav") || list.Name.Contains(".mp3") || list.Name.Contains(".ogg"))
                {
                    UnityWebRequest uwr = UnityWebRequest.Get("file://" + Path.Combine(list.FullName));
                    uwr.SendWebRequest();
                    while (!uwr.isDone)
                    {
                        yield return null;
                    }
                    c = WebRequestWWW.InternalCreateAudioClipUsingDH(uwr.downloadHandler, uwr.url, stream: false,
                        compressed: false, AudioType.UNKNOWN);
                    c.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                }
            }
            
            // if c was never set, that means that no suitable file was present so we stream one.
            if (c == null)
            {
                UnityWebRequest uwr = UnityWebRequest.Get(links[randomis] ?? getrandomLink);
                uwr.SendWebRequest();
                while (!uwr.isDone)
                {
                    yield return null;
                }
                c = WebRequestWWW.InternalCreateAudioClipUsingDH(uwr.downloadHandler, uwr.url, stream: false,
                    compressed: false, AudioType.OGGVORBIS);
                c.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            }


            if (GameObject.Find("LoadingBackground_TealGradient_Music") != null)
            {
                while (audio == null)
                {
                    audio = sound.GetComponent<AudioSource>();
                    yield return null;
                }

                try
                {
                    if (audio.isPlaying)
                    {
                        audio.Stop();
                    }
                }
                catch // we know why this happens, it's safe to throw away the exception
                {
                }

                if (!audio.enabled)
                {
                    audio.enabled = true;
                }

                audio.clip = c;
                audio.volume = 60f;
                audio.loop = true;
                audio.Play();
            }

            if (GameObject.Find("UserInterface/MenuContent/Popups/LoadingPopup/") != null)
            {
                while (audio == null)
                {
                    audio = sound.GetComponent<AudioSource>();
                    yield return null;
                }

                try
                {
                    if (audio.isPlaying)
                    {
                        audio.Stop();
                    }
                }
                catch // we know why this happens, it's safe to throw away the exception
                {
                }

                if (!audio.enabled)
                {
                    audio.enabled = true;
                }

                audio.clip = c;
                audio.volume = 60f;
                audio.loop = true;
                audio.Play();
            }
        }
    }
}
