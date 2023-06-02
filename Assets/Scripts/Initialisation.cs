using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using leadme_api;

namespace LeadMe
{
    /// <summary>
    /// Read in any command line arguments as the project is starting up and hold a reference to the projects 
    /// details that are to be sent to LeadMe Labs.
    /// </summary>
    class Initialisation
    {
        //List of the valid file types to try and load
        private static List<string> ValidFileTypes = new List<string> { ".mp4" };

        //Path to the specialised LeadMe video folder
        public static string folderPath = @"C:\Users\ecoad\Videos\Test"; //TODO replace with the specialised LeadMe video folder
        public static Dictionary<string, LocalFile> localFiles = new();

        // A reference to any arguments supplied to the program
        public static string cmdInfo = "";

        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeMethodLoad()
        {
            Debug.Log("After Scene is loaded and game is running look for command line arguments");
            ReadCommandLine();
            LoadLeadMeFiles();
        }

        /// <summary>
        /// Read any arguements that may have been supplied through the command line, this could mean
        /// that a particular video source and projection must be loaded first.
        /// </summary>
        private static void ReadCommandLine()
        {
            #nullable enable
            string? SuppliedScene = null;

            string[] arguments = System.Environment.GetCommandLineArgs();
            foreach (string arg in arguments)
            {
                cmdInfo += arg.ToString() + "\n ";

                string[] tokens = arg.Split("=");

                if (tokens.Length < 2) return;

                // Set the CurrentVideo and Projection arguments in the PipeController if required
                switch (tokens[0])
                {
                    case "projection":
                        PipeController.CurrentProjection = tokens[1];
                        break;

                    case "video":
                        PipeController.CurrentVideo = tokens[1];
                        break;

                    case "scene":
                        SuppliedScene = tokens[1];
                        break;
                }
            }
            
            Debug.Log($"Arguments: {cmdInfo}");

            //Always change the scene after all commands have been read
            if(SuppliedScene != null)
            {
                SceneManager.LoadScene(SuppliedScene);
            } 
        }

        /// <summary>
        /// Collect the files in the local folder, adding them to the Source popup menu as
        /// UI List items.
        /// </summary>
        private static async void LoadLeadMeFiles()
        {
            string[] files = Directory.GetFiles(folderPath);

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                if(!localFiles.ContainsKey(fileName) && ValidFileTypes.Contains(Path.GetExtension(filePath)))
                {
                    LocalFile temp = new LocalFile(fileName, filePath);
                    localFiles.Add(fileName, temp);

                    // Add to the details being sent to LeadMe
                    details.levels[3].actions.Add(new Action { name = fileName, trigger = $"Source,file://{filePath}" });
                }
            }

            // Send the experience details on start up
            PipeManager.SendLeadMeMessage(Details.Serialize(Initialisation.details));

            //Wait for the initial loading screen to finish
            await Task.Delay(3000);

            //Move to the Flat Screen scene if still on the Entry
            if (SceneManager.GetActiveScene().name == "Entry")
            {
                SceneManager.LoadScene("FlatScreen");
            }
        }

        /// <summary>
        /// A Details class instantiation that represents all the information about the experience.This can be sent
        /// over a network as a string with the use of the Details Serialize function.
        /// 
        /// Projection types are sent with three values, the Namespace (Projection), the related scene (eg. VR180) 
        /// and the type (eg. MONO). This is to stop VR180 scenes trying to load EAC projections for example.
        /// </summary>
        public static Details details = new Details
        {
            name = "InputTest",
            globalActions = new List<GlobalAction>
            {
                new GlobalAction { name = "Play", trigger = "resume" },
                new GlobalAction { name = "Pause", trigger = "pause" },
                new GlobalAction { name = "Shutdown", trigger = "shutdown" }
            },
            levels = new List<Level>
            {
                new Level
                {
                    name = "Flat Screen",
                    trigger = "Scene,FlatScreen",
                    actions = new List<Action>
                    {
                    }
                },
                new Level
                {
                    name = "180",
                    trigger = "Scene,VR180",
                    actions = new List<Action>
                    {
                        new Action { name = "Monoscopic", trigger = "Projection,VR180,MONO" },
                        new Action { name = "Over Under", trigger = "Projection,VR180,OU" },
                        new Action { name = "Side By Side", trigger = "Projection,VR180,SBS" }
                    }
                },
                new Level
                {
                    name = "360",
                    trigger = "Scene,VR360",
                    actions = new List<Action>
                    {
                        new Action { name = "Monoscopic", trigger = "Projection,VR360,MONO" },
                        new Action { name = "Over Under", trigger = "Projection,VR360,OU" },
                        new Action { name = "Side By Side", trigger = "Projection,VR360,SBS" },
                        new Action { name = "EAC", trigger = "Projection,VR360,EAC" },
                        new Action { name = "EAC 3D", trigger = "Projection,VR360,EAC3D" }
                    }
                },
                new Level
                {
                    name = "Sources",
                    trigger = "",
                    actions = new List<Action>()
                }
            }
        };
    }
}
