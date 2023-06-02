using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LeadMe
{
    public class SettingsController : MonoBehaviour
    {
        public Toggle[] toggles;

        void Start()
        {
            foreach (Toggle toggle in toggles)
            {
                // Add an event listener to each toggle's onValueChanged event
                toggle.onValueChanged.AddListener((value) => OnToggleValueChanged(toggle, value));
            }
        }

        private void OnToggleValueChanged(Toggle toggle, bool value)
        {
            GlobalSettings.settings[toggle.name] = value;
        }
    }
}
