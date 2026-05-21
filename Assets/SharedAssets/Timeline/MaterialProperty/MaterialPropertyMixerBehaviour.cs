using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Timeline.Samples
{
    public enum MaterialPropertyType { Float, Color }

    public class MaterialPropertyMixerBehaviour : PlayableBehaviour
    {
        public string propertyName = "_Alpha";
        public MaterialPropertyType propertyType = MaterialPropertyType.Float;

        Image m_TrackBinding;
        float m_DefaultFloat;
        Color m_DefaultColor;
        bool m_DefaultsCaptured;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var image = playerData as Image;
            UpdateBinding(image);

            if (m_TrackBinding == null)
                return;

            var mat = m_TrackBinding.material;
            if (mat == null)
                return;

            CaptureDefaults(mat);

            int inputCount = playable.GetInputCount();
            float blendedFloat = 0f;
            Color blendedColor = Color.clear;
            float totalWeight = 0f;

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight <= 0f)
                    continue;

                var inputPlayable = (ScriptPlayable<MaterialPropertyBehaviour>)playable.GetInput(i);
                MaterialPropertyBehaviour input = inputPlayable.GetBehaviour();

                blendedFloat += input.floatValue * inputWeight;
                blendedColor += input.colorValue * inputWeight;
                totalWeight += inputWeight;
            }

            if (totalWeight <= 0f)
                return;

            if (propertyType == MaterialPropertyType.Float)
                mat.SetFloat(propertyName, Mathf.Lerp(m_DefaultFloat, blendedFloat, totalWeight));
            else
                mat.SetColor(propertyName, Color.Lerp(m_DefaultColor, blendedColor, totalWeight));
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            RestoreDefaults();
        }

        void UpdateBinding(Image image)
        {
            if (image == m_TrackBinding)
                return;

            RestoreDefaults();
            m_TrackBinding = image;
            m_DefaultsCaptured = false;
        }

        void CaptureDefaults(Material mat)
        {
            if (m_DefaultsCaptured)
                return;

            if (mat.HasProperty(propertyName))
            {
                if (propertyType == MaterialPropertyType.Float)
                    m_DefaultFloat = mat.GetFloat(propertyName);
                else
                    m_DefaultColor = mat.GetColor(propertyName);
            }

            m_DefaultsCaptured = true;
        }

        void RestoreDefaults()
        {
            if (m_TrackBinding == null || !m_DefaultsCaptured)
                return;

            var mat = m_TrackBinding.material;
            if (mat == null || !mat.HasProperty(propertyName))
                return;

            if (propertyType == MaterialPropertyType.Float)
                mat.SetFloat(propertyName, m_DefaultFloat);
            else
                mat.SetColor(propertyName, m_DefaultColor);
        }
    }
}
