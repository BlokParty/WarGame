using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

namespace PlayTable.Unity
{
    public class PTUI : MonoBehaviour
    {
        public const string UNITYDEFAULT_UILayerName = "UI";
        public const float UNITYDEFAULT_Width = 160f;
        public const float UNITYDEFAULT_ThickHeight = 30f;
        public const float UNITYDEFAULT_ThinHeight = 20f;
        public const string UNITYDEFAULT_StandardUISpritePath = "UI/Skin/UISprite.psd";
        public const string UNITYDEFAULT_BackgroundSpriteResourcePath = "UI/Skin/Background.psd";
        public const string UNITYDEFAULT_InputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        public const string UNITYDEFAULT__KnobPath = "UI/Skin/Knob.psd";
        public const string UNITYDEFAULT_CheckmarkPath = "UI/Skin/Checkmark.psd";

        public static Vector2 UNITYDEFAULT_ThickGUIElementSize = new Vector2(UNITYDEFAULT_Width, UNITYDEFAULT_ThickHeight);
        public static Vector2 UNITYDEFAULT_ThinGUIElementSize = new Vector2(UNITYDEFAULT_Width, UNITYDEFAULT_ThinHeight);
        public static Vector2 UNITYDEFAULT_ImageGUIElementSize = new Vector2(100f, 100f);
        public static Color UNITYDEFAULT_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        public static Color UNITYDEFAULT_PanelColor = new Color(1f, 1f, 1f, 0.392f);
        public static Color UNITYDEFAULT_TEXTCOLOR = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

        IEnumerator LoadSceneCouroutine(string sceneName)
        {
            //Delete all unwanted dontdestroyonload objects
            foreach (NetworkDiscovery nd in FindObjectsOfType<NetworkDiscovery>())
            {
                Destroy(nd.gameObject);
            }

            // The Application loads the Scene in the background at the same time as the current Scene.
            //This is particularly good for creating loading screens. You could also load the Scene by build //number.
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            //Wait until the last operation fully loads to return anything
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        /**
         * \cond HIDDEN_SYMBOLS
         * 
         * 
         */
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneCouroutine(sceneName));
        }

        public void ToggleActive(GameObject other)
        {
            other.SetActive(!other.activeSelf);
        }
    }
}


