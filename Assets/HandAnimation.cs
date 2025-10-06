using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class HandAnimation : MonoBehaviour
{
    [SerializeField]
    XRInputValueReader<float> m_GripInput = new XRInputValueReader<float>("Grip");


    void Update()
    {
        if (m_GripInput != null)
        {
            var gripVal = m_GripInput.ReadValue();
            
        }
    }
}
