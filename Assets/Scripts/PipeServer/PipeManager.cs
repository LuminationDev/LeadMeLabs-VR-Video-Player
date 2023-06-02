using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using leadme_api;

namespace LeadMe
{
    public static class PipeManager
    {
        public static bool isRunning = false;

        /// <summary>
        /// Send a message back to the ParentPipeServer (Running within the LeadMe Station software).
        /// </summary>
        /// <param name="message">A string representing the message to be sent.</param>
        public static void SendLeadMeMessage(string message)
        {
            ParentPipeClient.Send(LogHandler, message);
        }

        // Start is called before the first frame update
        public static void Run()
        {
            if (!isRunning)
            {
                Debug.Log($"Starting pipe server: {System.DateTime.Now.ToString("hh:mm:ss")}");
                isRunning = true;
                PipeServer.Run(
                LogHandler,
                PauseHandler,
                ResumeHandler,
                ShutdownHandler,
                DetailsHandler,
                ActionHandler);
            }
        }

        public static void Close()
        {
            PipeServer.Close();
        }

        /// <summary>
        /// Handle log messages recorded by the leadme_api.dll
        /// </summary>
        /// <param name="message">A string representing a status, error or update.</param>
        private static void LogHandler(string message)
        {
            Debug.Log(message);
        }

        /// <summary>
        /// Pause the current UI session and all peripherals except for the pipe server.
        /// </summary>
        private static void PauseHandler()
        {
            PipeController.CurrentAction = "Pause";
            Debug.Log("Pause");
        }

        /// <summary>
        /// Resume any paused UI session or peripherals.
        /// </summary>
        private static void ResumeHandler()
        {
            PipeController.CurrentAction = "Play";
            Debug.Log("Resume");
        }

        /// <summary>
        /// Shutdown the applications and gracefully exit the program.
        /// </summary>
        private static void ShutdownHandler()
        {
            Debug.Log("Shutdown");

            Application.Quit();
        }

        /// <summary>
        /// Collect the available levels and actions, sending them back using a parent pipe client.
        /// </summary>
        private static void DetailsHandler()
        {
            Debug.Log("Get Details");
            SendLeadMeMessage(Details.Serialize(Initialisation.details));
        }

        /// <summary>
        /// Log or take action on any additional messages that may be sent.
        /// </summary>
        /// <param name="message"></param>
        private static void ActionHandler(string message)
        {
            Debug.Log("Action: " + message);
            PipeController.Message = message;
        }
    }
}
