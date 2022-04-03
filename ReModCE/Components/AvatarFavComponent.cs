using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.UI;
using MelonLoader;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.VRChat;

namespace ReModCE.Components
{
    internal class AvatarFavComponent : ModComponent
    {
        private static GameObject avatarPage;
        public static UiAvatarList newFavList;
        public static Il2CppSystem.Collections.Generic.List<ApiAvatar> favedAvatars = new Il2CppSystem.Collections.Generic.List<ApiAvatar>();
        bool JustOpened;

        // onupdate to check if the menu is open, if true then refresh our favorites
        public override void OnUpdate()
        {
            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 == null)
                return;

            if (avatarPage.activeSelf && !JustOpened)
            {
                JustOpened = true;
                MelonCoroutines.Start(RefreshMenu(1f));
            }
            else if (!avatarPage.activeSelf && JustOpened)
                JustOpened = false;
        }

        public static void UI()
        {
            // check if our favorites json exists, if not then create it.
            if (!File.Exists($"UserData/Odious/odious_favorites.json"))
            {
                File.Create($"UserData/Odious/odious_favorites.json");
            }

            // fetches avatar page
            avatarPage = GameObject.Find("UserInterface/MenuContent/Screens/Avatar");
            newFavList = VRCUiManager.prop_VRCUiManager_0.field_Public_GameObject_0.transform.Find("Screens/Avatar/Vertical Scroll View/Viewport/Content/Legacy Avatar List").gameObject.GetComponent<UiAvatarList>();
            newFavList.transform.SetSiblingIndex(0);
            newFavList.clearUnseenListOnCollapse = false;
            newFavList.field_Public_Category_0 = UiAvatarList.Category.SpecificList;
            newFavList.GetComponentInChildren<Text>().text = "Odious Favorites";

            // creates buttons for favorites system to work
            GameObject NewFavButton = UnityEngine.Object.Instantiate<GameObject>(VRCUiManager.prop_VRCUiManager_0.field_Public_GameObject_0.transform.Find("Screens/Avatar/Favorite Button").gameObject, VRCUiManager.prop_VRCUiManager_0.field_Public_GameObject_0.transform.Find("Screens/Avatar/"));
            NewFavButton.GetComponentInChildren<RectTransform>().localPosition = new Vector3(238.9283f, 372.6159f, -2);
            GameObject NewLoadButton = UnityEngine.Object.Instantiate<GameObject>(VRCUiManager.prop_VRCUiManager_0.field_Public_GameObject_0.transform.Find("Screens/Avatar/Favorite Button").gameObject, VRCUiManager.prop_VRCUiManager_0.field_Public_GameObject_0.transform.Find("Screens/Avatar/"));
            NewLoadButton.GetComponentInChildren<RectTransform>().localPosition = new Vector3(-224.6199f, 373.36f, -2);
            PageAvatar pageAvatar = VRCUiManager.prop_VRCUiManager_0.field_Public_GameObject_0.transform.Find("Screens/Avatar/").GetComponentInChildren<PageAvatar>();
            Button NewFavButtonButton = NewFavButton.GetComponent<Button>();
            Button NewLoadButtonButton = NewLoadButton.GetComponent<Button>();
            NewLoadButtonButton.transform.Find("Horizontal/FavoritesCountSpacingText").gameObject.SetActive(false);
            NewLoadButtonButton.transform.Find("Horizontal/FavoritesCurrentCountText").gameObject.SetActive(false);
            NewLoadButtonButton.transform.Find("Horizontal/FavoritesCountDividerText").gameObject.SetActive(false);
            NewLoadButtonButton.transform.Find("Horizontal/FavoritesMaxAvailableText").gameObject.SetActive(false);
            NewLoadButtonButton.GetComponentInChildren<Text>().text = "Refresh Avatars";
            NewLoadButtonButton.gameObject.SetActive(true);
            NewLoadButtonButton.onClick.RemoveAllListeners();
            NewLoadButtonButton.onClick.AddListener(new System.Action(() =>
            {
                MelonCoroutines.Start(RefreshMenu(1f));
                newFavList.StartRenderElementsCoroutine(favedAvatars);
            }));
            NewFavButtonButton.transform.Find("Horizontal/FavoritesCountSpacingText").gameObject.SetActive(false);
            NewFavButtonButton.transform.Find("Horizontal/FavoritesCurrentCountText").gameObject.SetActive(false);
            NewFavButtonButton.transform.Find("Horizontal/FavoritesCountDividerText").gameObject.SetActive(false);
            NewFavButtonButton.transform.Find("Horizontal/FavoritesMaxAvailableText").gameObject.SetActive(false);
            NewFavButtonButton.GetComponentInChildren<Text>().text = "Favorite/UnFavorite";
            NewFavButtonButton.gameObject.SetActive(true);
            NewFavButtonButton.onClick.RemoveAllListeners();

            // controls if an avatar is added to/removed from the favorites
            NewFavButtonButton.onClick.AddListener(new System.Action(() => {
                ApiAvatar apiAvatar = pageAvatar.field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0;
                if (favedAvatars.Contains(apiAvatar))
                {
                    favedAvatars.Remove(apiAvatar);
                    string[] arrLine = File.ReadAllLines("UserData/Odious/odious_favorites.json");
                    string avText = "";
                    for (int i = 0; i < arrLine.Length; i++)
                    {
                        if (!arrLine[i].Contains(apiAvatar.id))
                        {
                            avText += arrLine[i];
                        }

                        File.WriteAllText("UserData/Odious/odious_favorites.json", avText);
                        newFavList.StartRenderElementsCoroutine(favedAvatars);
                    }
                }

                else
                {
                    favedAvatars.Add(apiAvatar);
                    MelonCoroutines.Start(RefreshMenu(1f));
                    File.AppendAllText("UserData/Odious/odious_favorites.json", apiAvatar.id + "|" + apiAvatar.name + "|" + apiAvatar.thumbnailImageUrl + "\n");
                    newFavList.StartRenderElementsCoroutine(favedAvatars);
                }
            }));

            string[] avatars = File.ReadAllLines("UserData/Odious/odious_favorites.json");
            for (int i = 0; i < avatars.Length; i++)
            {
                string[] args = avatars[i].Split('|');
                favedAvatars.Add(new ApiAvatar { id = args[0], name = args[1], thumbnailImageUrl = args[2] });
            }

            MelonCoroutines.Start(RefreshMenu(1f));
        }

        public static IEnumerator RefreshMenu(float v)
        {
            yield return new WaitForSeconds(v);
            newFavList.StartRenderElementsCoroutine(favedAvatars);
            yield break;
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            UI();
            MelonCoroutines.Start(RefreshMenu(1f));
        }
    }
}
