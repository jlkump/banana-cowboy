using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeManager : MonoBehaviour
{
    private CinemachineBasicMultiChannelPerlin cinCamera;
    private float _shakeTimer;
    private float _shakeTimerTotal;
    private float _startintIntensity;

    public static ScreenShakeManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        cinCamera = GameObject.Find("Third Person Camera").GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float intensity, float frequency, float time)
    {
        _shakeTimerTotal = time;
        cinCamera.m_AmplitudeGain = intensity;
        cinCamera.m_FrequencyGain = frequency;
        _startintIntensity = intensity;
        _shakeTimer = time;
    }

    private void Update()
    {
        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.deltaTime;
            cinCamera.m_AmplitudeGain = Mathf.Lerp(_startintIntensity, 0f, 1 - (_shakeTimer / _shakeTimerTotal));
            
        }
    }
}
