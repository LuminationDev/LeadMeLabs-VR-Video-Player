using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeadMe
{
    /// <summary>
    /// A static class that holds the currently selected settings for this session. These
    /// values stay the same across multiple scenes and are changed through the SettingsController
    /// attached to each Main Menu GameObject.
    /// </summary>
    public class GlobalSettings
    {
        /// <summary>
        /// A dictionary that holds all the settings, organised by their key and their associated
        /// value.
        /// </summary>
        public static Dictionary<string, bool> settings = new()
        {
            { "isVoidOn", false },
            { "isRepeatOn", false }
        };

        public static bool GetVoidStatus()
        {
            return settings["isVoidOn"];
        }

        public static bool GetRepeatStatus()
        {
            return settings["isRepeatOn"];
        }

        /// <summary>
        /// Hold the current video variables, tracks what URL is playing and
        /// what time it is currently at. This will handle the video loading when switching between
        /// different scenes.
        /// </summary>
        public static string CurrentUrl { get; set; } = "";
        public static double CurrentTime { get; set; } = 0;
    }
}
