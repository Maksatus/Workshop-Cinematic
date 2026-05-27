using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace Core.Inventory.Scripts
{
    public class TMPAnimateMixerBehaviour : PlayableBehaviour
    {
        private string _defaultText;
        private Color  _defaultColor;
        private bool   _captured;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var label = playerData as TMP_Text;
            if (label == null) return;

            // Запоминаем оригинальные значения один раз
            if (!_captured)
            {
                _defaultText  = label.text;
                _defaultColor = label.color;
                _captured     = true;
            }

            int   inputCount     = playable.GetInputCount();
            Color blendedColor   = Color.clear;
            float totalWeight    = 0f;
            float dominantWeight = 0f;
            string activeText    = _defaultText;

            for (int i = 0; i < inputCount; i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight <= 0f) continue;

                var input = (ScriptPlayable<TMPAnimateBehaviour>)playable.GetInput(i);
                var b     = input.GetBehaviour();

                blendedColor  += b.color * weight;
                totalWeight   += weight;

                // Текст берём из клипа с наибольшим весом
                if (weight > dominantWeight)
                {
                    dominantWeight = weight;
                    activeText     = b.text;
                }
            }

            if (totalWeight > 0f)
            {
                label.color = blendedColor;
                label.text  = activeText;
            }
            else
            {
                label.color = _defaultColor;
                label.text  = _defaultText;
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            _captured = false;
        }
    }
}
