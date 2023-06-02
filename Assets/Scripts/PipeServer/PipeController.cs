using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace LeadMe
{
    public class PipeController : MonoBehaviour
    {
        //Use a static variable to receive messages from the Handler function that is running on a  
        //different thread
        public static string Message = "";

        //Keep track of the current settings when changing scenes, these are read periodically by the
        //VideoController class to monitor for any changes.
        public static string CurrentAction = "";
        public static string CurrentVideo = "";
        public static string CurrentProjection = "";

        //List of the valid scenes to try and load
        private List<string> ValidScenes = new List<string> { "FlatScreen", "VR180", "VR360" };

        /// <summary>
        /// Make sure the controller is not destroyed between scenes
        /// </summary>
        private void Awake()
        {
            if (FindObjectsOfType<PipeController>().Length > 1)
                Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        ///Instead of using the Update() method, create a basic timer that allows us to check if there
        ///is a new message from the pipe server every second to greatly reduced computing power.
        /// </summary>
        IEnumerator CheckForPipeMessage()
        {
            while (true)
            {
                if (Message != "")
                {
                    Debug.Log(PipeController.Message);
                    determineAction(PipeController.Message);
                    PipeController.Message = "";
                }

                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Start is called before the first frame update, run the pipe server and start the message 
        /// checker coroutine.
        /// </summary>
        void Start()
        {
            //Start the IPC handler
            PipeManager.Run();
            StartCoroutine(CheckForPipeMessage());
        }

        /// <summary>
        /// Basic action handler triggered in CheckForPipeMessage() to handle the current Message value.
        /// Used to change the video type, projection type and video source.
        /// </summary>
        /// <param name="message"></param>
        private void determineAction(string message)
        {
            Debug.Log("Pipe message: " + message);
            var tokens = message.Split(',');

            switch (tokens[0])
            {
                case "Scene":
                    if (!ValidScenes.Contains(tokens[1])) return;
                    SceneManager.LoadScene(tokens[1]);
                    return;

                case "Projection":
                    if (tokens.Length < 3) return;
                    if (tokens[1] != SceneManager.GetActiveScene().name) return;
                    CurrentProjection = tokens[2];
                    return;

                case "Source":
                    CurrentVideo = tokens[1];
                    return;
            }
        }

        /// <summary>
        /// Close the PipeServer on application close
        /// </summary>
        void OnApplicationQuit()
        {
            Debug.Log($"Stopping pipe server: {System.DateTime.Now.ToString("hh:mm:ss")}");
            PipeManager.Close();
        }
    }
}
