using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background;
    public RectTransform handle;

    private Vector2 inputVector;
    private Vector2 originalHandlePosition;

    void Start()
    {
        originalHandlePosition = handle.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 backgroundPosition = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, background.position);
        Vector2 radius = background.sizeDelta / 2;
        inputVector = (eventData.position - backgroundPosition) / radius;
        inputVector = inputVector.magnitude > 1.0f ? inputVector.normalized : inputVector;

        handle.anchoredPosition = new Vector2(originalHandlePosition.x + inputVector.x * radius.x, originalHandlePosition.y + inputVector.y * radius.y);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = originalHandlePosition;
    }

    public Vector2 GetInputDirection()
    {
        return inputVector;
    }
}
