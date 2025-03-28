using UnityEngine;
using UnityEngine.EventSystems;

//Example https://www.youtube.com/watch?v=h_BMXDWv10I

public class Pointer : MonoBehaviour
{
    public float m_DefaultLength = 5.0f;
    public GameObject m_Dot;
    public VRInputModule m_InputModule;


    private LineRenderer m_LineRenderer = null;

    private void Awake()
    {
        m_LineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        UpdateLine();
    }

    private void UpdateLine()
    {
        // Check if the InputModule has been loaded
        PointerEventData data = m_InputModule.GetData();
        float targetLength = data.pointerCurrentRaycast.distance == 0 ? m_DefaultLength : data.pointerCurrentRaycast.distance;          

        // Ray cast
        RaycastHit hit = CreateRayCast(targetLength);

        // Default
        Vector3 endPosition = transform.position + (transform.forward * targetLength);

        // Or based on hit
        if(hit.collider != null)
        {
            endPosition = hit.point;
        }

        // Set position of the dot
        m_Dot.transform.position = endPosition;

        // Set linerenderer
        m_LineRenderer.SetPosition(0, transform.position);
        m_LineRenderer.SetPosition(1, endPosition);
    }

    private RaycastHit CreateRayCast(float length)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        Physics.Raycast(ray, out hit, m_DefaultLength);

        return hit;
    }
}
