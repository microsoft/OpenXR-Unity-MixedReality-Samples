using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.OpenXR;

public class ChangeRenderMode : MonoBehaviour
{

    private bool m_coroutineStarted = false;
    // Update is called once per frame
    void Update()
    {
        if (!m_coroutineStarted)
        {
            m_coroutineStarted = true;
            StartCoroutine(ToggleRenderMode());
        }
    }

    public IEnumerator ToggleRenderMode()
    {
        for (int i = 0; i < 50; i++)
        {
            if (OpenXRSettings.Instance.renderMode == OpenXRSettings.RenderMode.SinglePassInstanced)
            {
                OpenXRSettings.Instance.renderMode = OpenXRSettings.RenderMode.MultiPass;
            }
            else
            {
                OpenXRSettings.Instance.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
