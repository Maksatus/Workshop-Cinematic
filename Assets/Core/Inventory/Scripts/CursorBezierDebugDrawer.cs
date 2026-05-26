using UnityEngine;

namespace Core.Inventory.Scripts
{
    [ExecuteAlways]
    public class CursorBezierDebugDrawer : MonoBehaviour
    {
        private Vector2 _p0, _p1, _p2, _p3;
        private bool _hasData;
        private RectTransform _rect;

        void Awake() => _rect = GetComponent<RectTransform>();

        public void SetCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            _p0 = p0; _p1 = p1; _p2 = p2; _p3 = p3;
            _hasData = true;
        }

        void OnDrawGizmos()
        {
            if (!_hasData) return;
            if (_rect == null) _rect = GetComponent<RectTransform>();
            if (_rect == null) return;

            // Кривая
            Gizmos.color = new Color(0f, 0.9f, 1f, 0.85f);
            Vector2 prev = _p0;
            for (int i = 1; i <= 40; i++)
            {
                float t = i / 40f;
                Vector2 next = CubicBezier(_p0, _p1, _p2, _p3, t);
                Gizmos.DrawLine(ToWorld(prev), ToWorld(next));
                prev = next;
            }

            // Касательные к контрольным точкам
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            Gizmos.DrawLine(ToWorld(_p0), ToWorld(_p1));
            Gizmos.DrawLine(ToWorld(_p3), ToWorld(_p2));

            // Конечная точка
            Gizmos.color = new Color(0f, 1f, 0.4f, 0.9f);
            Gizmos.DrawSphere(ToWorld(_p3), 4f);
        }

        private Vector3 ToWorld(Vector2 anchoredPos)
        {
            if (_rect.parent == null) return anchoredPos;
            return _rect.parent.TransformPoint(new Vector3(anchoredPos.x, anchoredPos.y, 0f));
        }

        private static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            return u * u * u * p0
                 + 3f * u * u * t * p1
                 + 3f * u * t * t * p2
                 +     t * t * t * p3;
        }
    }
}
