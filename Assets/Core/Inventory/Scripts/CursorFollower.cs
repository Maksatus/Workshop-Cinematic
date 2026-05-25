using UnityEngine;

[ExecuteAlways]
public class CursorFollower : MonoBehaviour
{
    public Transform target;
    public Camera renderCamera;

    private RectTransform _rect;
    private Canvas _canvas;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (_rect == null) _rect = GetComponent<RectTransform>();
        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
        if (_rect == null || _canvas == null) return;

        var cam = renderCamera != null ? renderCamera : Camera.main;
        if (cam == null) return;

        Vector2 screenPos = cam.WorldToScreenPoint(target.position);

        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            _rect.position = new Vector3(screenPos.x, screenPos.y, 0f);
        }
        else
        {
            var canvasCam = _canvas.renderMode == RenderMode.ScreenSpaceCamera ? _canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.GetComponent<RectTransform>(), screenPos, canvasCam, out Vector2 localPos))
                _rect.anchoredPosition = localPos;
        }
    }
}
