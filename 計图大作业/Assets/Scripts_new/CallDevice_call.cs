using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallDevice : MonoBehaviour
{
    // 定义两种模式
    public enum DeviceMode
    {
        Call,   // 呼叫模式
        Alarm   // 警报模式
    }

    [Header("设备设置")]
    [SerializeField] private DeviceMode currentMode = DeviceMode.Call;  // 当前设备模式
    [SerializeField] private float callDuration = 3f;  // Call模式持续时间
    [SerializeField] private float alarmDuration = 5f;  // Alarm模式持续时间

    [Header("材质设置")]
    [SerializeField] private Material activatedMaterial;  // 激活时的材质
    [SerializeField] private Material deactivatedMaterial;  // 关闭时的材质

    [Header("音频设置")]
    [SerializeField] private AudioClip callAudio;  // 呼叫音频
    [SerializeField] private AudioClip alarmAudio;  // 警报音频
    [SerializeField] private float audioVolume = 0.5f;  // 音频音量

    private MeshRenderer deviceRenderer;
    private AudioSource audioSource;
    private bool isActive = false;
    private float activationTime = 0f;
    private DeviceMode activeMode;  // 当前激活的模式
    private float currentActiveDuration;  // 当前激活的持续时间

    private void Start()
    {
        // 优先查找胶囊体（子物体）的Renderer
        FindCapsuleRenderer();

        // 如果没找到胶囊体，则使用Cube的Renderer
        if (deviceRenderer == null)
        {
            deviceRenderer = GetComponent<MeshRenderer>();
        }

        // 添加音频源组件
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;  // 3D音效
        audioSource.volume = audioVolume;

        // 设置初始材质
        if (deviceRenderer != null && deactivatedMaterial != null)
        {
            deviceRenderer.material = deactivatedMaterial;
        }
    }

    private void FindCapsuleRenderer()
    {
        MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in childRenderers)
        {
            // 通过名称识别胶囊体
            string objName = renderer.gameObject.name.ToLower();
            if (objName.Contains("capsule") || objName.Contains("indicator"))
            {
                deviceRenderer = renderer;
                break;
            }
        }
    }

    private void Update()
    {
        // 如果激活中，检查是否到达持续时间
        if (isActive && Time.time - activationTime >= currentActiveDuration)
        {
            Deactivate();
        }
    }

    // 激活呼叫模式（由按钮调用）
    public void ActivateCall()
    {
        ActivateDevice(DeviceMode.Call, callDuration);
    }

    // 激活警报模式
    public void ActivateAlarm()
    {
        ActivateDevice(DeviceMode.Alarm, alarmDuration);
    }

    // 通用激活方法
    private void ActivateDevice(DeviceMode mode, float duration)
    {
        if (isActive) return;  // 已经在激活状态则不重复触发

        isActive = true;
        activeMode = mode;
        currentActiveDuration = duration;
        activationTime = Time.time;

        // 改变材质
        if (deviceRenderer != null && activatedMaterial != null)
        {
            deviceRenderer.material = activatedMaterial;
        }

        // 播放对应模式的音频
        PlayModeAudio(mode);

        Debug.Log($"呼叫器激活[{mode}模式]，持续 {duration} 秒");
    }

    private void PlayModeAudio(DeviceMode mode)
    {
        AudioClip clipToPlay = null;

        switch (mode)
        {
            case DeviceMode.Call:
                clipToPlay = callAudio;
                break;
            case DeviceMode.Alarm:
                clipToPlay = alarmAudio;
                break;
        }

        if (clipToPlay != null && audioSource != null)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
        else if (clipToPlay == null)
        {
            Debug.LogWarning($"呼叫器未设置{mode}模式音频");
        }
    }

    private void Deactivate()
    {
        isActive = false;

        // 恢复材质
        if (deviceRenderer != null && deactivatedMaterial != null)
        {
            deviceRenderer.material = deactivatedMaterial;
        }

        // 停止音频（如果还在播放）
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Debug.Log($"呼叫器[{activeMode}模式]关闭");
    }

    // 设置设备模式（可用于外部控制）
    public void SetDeviceMode(DeviceMode newMode)
    {
        currentMode = newMode;
        Debug.Log($"呼叫器模式设置为: {newMode}");
    }

    // 设置Call模式持续时间
    public void SetCallDuration(float duration)
    {
        callDuration = Mathf.Max(0.1f, duration);  // 确保持续时间大于0
        Debug.Log($"Call模式持续时间设置为: {callDuration}秒");
    }

    // 设置Alarm模式持续时间
    public void SetAlarmDuration(float duration)
    {
        alarmDuration = Mathf.Max(0.1f, duration);  // 确保持续时间大于0
        Debug.Log($"Alarm模式持续时间设置为: {alarmDuration}秒");
    }

    // 获取Call模式持续时间
    public float GetCallDuration()
    {
        return callDuration;
    }

    // 获取Alarm模式持续时间
    public float GetAlarmDuration()
    {
        return alarmDuration;
    }

    // 切换模式（呼叫<->警报）
    public void ToggleMode()
    {
        currentMode = (currentMode == DeviceMode.Call) ? DeviceMode.Alarm : DeviceMode.Call;
        Debug.Log($"呼叫器模式切换为: {currentMode}");
    }

    // 获取当前模式
    public DeviceMode GetCurrentMode()
    {
        return currentMode;
    }

    // 获取当前是否激活
    public bool IsActive()
    {
        return isActive;
    }
}