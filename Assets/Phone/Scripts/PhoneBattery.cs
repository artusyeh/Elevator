using UnityEngine;
public sealed class PhoneBattery : MonoBehaviour
{
    [Range(0,100)]public int percentage=100;
    public int Percentage=>Mathf.Clamp(percentage,0,100);
}
