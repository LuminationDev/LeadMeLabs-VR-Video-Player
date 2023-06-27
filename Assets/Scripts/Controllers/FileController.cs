using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace LeadMe
{
    public class FileController : MonoBehaviour
    {
        //File Management
        [Header("File Management")]
        public Transform contentPanel;
        public GameObject listItemPrefab;
        public GameObject directoryItemPrefab;
        public GameObject noItemsPrefab;
        public GameObject backstepButton;
        public GameObject currentFolderText;

        //private string currentFolderPath = @"Z:\Development Team\LeadMe Labs\testing_videos";
        private string currentFolderPath = @"C:\Users";

        //List of the valid file types to try and load
        private List<string> ValidFileTypes = new List<string> { ".mp4" };

        public static Dictionary<string, LocalFile> localFiles = new();
        public List<string> localDirectories = new();

        void OnEnable()
        {
            currentFolderPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\videos";
        }

        // Start is called before the first frame update
        void Start()
        {
            UpdateCurrentFolderText();

            // Check if there is a parent folder beyond the current one
            if (Path.GetDirectoryName(currentFolderPath) == null)
            {
                print("At the root folder");
                backstepButton.SetActive(false);
            }

            // Load the initial values from the supplied starting point (currentFolderPath)
            LoadLocalDirectories();
        }

        /// <summary>
        /// Update the UI text showing what folder the user is currently in.
        /// </summary>
        private void UpdateCurrentFolderText()
        {
            currentFolderText.GetComponentInChildren<Text>().text = currentFolderPath;
        }

        /// <summary>
        /// A Directory prefab item has been clicked, update the currentFolderPath with the new directory
        /// name and reload the sub directories/files (now active) as the new UI.
        /// </summary>
        public void StepInDirectory(string folder)
        {
            currentFolderPath = $"{currentFolderPath}\\{folder}";
            UpdateCurrentFolderText();

            // Check if there is a parent folder beyond the new one
            if (Path.GetDirectoryName(currentFolderPath) != null && !backstepButton.activeSelf)
            {
                print("No longer at root folder");
                backstepButton.SetActive(true);
            }

            // Reload the UI
            LoadLocalDirectories();
        }

        /// <summary>
        /// The step out button has been pressed so return to the parent folder of the currentFolderPath,
        /// if there is one and reload the UI with the now active directories and files.
        /// </summary>
        public void StepOutDirectory()
        {
            // Get the parent folder of the current directory
            currentFolderPath = Path.GetDirectoryName(currentFolderPath);

            UpdateCurrentFolderText();

            // Check if there is a parent folder beyond the new one
            if (Path.GetDirectoryName(currentFolderPath) == null)
            {
                print("At the root folder");
                backstepButton.SetActive(false);
            }

            // Reload the UI
            LoadLocalDirectories();
        }

        /// <summary>
        /// Collect the files in the local folder, adding them to the Source popup menu as
        /// UI List items.
        /// </summary>
        private void LoadLocalDirectories()
        {
            // Clear the previous directory entires
            localDirectories = new();

            // Load the directories at the current level
            string[] directories = Directory.GetDirectories(currentFolderPath);

            // Add the directory names to the new list of directories
            foreach (string directoryPath in directories)
            {
                // Get the last folder in the current path
                string directoryName = Path.GetFileName(directoryPath);
                if (!localDirectories.Contains(directoryName))
                {
                    localDirectories.Add(directoryName);
                }
            }

            // Load the local files that are at the current level
            LoadLocalFiles();
        }

        /// <summary>
        /// Collect the files in the local folder, adding them to the Source popup menu as
        /// UI List items.
        /// </summary>
        private void LoadLocalFiles()
        {
            // Clear the previous file entries
            localFiles = new();

            // Load the files at the current location
            string[] files = Directory.GetFiles(currentFolderPath);

            // Collect the required information for the onClick listener
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                if (!localFiles.ContainsKey(fileName) && ValidFileTypes.Contains(Path.GetExtension(filePath)))
                {
                    print($"Adding valid file: {fileName}");
                    LocalFile temp = new LocalFile(fileName, filePath);
                    localFiles.Add(fileName, temp);
                }
            }

            // Update the source UI
            PopulateSourcePopup();
        }

        /// <summary>
        /// Read through the current file list and populate the source popup with 
        /// the names of the found files. If no files were found, add the default noItems prefab
        /// instead.
        /// </summary>
        private async void PopulateSourcePopup()
        {
            GameObject directoryItemElement = directoryItemPrefab;
            GameObject listItemElement = listItemPrefab;

            // Clear the current UI board
            DestroyUIChildren();

            await Task.Delay(200);

            // Check if no files exist
            if (localFiles.Count == 0 && localDirectories.Count == 0)
            {
                GameObject listItem = Instantiate(noItemsPrefab, contentPanel.transform);
                listItem.GetComponentInChildren<Text>().text = "There were no items found in the local folder.";
                return;
            }

            // Load in the locally found directories
            foreach (string item in localDirectories)
            {
                GameObject directoryItem = Instantiate(directoryItemElement, contentPanel.transform);
                directoryItem.GetComponentInChildren<Text>().text = item;

                //Assign an onclick delegate to open the sub-directory
                directoryItem.GetComponent<Button>().onClick.AddListener(delegate { StepInDirectory(item); });
            }

            // Load in the locally found files
            foreach (KeyValuePair<string, LocalFile> item in localFiles)
            {
                GameObject listItem = Instantiate(listItemElement, contentPanel.transform);
                listItem.GetComponentInChildren<Text>().text = item.Value.fileName;
            }
        }

        /// <summary>
        /// Remove all child UI buttons that have been placed on the UI canvas by a previous directory
        /// search, this will avoid endlessly adding new values to the UI and give the file management
        /// look.
        /// </summary>
        private void DestroyUIChildren()
        {
            foreach (Transform child in contentPanel.transform)
            {
                print($"Destroying: {child.gameObject}");
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}
