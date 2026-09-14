using UnityEngine;

public class RingEscapeCamera : MonoBehaviour
{
    public Color backgroundColor = new Color(0.035f, 0.045f, 0.08f);

    private void Awake()
    {
        Camera cameraComponent = GetComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 10f;
        cameraComponent.backgroundColor = backgroundColor;
    }
}
