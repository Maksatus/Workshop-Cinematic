using UnityEngine.Splines;
using UnityEngine;

public class SplineHandler : MonoBehaviour
{
    public SplineAnimate SplineAnimateComponent;
    public float SplineEvaluateValue;
    
    void Update()
    {
        SplineAnimateComponent.ElapsedTime = SplineEvaluateValue;
    }
}
