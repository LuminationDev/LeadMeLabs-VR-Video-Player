using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class VRInputModule : BaseInputModule
{
    [Header("Interaction Pointer")]
    public Camera m_Camera;

    [Header("Input Management")]
    public SteamVR_Input_Sources m_RightTargetSource;
    public SteamVR_Input_Sources m_LeftTargetSource;
    public SteamVR_Action_Boolean m_RightTriggerAction;
    public SteamVR_Action_Boolean m_LeftTriggerAction;

    [Header("Menu Management")]
    public GameObject m_camera;
    public GameObject menuMain;
    public GameObject menuPointer;

    private GameObject m_CurrentObject = null;
    private PointerEventData m_Data = null;
    private float m_MenuDistance = 0;

    protected override void Awake()
    {
        base.Awake();

        m_Data = new PointerEventData(eventSystem);
        m_MenuDistance = menuMain.transform.position.z;
    }

    #region VR Input Management
    public override void Process()
    {
        // Reset data, set camera
        m_Data.Reset();
        m_Data.position = new Vector2(m_Camera.pixelWidth / 2, m_Camera.pixelHeight / 2);

        // Raycast
        eventSystem.RaycastAll(m_Data, m_RaycastResultCache);
        m_Data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        m_CurrentObject = m_Data.pointerCurrentRaycast.gameObject;

        // Clear
        m_RaycastResultCache.Clear();

        // Hover
        HandlePointerExitAndEnter(m_Data, m_CurrentObject);

        // Press
        if(m_RightTriggerAction.GetStateDown(m_RightTargetSource))
        {
            ProcessPress(m_Data);
        }

        // Release
        if (m_RightTriggerAction.GetStateUp(m_RightTargetSource))
        {
            ProcessRelease(m_Data);
        }

        // Show/Hide Menu and Line renderer only on release
        if (m_LeftTriggerAction.GetStateUp(m_LeftTargetSource))
        {
            ManageMenuVisibility();
        }
    }

    public PointerEventData GetData()
    {
        return m_Data;
    }

    private void ProcessPress(PointerEventData data)
    {
        // Set raycast
        data.pointerPressRaycast = data.pointerCurrentRaycast;

        // Check for object hit, get the down handler, call
        GameObject newPointerPress = ExecuteEvents.ExecuteHierarchy(m_CurrentObject, data, ExecuteEvents.pointerDownHandler);

        // If no downhandler, try and get click handler
        if (newPointerPress == null)
        {
            newPointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(m_CurrentObject);
        }

        // Set data
        data.pressPosition = data.position;
        data.pointerPress = newPointerPress;
        data.rawPointerPress = m_CurrentObject;
    }

    private void ProcessRelease(PointerEventData data)
    {
        // Execute pointer up
        ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

        // Check for click handler
        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(m_CurrentObject);

        // Check if actual
        if(data.pointerPress == pointerUpHandler)
        {
            ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
        }

        // Clear selected gameobject
        eventSystem.SetSelectedGameObject(null);

        // Reset data
        data.pressPosition = Vector2.zero;
        data.pointerPress = null;
        data.rawPointerPress = null;
    }
    #endregion

    #region Menu Management
    /// <summary>
    /// Open or close the main menu and pointer. The menu is controlled by the CanvasGroup component
    /// </summary>
    public void ManageMenuVisibility()
    {
        bool show = !menuPointer.activeSelf;
        menuPointer.SetActive(show);

        CanvasGroup menu = menuMain.GetComponent<CanvasGroup>();

        bool isMenuShown = menu.alpha == 1;
        if(!isMenuShown) MoveMenu();
        
        menu.alpha = isMenuShown ? 0 : 1;
        menu.interactable = !isMenuShown;
        menu.blocksRaycasts = !isMenuShown;
    }

    /// <summary>
    /// Make the Menu follow the camera rotation when not active, so when it becomes active it
    /// is always in front of the player.
    /// </summary>
    private void MoveMenu()
    {
        //calculate the new position
        Quaternion cameraRotation = m_camera.transform.rotation; //get the cameras y rotation value
        float radY = ((cameraRotation.eulerAngles.y) * (Mathf.PI)) / 180; //convert eulerAngle degrees to radians
        menuMain.transform.position = new Vector3((m_MenuDistance * (Mathf.Sin(radY)) + 0.2F), menuMain.transform.position.y, m_MenuDistance * (Mathf.Cos(radY))); //work out the point in a circle

        //calculate the new rotation 
        menuMain.transform.rotation = Quaternion.Euler(new Vector3(gameObject.transform.rotation.eulerAngles.x, cameraRotation.eulerAngles.y, 0));
    }
    #endregion
}
