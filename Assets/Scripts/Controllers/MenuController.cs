using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine;

namespace LeadMe
{
    /// <summary>
    /// A class for controlling the visibility of the main menu and sub menus (pop ups)
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        // Menu Management
        [Header("Menu Management")]
        public GameObject menuPointer;
        public GameObject scenePopup;
        public GameObject projectionTrigger;
        public GameObject projectionPopup;
        public GameObject sourcePopup;
        public GameObject settingsPopup;

        void Start()
        {
            //Hide the projection type if on the flat screen scene
            projectionTrigger.SetActive(SceneManager.GetActiveScene().name != "FlatScreen");
        }

        /// <summary>
        /// Change the scene to the supplied one.
        /// </summary>
        /// <param name="levelName">A string representing the level to change to.</param>
        public void ChangeScene(string levelName)
        {
            SceneManager.LoadScene(levelName);
        }

        /// <summary>
        /// Open or close the scene pop up panel and perform any other actions that are required.
        /// </summary>
        public void ManageScenePopUp()
        {
            scenePopup.SetActive(!scenePopup.activeSelf);
        }

        /// <summary>
        /// Open or close the projection pop up panel and perform any other actions that are required.
        /// </summary>
        public void ManageProjectionPopUp()
        {
            projectionPopup.SetActive(!projectionPopup.activeSelf);
        }

        /// <summary>
        /// Open or close the scene pop up panel and perform any other actions that are required.
        /// </summary>
        public void ManageSourcePopUp()
        {
            sourcePopup.SetActive(!sourcePopup.activeSelf);
        }

        /// <summary>
        /// Open or close the settings pop up panel and perform any other actions that are required.
        /// </summary>
        public void ManageSettingsPopUp()
        {
            settingsPopup.SetActive(!settingsPopup.activeSelf);
        }

        /// <summary>
        /// Close the main menu and disable the pointer when a new source is set. The menu is 
        /// controlled by the CanvasGroup component.
        /// </summary>
        public void CloseMenuOnSourceSelection()
        {
            menuPointer.SetActive(false);
            CanvasGroup menu = gameObject.GetComponent<CanvasGroup>();

            menu.alpha = 0;
            menu.interactable = false;
            menu.blocksRaycasts = false;
        }
    }
}

