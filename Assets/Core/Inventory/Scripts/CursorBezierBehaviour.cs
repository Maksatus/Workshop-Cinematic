using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Core.Inventory.Scripts
{
    [Serializable]
    public class CursorBezierBehaviour : PlayableBehaviour
    {
        public Vector2 targetPosition;
        public Vector2 bend  = new Vector2(0f, 120f);
        public AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Vector2 _startPosition;
        private bool    _started;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            _started = false;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var rect = playerData as RectTransform;
            if (rect == null) return;

            if (!_started)
            {
                _startPosition = rect.anchoredPosition;
                _started = true;
            }

            // Контрольные точки автоматически из bend
            Vector2 p1 = Vector2.Lerp(_startPosition, targetPosition, 1f / 3f) + bend;
            Vector2 p2 = Vector2.Lerp(_startPosition, targetPosition, 2f / 3f) + bend;

            double duration = playable.GetDuration();
            float t = duration > 0.0
                ? easing.Evaluate(Mathf.Clamp01((float)(playable.GetTime() / duration)))
                : 1f;

            rect.anchoredPosition = CubicBezier(_startPosition, p1, p2, targetPosition, t);

#if UNITY_EDITOR
            rect.GetComponent<CursorBezierDebugDrawer>()?.SetCurve(_startPosition, p1, p2, targetPosition);
#endif
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
