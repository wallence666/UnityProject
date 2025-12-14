using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInteraction : MonoBehaviour
{
    [Header("呼叫器设置")]
    [SerializeField] private List<CallDevice> callDevices = new List<CallDevice>();  // 可以操作多个呼叫器
    [SerializeField] private float interactionDistance = 3f;  // 交互距离

    [Header("按钮材质设置")]
    [SerializeField] private Material buttonActivatedMaterial;  // 按钮激活时的材质
    [SerializeField] private Material buttonDeactivatedMaterial;  // 按钮关闭时的材质
    [SerializeField] private float buttonActiveDuration = 1f;  // 按钮激活持续时间

    private Camera playerCamera;
    private MeshRenderer buttonIndicatorRenderer;
    private bool isPlayerLooking = false;
    private bool isButtonActive = false;
    private float buttonActivationTime = 0f;

    private void Start()
    {
        // 查找玩家相机
        playerCamera = Camera.main;

        // 自动查找按钮子物体中的胶囊体Renderer
        FindButtonIndicatorRenderer();

        // 设置初始材质
        if (buttonIndicatorRenderer != null && buttonDeactivatedMaterial != null)
        {
            buttonIndicatorRenderer.material = buttonDeactivatedMaterial;
        }
        else if (buttonIndicatorRenderer == null)
        {
            Debug.LogWarning($"按钮 {gameObject.name} 没有找到胶囊体指示器");
        }

        // 检查呼叫器列表
        if (callDevices.Count == 0)
        {
            Debug.LogWarning($"按钮 {gameObject.name} 没有设置任何呼叫器");
        }
    }

    private void FindButtonIndicatorRenderer()
    {
        MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in childRenderers)
        {
            // 通过名称识别胶囊体
            string objName = renderer.gameObject.name.ToLower();
            if (objName.Contains("capsule") || objName.Contains("indicator") || objName.Contains("buttonlight"))
            {
                buttonIndicatorRenderer = renderer;
                Debug.Log($"找到按钮指示器: {renderer.gameObject.name}");
                break;
            }
        }

        // 如果没找到胶囊体，则使用按钮自身的Renderer
        if (buttonIndicatorRenderer == null)
        {
            buttonIndicatorRenderer = GetComponent<MeshRenderer>();
            if (buttonIndicatorRenderer != null)
            {
                Debug.Log($"使用按钮自身Renderer作为指示器");
            }
        }
    }

    private void Update()
    {
        if (playerCamera == null) return;

        // 检测玩家是否看着按钮
        CheckIfPlayerIsLooking();

        // 检测交互输入
        if (isPlayerLooking && Input.GetMouseButtonDown(0))
        {
            OnButtonClicked();
        }

        // 更新按钮激活状态
        UpdateButtonState();
    }

    private void CheckIfPlayerIsLooking()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        isPlayerLooking = Physics.Raycast(ray, out hit, interactionDistance) &&
                         hit.collider.gameObject == gameObject;
    }

    private void OnButtonClicked()
    {
        Debug.Log("按钮被点击!");

        // 激活按钮指示器
        ActivateButton();

        // 触发所有呼叫器（调用Call模式）
        ActivateAllCallDevices();
    }

    private void ActivateAllCallDevices()
    {
        if (callDevices.Count == 0)
        {
            Debug.LogWarning("没有呼叫器可以激活");
            return;
        }

        int activatedCount = 0;
        foreach (CallDevice callDevice in callDevices)
        {
            if (callDevice != null)
            {
                callDevice.ActivateCall();  // 调用Call模式
                activatedCount++;
            }
        }

        Debug.Log($"成功激活 {activatedCount}/{callDevices.Count} 个呼叫器[Call模式]");
    }

    private void ActivateButton()
    {
        isButtonActive = true;
        buttonActivationTime = Time.time;

        // 设置按钮指示器材质为激活材质
        if (buttonIndicatorRenderer != null && buttonActivatedMaterial != null)
        {
            buttonIndicatorRenderer.material = buttonActivatedMaterial;
        }

        Debug.Log($"按钮激活，持续 {buttonActiveDuration} 秒");
    }

    private void UpdateButtonState()
    {
        // 如果按钮激活中，检查是否到达持续时间
        if (isButtonActive && Time.time - buttonActivationTime >= buttonActiveDuration)
        {
            DeactivateButton();
        }
    }

    private void DeactivateButton()
    {
        isButtonActive = false;

        // 恢复按钮指示器材质为关闭材质
        if (buttonIndicatorRenderer != null && buttonDeactivatedMaterial != null)
        {
            buttonIndicatorRenderer.material = buttonDeactivatedMaterial;
        }

        Debug.Log("按钮关闭");
    }

    // 添加呼叫器到列表
    public void AddCallDevice(CallDevice device)
    {
        if (!callDevices.Contains(device))
        {
            callDevices.Add(device);
            Debug.Log($"已将呼叫器 {device.gameObject.name} 添加到按钮 {gameObject.name}");
        }
    }

    // 从列表中移除呼叫器
    public void RemoveCallDevice(CallDevice device)
    {
        if (callDevices.Contains(device))
        {
            callDevices.Remove(device);
            Debug.Log($"已将呼叫器 {device.gameObject.name} 从按钮 {gameObject.name} 移除");
        }
    }

    // 清除所有呼叫器
    public void ClearAllCallDevices()
    {
        callDevices.Clear();
        Debug.Log($"已清除按钮 {gameObject.name} 的所有呼叫器");
    }

    // 激活所有呼叫器的警报模式（如果需要可以从其他脚本调用）
    public void ActivateAllCallDevicesAlarm()
    {
        if (callDevices.Count == 0)
        {
            Debug.LogWarning("没有呼叫器可以激活");
            return;
        }

        int activatedCount = 0;
        foreach (CallDevice callDevice in callDevices)
        {
            if (callDevice != null)
            {
                callDevice.ActivateAlarm();  // 调用Alarm模式
                activatedCount++;
            }
        }

        Debug.Log($"成功激活 {activatedCount}/{callDevices.Count} 个呼叫器[Alarm模式]");
    }

    // 在Unity编辑器中显示交互距离
    private void OnDrawGizmosSelected()
    {
        // 显示交互距离
        Gizmos.color = Color.blue;
        Vector3 startPos = transform.position;
        if (playerCamera != null)
        {
            startPos = playerCamera.transform.position;
        }
        else if (Camera.main != null)
        {
            startPos = Camera.main.transform.position;
        }
        Gizmos.DrawLine(startPos, startPos + transform.forward * interactionDistance);

        // 显示连接到所有呼叫器的线
        Gizmos.color = Color.yellow;
        foreach (CallDevice callDevice in callDevices)
        {
            if (callDevice != null)
            {
                Gizmos.DrawLine(transform.position, callDevice.transform.position);
            }
        }

        // 显示按钮指示器范围
        if (buttonIndicatorRenderer != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.1f);  // 红色半透明
            Bounds bounds = buttonIndicatorRenderer.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}