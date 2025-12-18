using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPScheck : MonoBehaviour
{
    public float updateInterval = 1.0f;

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= updateInterval)
        {
            float fps = 1f / Time.deltaTime;
            Debug.Log($"{fps:F2} FPS");
            _timer = 0f;
        }
    }
}
