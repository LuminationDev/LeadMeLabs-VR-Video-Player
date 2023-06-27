using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LeadMe
{
    /// <summary>
    /// A class for controller the video player associated with the prefab menu.
    /// </summary>
    public class VideoController : MonoBehaviour
    {
        #region Video Management
        // Video screen and player references
        [Header("Video Player")]
        public GameObject videoController;
        private static VideoPlayer videoPlayer;
        private string LatestProjectionType = "MONO";
        private string LatestAction = "";

        // Time references
        [Header("Time Display")]
        public GameObject timeTracker;
        public GameObject currentTimeText;
        public GameObject endTimeText;
        private RectTransform timeTrackerWindow;

        // Static variable for time increments
        private static double TIME_INTERVAL = 5.0;

        // Current settings values
        private bool isRepeatOn;
        private bool isVoidOn;

        void Start()
        {
            //Get the parent of the timetracker as this is the 'Rail' it moves along
            timeTrackerWindow = (RectTransform)timeTracker.transform.parent.gameObject.transform;

            //Set the values for the video player management and preload the any video or projection that may already be set.
            videoPlayer = videoController.GetComponent<VideoPlayer>();
            videoPlayer.errorReceived += OnVideoErrorReceived;
            UpdateVideoSettings();
            UpdateProjectSettings();

            //Read the global url settings
            if (!string.IsNullOrEmpty(GlobalSettings.CurrentUrl))
            {
                SetVideoSource((string)GlobalSettings.CurrentUrl);

                //TODO this is not working, or video time is being reset afterwards somewhere
                SetCurrentTime((double)GlobalSettings.CurrentTime);

                //Check for the void
                CheckForSkybox();

                PlayVideo();
            }
            else if (!videoPlayer.isPlaying && !string.IsNullOrEmpty(videoPlayer.url))
            {
                LoadFirstFrame(videoPlayer.url);
            }

            StartCoroutine(CheckProjectSettings());
        }

        void OnDestroy()
        {
            videoPlayer.errorReceived -= OnVideoErrorReceived;
            StopCoroutine(CheckProjectSettings());
        }

        /// <summary>
        /// Check for an errors in the video source or video player in general.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        private void OnVideoErrorReceived(VideoPlayer source, string message)
        {
            Debug.LogError($"Video player encountered an error for source: {source}; error: {message}");
            PipeController.CurrentVideo = null;
            GlobalSettings.CurrentUrl = "";
            GlobalSettings.CurrentTime = 0;
        }

        /// <summary>
        /// Instead of using the Update() method, create a basic timer that allows us to check
        /// the settings of the project every second to greatly reduced computing power.
        /// </summary>
        IEnumerator CheckProjectSettings()
        {
            while (true)
            {
                UpdateVideoSettings();
                UpdateTimeText();
                UpdateVideoAction();
                UpdateProjectSettings();

                //This can be decreased if reaction time deemed to slow.
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Check the current settings for the video source and projection type against the any messages that may of come from the 
        /// pipe server, updating the parts if required.
        /// </summary>
        private void UpdateVideoSettings()
        {
            if (videoPlayer.url != PipeController.CurrentVideo && !string.IsNullOrEmpty(PipeController.CurrentVideo))
            {
                videoPlayer.clip = null;
                videoPlayer.Stop();
                videoPlayer.url = PipeController.CurrentVideo;
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += OnVideoPrepareCompleted;

                //Update global url settings
                GlobalSettings.CurrentUrl = videoPlayer.url;
                GlobalSettings.CurrentTime = 0;

                //Clear the PipeController variable
                PipeController.CurrentVideo = "";
            }

            if (LatestProjectionType != PipeController.CurrentProjection && !string.IsNullOrEmpty(PipeController.CurrentProjection))
            {
                ChangeProjection(PipeController.CurrentProjection);

                //Clear the PipeController variable
                PipeController.CurrentProjection = "";
            }
        }

        /// <summary>
        /// Check if the supplied source can be played in the current video player or if it is corrupt or an incorrect file type.
        /// </summary>
        private void OnVideoPrepareCompleted(VideoPlayer source)
        {
            if (source.isPrepared)
            {
                //Check for the void
                CheckForSkybox();
                Debug.Log("Video is playable!");
                source.Play();
            }
            else
            {
                Debug.LogError("Video is not playable!");
                PipeController.CurrentVideo = null;
            }

            source.prepareCompleted -= OnVideoPrepareCompleted;
        }

        /// <summary>
        /// Check the current time of the video, updating the UI to the correct position.
        /// </summary>
        private void UpdateTimeText()
        {
            if (videoPlayer.length == 0) return;

            //Update the global settings
            GlobalSettings.CurrentTime = GetCurrentTime();
            
            //Update the current time text and end time incase the source has changed
            currentTimeText.GetComponent<Text>().text = ConvertTimeToString(GetCurrentTime());
            endTimeText.GetComponent<Text>().text = ConvertTimeToString(GetTotalTime());

            //Update the time tracker location
            // Get the current position of the object
            Vector3 currentPosition = timeTracker.transform.localPosition;

            // Modify the x value of the position against the total video time length (450) and the current time as a percent of the total time
            currentPosition.x = (float)(timeTrackerWindow.rect.width * (GetCurrentTime() / GetTotalTime()) - timeTrackerWindow.rect.width / 2);

            // Set the new position of the object
            timeTracker.transform.localPosition = currentPosition;
        }

        /// <summary>
        /// Convert a seconds into a 0:00 time format string for display to a user.
        /// </summary>
        /// <param name="seconds">A double representing the seconds to convert.</param>
        /// <returns>A string in 0:00 format.</returns>
        private string ConvertTimeToString(double seconds)
        {
            return string.Format("{0:00}:{1:00}", Mathf.FloorToInt((float)seconds / 60f), Mathf.FloorToInt((float)seconds % 60f));
        }

        /// <summary>
        /// Check if the current action has changed, this is currently set to Play or Pause.
        /// </summary>
        private void UpdateVideoAction()
        {
            LatestAction = PipeController.CurrentAction;

            switch (LatestAction)
            {
                case "Play":
                    if (!videoPlayer.isPlaying)
                    {
                        PlayVideo();
                    }
                    break;
                case "Pause":
                    if (videoPlayer.isPlaying)
                    {
                        PauseVideo();
                    }
                    break;
            }
        }

        /// <summary>
        /// Check the global settings for any changes and take an actions that are required.
        /// </summary>
        private void UpdateProjectSettings()
        {
            bool newVoid = GlobalSettings.GetVoidStatus();
            if (isVoidOn != newVoid)
            {
                isVoidOn = newVoid;
                SetDefaultSkyboxOrVoid(isVoidOn);
            }

            bool newRepeat = GlobalSettings.GetRepeatStatus();
            if (isRepeatOn != newRepeat)
            {
                isRepeatOn = newRepeat;
                videoPlayer.isLooping = isRepeatOn;
            }
        }

        /// <summary>
        /// Load a local file that has been selected from the Source popup menu
        /// </summary>
        public void LoadLocalFileSource(GameObject textObject)
        {
            string textValue = textObject.GetComponent<Text>().text;
            Debug.Log("Text Value: " + textValue);

            LocalFile localFile;
            if (Initialisation.localFiles.TryGetValue(textValue, out localFile))
            {
                Debug.Log($"{textValue} value: {localFile.filePath}");
                PipeController.CurrentVideo = "";
                LoadFirstFrame(localFile.filePath);
            }
            else if (FileController.localFiles.TryGetValue(textValue, out localFile))
            {
                Debug.Log($"{textValue} value: {localFile.filePath}");
                PipeController.CurrentVideo = "";
                LoadFirstFrame(localFile.filePath);
            }
            else
            {
                Debug.Log($"{textValue} not found in dictionary");
                return;
            }

            //TODO update global url settings
            GlobalSettings.CurrentUrl = localFile.filePath;
            GlobalSettings.CurrentTime = 0;
        }

        /// <summary>
        /// Load and then play the video for a little bit so that the first frame appears.
        /// </summary>
        /// <param name="path">A string URL pointing to the location of the new source.</param>
        private void LoadFirstFrame(string path)
        {
            SetVideoSource(path);

            //Check for the void
            CheckForSkybox();

            PlayVideo();
        }

        /// <summary>
        /// Play the currently loaded video.
        /// </summary>
        public void PlayVideo()
        {
            videoPlayer.Play();
        }

        /// <summary>
        /// Pause the currently loaded video.
        /// </summary>
        public void PauseVideo()
        {
            videoPlayer.Pause();
        }

        /// <summary>
        /// Stop the currently loaded video. Returning the video time to 0 seconds and changing the Skybox to the
        /// preselected default.
        /// </summary>
        public void StopVideo()
        {
            videoPlayer.Stop();
            PipeController.CurrentVideo = "";
            videoPlayer.url = "";
            GlobalSettings.CurrentUrl = "";
            GlobalSettings.CurrentTime = 0;

            //Reset the current video time text back to 0
            currentTimeText.GetComponent<Text>().text = "0:00";

            SetDefaultSkyboxOrVoid(isVoidOn);
        }

        /// <summary>
        /// Increase the current time of the video by the TIME_INTERVAL. Only trigger if the current time 
        /// is above 0 so we know it is prepared and is more than 5 seconds away from the total time.
        /// </summary>
        public void FastForwardVideo()
        {
            if (videoPlayer.length == 0) return;
            double current = GetCurrentTime();
            if (current > 0 && current < GetTotalTime() - TIME_INTERVAL)
            {
                current += TIME_INTERVAL;
                SetCurrentTime(current);
            }
        }

        /// <summary>
        /// Decrease the current time of the video by the TIME_INTERVAL. Only trigger if the current time 
        /// is above the TIME_INTERVAL.
        /// </summary>
        public void RewindVideo()
        {
            if (videoPlayer.length == 0) return;
            double current = GetCurrentTime();
            if (current > TIME_INTERVAL)
            {
                current -= TIME_INTERVAL;
                SetCurrentTime(current);
            }
        }

        /// <summary>
        /// Return the total time of the video that is currently loaded.
        /// </summary>
        /// <returns>A double representing the length of the video in seconds.</returns>
        public double GetTotalTime()
        {
            if (videoPlayer.length == 0) return 0;
            return videoPlayer.length;
        }

        /// <summary>
        /// Return the current time of the video that is currently loaded.
        /// </summary>
        /// <returns>A double representing the current time of the video in seconds.</returns>
        public double GetCurrentTime()
        {
            return videoPlayer.time;
        }

        /// <summary>
        /// Set the current time of the video that is currently loaded.
        /// </summary>
        public void SetCurrentTime(double seconds)
        {
            videoPlayer.time = seconds;
        }

        /// <summary>
        /// Set the source of the connected video player. This may be a local URI/video clip, or a link. It
        /// may require further changes to the videoPlayer parameters.
        /// </summary>
        /// <param name="source"></param>
        public static void SetVideoSource(string source)
        {
            videoPlayer.url = source;
        }
        #endregion

        #region Projection Management
        /// <summary>
        /// Change the current projection type, this is only applicable for 180 and 360 VR scenes. Flat
        /// screen will always be displayed as a mono projection. This may involve changed the current 
        /// rendering texture.
        /// </summary>
        /// <param name="projectionType"></param>
        public void ChangeProjection(string projectionType)
        {
            Debug.Log("Changing to projection: " + projectionType);
            LatestProjectionType = projectionType;

            bool isVRMode360 = SceneManager.GetActiveScene().name.Equals("VR360");
            bool isVRMode180 = SceneManager.GetActiveScene().name.Equals("VR180");
            ChangeRenderTexture(projectionType == "MONO", isVRMode360, isVRMode180);

            switch (projectionType)
            {
                case "EAC":
                    SetEACProjection();
                    break;

                case "EAC3D":
                    SetEAC3DProjection();
                    break;

                case "OU":
                    SetOUProjection(isVRMode360 ? "360" : "180");
                    break;

                case "SBS":
                    SetSBSProjection(isVRMode360 ? "360" : "180");
                    break;

                case "MONO":
                    SetMONOProjection(isVRMode360 ? "360" : "180");
                    break;

                default:
                    Debug.Log("Projection default: Flat");
                    break;
            }
        }

        /// <summary>
        /// Set the Equi-Angular Cubemap projection on the current skybox.
        /// </summary>
        public void SetEACProjection()
        {
            RenderSettings.skybox = (Material)Resources.Load("Materials/360/PanoramicSkyboxEAC") as Material;
        }

        /// <summary>
        /// Set the Equi-Angular Cubemap 3D projection on the current skybox.
        /// </summary>
        public void SetEAC3DProjection()
        {
            RenderSettings.skybox = (Material)Resources.Load("Materials/360/PanoramicSkybox3DEAC") as Material;
        }

        /// <summary>
        /// Set the Over under projection on the current skybox.
        /// </summary>
        public void SetOUProjection(string folder)
        {
            RenderSettings.skybox = (Material)Resources.Load($"Materials/{folder}/PanoramicSkybox3DOverUnder") as Material;
        }

        /// <summary>
        /// Set the Side by Side projection on the current skybox.
        /// </summary>
        public void SetSBSProjection(string folder)
        {
            RenderSettings.skybox = (Material)Resources.Load($"Materials/{folder}/PanoramicSkybox3Dside") as Material;
        }

        /// <summary>
        /// Set the Monoscopic projection on the current skybox. NOTE: This requires a different render texture
        /// that the other projections.
        /// </summary>
        public void SetMONOProjection(string folder)
        {
            RenderSettings.skybox = (Material)Resources.Load($"Materials/{folder}/PanoramicSkybox") as Material;
        }

        /// <summary>
        /// Set the Monoscopic projection on the current skybox. NOTE: This requires a different render texture
        /// that the other projections.
        /// </summary>
        public void SetFlatProjection()
        {
            Debug.Log("Set projection to flat mode.");
        }

        /// <summary>
        /// Set the skybox as either the default (currently space) skybox or the black void. On 180 or 360 scenes
        /// this is only present when the video source is null, and can be toggled in the flat screen.
        /// </summary>
        public void SetDefaultSkyboxOrVoid(bool isVoid)
        {
            if (isVoid)
            {
                RenderSettings.skybox = (Material)Resources.Load($"Skyboxes/Void") as Material;
            } else
            {
                RenderSettings.skybox = (Material)Resources.Load($"Skyboxes/6sidedCosmicCoolCloud") as Material;
            }
        }

        /// <summary>
        /// Change the render texture depending if the video is Monoscopic or another projection type. The main
        /// difference is the resolution that a video takes up whilst wrapped on the skybox.
        /// </summary>
        /// <param name="isMono">A bool representing the if the projection type is mono</param>
        /// <param name="is360">A bool representing the if the scene type is 360 degree</param>
        /// <param name="is180">A bool representing the if the scene type is 180 degree</param>
        public void ChangeRenderTexture(bool isMono, bool is360, bool is180)
        {
            if(isMono && !is180)
            {
                videoPlayer.targetTexture = (RenderTexture)Resources.Load("Renderers/MonoRenderTexture") as RenderTexture;
            } else if(is360)
            {
                videoPlayer.targetTexture = (RenderTexture)Resources.Load("Renderers/360RenderTexture") as RenderTexture;
            } else
            {
                videoPlayer.targetTexture = (RenderTexture)Resources.Load("Renderers/180RenderTexture") as RenderTexture;
            }
        }

        /// <summary>
        /// Check if the skybox is current set as a void of the render material.
        /// </summary>
        private void CheckForSkybox()
        {
            Material skyboxMaterial = RenderSettings.skybox;

            // Check if skyboxMaterial is not null
            if (skyboxMaterial != null)
            {
                // Get the name of the skybox material
                string skyboxName = skyboxMaterial.name;
                Debug.Log("Skybox Material Name: " + skyboxName);

                if(skyboxName.Equals("Void") || skyboxName.Equals("6sidedCosmicCoolCloud"))
                {
                    ChangeProjection("MONO");
                }
            }
            else
            {
                Debug.Log("No skybox material is set.");
                ChangeProjection("MONO");
            }
        }
        #endregion
    }
}
