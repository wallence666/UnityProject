using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private MeshRenderer buttonRenderer;  // 按钮的胶囊体渲染器
    [SerializeField] private Material activatedMaterial;   // 激活时的材质
    [SerializeField] private Material deactivatedMaterial; // 非激活时的材质

    [Header("交互设置")]
    [SerializeField] private float interactionDistance = 3f;  // 交互距离
    [SerializeField] private string mouseClickButton = "Fire1";  // 交互按键（默认鼠标左键）
    [SerializeField] private float activationDuration = 5f;  // 激活持续时间（秒）

    [Header("相机设置")]
    [SerializeField] private string targetCameraName = "PlayerCamera";  // 目标相机名称

    [Header("呼叫设备")]
    [SerializeField] private List<CallDevice> targetCallDevices = new List<CallDevice>();  // 目标呼叫设备列表

    private Camera playerCamera;
    private bool isPlayerLooking = false;
    private bool isButtonActive = false;
    private Coroutine activationCoroutine;

    private void Start()
    {
        // 如果未指定按钮渲染器，自动查找胶囊体
        if (buttonRenderer == null)
        {
            FindButtonRenderer();
        }

        // 查找玩家相机
        FindPlayerCamera();

        // 设置初始材质为非激活状态
        SetButtonMaterial(deactivatedMaterial);
    }

    private void FindButtonRenderer()
    {
        // 查找所有子物体的MeshRenderer
        MeshRenderer[] childRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in childRenderers)
        {
            // 通过物体名称或标签识别胶囊体
            string objName = renderer.gameObject.name.ToLower();
            if (objName.Contains("capsule") ||
                objName.Contains("button") ||
                renderer.gameObject == gameObject)
            {
                buttonRenderer = renderer;
                break;
            }
        }
    }

    private void FindPlayerCamera()
    {
        // 根据名称查找相机
        GameObject cameraObj = GameObject.Find(targetCameraName);
        if (cameraObj != null)
        {
            playerCamera = cameraObj.GetComponent<Camera>();
            if (playerCamera == null)
            {
                Debug.LogError($"找到的对象 {targetCameraName} 没有Camera组件");
            }
        }
        else
        {
            // 如果没找到指定名称的相机，尝试使用主相机
            playerCamera = Camera.main;
            if (playerCamera != null)
            {
                Debug.LogWarning($"未找到名为 {targetCameraName} 的相机，使用主相机");
            }
            else
            {
                Debug.LogError($"未找到相机: 既没有名为 {targetCameraName} 的相机，也没有标签为MainCamera的相机");
            }
        }
    }

    private void Update()
    {
        if (playerCamera == null || buttonRenderer == null) return;

        // 检测玩家是否看着按钮
        CheckIfPlayerIsLooking();

        // 检测交互输入
        if (isPlayerLooking && Input.GetButtonDown(mouseClickButton) && !isButtonActive)
        {
            OnButtonClicked();
        }
    }

    private void CheckIfPlayerIsLooking()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // 检测射线是否击中按钮本身或其子物体
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // 检查是否击中了按钮本身或按钮的子物体
            isPlayerLooking = IsHitObjectOrChild(hit.collider.gameObject);
        }
        else
        {
            isPlayerLooking = false;
        }
    }

    private bool IsHitObjectOrChild(GameObject hitObject)
    {
        // 如果击中的是按钮本身
        if (hitObject == gameObject)
        {
            return true;
        }

        // 如果击中的是按钮的子物体
        Transform currentTransform = hitObject.transform;
        while (currentTransform.parent != null)
        {
            if (currentTransform.parent.gameObject == gameObject)
            {
                return true;
            }
            currentTransform = currentTransform.parent;
        }

        return false;
    }

    private void OnButtonClicked()
    {
        // 激活按钮
        ActivateButton();

        // 激活所有关联的呼叫设备的Call模式
        if (targetCallDevices.Count > 0)
        {
            foreach (CallDevice device in targetCallDevices)
            {
                if (device != null)
                {
                    device.ActivateCall();
                }
            }
        }
    }

    private void ActivateButton()
    {
        if (isButtonActive) return;  // 防止重复激活

        isButtonActive = true;

        // 应用激活材质
        SetButtonMaterial(activatedMaterial);

        // 启动计时器，在指定时间后恢复非激活状态
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
        }
        activationCoroutine = StartCoroutine(DeactivateAfterDelay());
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDuration);

        // 恢复非激活状态
        DeactivateButton();
    }

    private void DeactivateButton()
    {
        if (!isButtonActive) return;

        isButtonActive = false;

        // 恢复非激活材质
        SetButtonMaterial(deactivatedMaterial);
    }

    private void SetButtonMaterial(Material material)
    {
        if (buttonRenderer != null && material != null)
        {
            buttonRenderer.material = material;
        }
    }

    // 外部调用方法：手动激活按钮
    public void ManuallyActivateButton()
    {
        OnButtonClicked();
    }

    // 外部调用方法：手动恢复按钮
    public void ManuallyDeactivateButton()
    {
        DeactivateButton();
    }

    // 添加单个呼叫设备
    public void AddCallDevice(CallDevice device)
    {
        if (device != null && !targetCallDevices.Contains(device))
        {
            targetCallDevices.Add(device);
        }
    }

    // 移除单个呼叫设备
    public void RemoveCallDevice(CallDevice device)
    {
        if (targetCallDevices.Contains(device))
        {
            targetCallDevices.Remove(device);
        }
    }

    // 设置呼叫设备列表
    public void SetCallDevices(List<CallDevice> devices)
    {
        targetCallDevices.Clear();
        targetCallDevices.AddRange(devices);
    }

    // 清空呼叫设备列表
    public void ClearCallDevices()
    {
        targetCallDevices.Clear();
    }

    // 获取所有关联的呼叫设备
    public List<CallDevice> GetCallDevices()
    {
        return new List<CallDevice>(targetCallDevices);
    }

    // 获取按钮当前状态
    public bool IsButtonActive()
    {
        return isButtonActive;
    }

    // 设置激活持续时间
    public void SetActivationDuration(float duration)
    {
        activationDuration = Mathf.Max(0.1f, duration);
    }

    // 获取激活持续时间
    public float GetActivationDuration()
    {
        return activationDuration;
    }

    // 获取关联的呼叫设备数量
    public int GetCallDeviceCount()
    {
        return targetCallDevices.Count;
    }

    // 设置检测相机名称
    public void SetCameraName(string cameraName)
    {
        targetCameraName = cameraName;
        FindPlayerCamera();
    }

    private void OnDrawGizmosSelected()
    {
        // 在编辑器中显示交互距离
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        // 绘制到每个呼叫设备的连接线（如果设备存在）
        if (targetCallDevices.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (CallDevice device in targetCallDevices)
            {
                if (device != null)
                {
                    Gizmos.DrawLine(transform.position, device.transform.position);
                }
            }
        }

        // 绘制到相机的连接线（如果相机存在）
        if (playerCamera != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, playerCamera.transform.position);
        }
    }
}