using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 需要添加UI命名空间

public class FPScheck : MonoBehaviour
{
    public float updateInterval = 1.0f; // 更新间隔
    public Text fpsText; // 引用UI Text组件
    public bool showInConsole = false; // 是否同时在控制台显示

    private float _timer;
    private float _frames = 0;
    private float _timePassed = 0;
    private float _currentFPS = 0;

    private void Start()
    {
        // 如果未指定fpsText，尝试自动查找
        if (fpsText == null)
        {
            GameObject fpsTextObj = GameObject.Find("FPSText");
            if (fpsTextObj != null)
            {
                fpsText = fpsTextObj.GetComponent<Text>();
            }
        }

        // 如果还没有找到，尝试使用任何名为"FPSText"的子对象
        if (fpsText == null && transform.Find("FPSText") != null)
        {
            fpsText = transform.Find("FPSText").GetComponent<Text>();
        }
    }

    private void Update()
    {
        _frames++;
        _timePassed += Time.deltaTime;
        _timer += Time.deltaTime;

        // 使用累积的方式计算平均FPS，更准确
        if (_timePassed >= updateInterval)
        {
            _currentFPS = _frames / _timePassed;

            // 更新UI显示
            if (fpsText != null)
            {
                fpsText.text = $"FPS: {_currentFPS:F1}";
            }

            // 控制台显示（可选）
            if (showInConsole)
            {
                Debug.Log($"{_currentFPS:F2} FPS");
            }

            // 重置计数器
            _frames = 0;
            _timePassed = 0;
        }
    }
}