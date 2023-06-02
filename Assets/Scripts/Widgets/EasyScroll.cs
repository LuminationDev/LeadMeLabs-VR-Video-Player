using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

public class EasyScroll : MonoBehaviour
{
    [Header("Scroll Management")]
    public SteamVR_Input_Sources controller;
    public SteamVR_Action_Vector2 thumbstickAction;
    public GameObject container;
    public float scrollSpeed = 1.0f;

    //Track the movement of the thumbstick for scrolling viewports
    void Update()
    {
        if (container.activeSelf)
        {
            // Get thumbstick input
            Vector2 thumbstickInput = thumbstickAction.GetAxis(controller);

            // Scroll the view based on thumbstick input
            if (thumbstickInput.y != 0)
            {
                Debug.Log(thumbstickInput);
                gameObject.GetComponent<ScrollRect>().verticalNormalizedPosition += thumbstickInput.y * scrollSpeed * Time.deltaTime;
            }
        }
    }
}
