using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject menuImage1;          // 菜单Image1
    [SerializeField] private GameObject monitorImage2;       // 监控画面Image2
    [SerializeField] private RawImage cameraRawImage;        // Image3 (RawImage)
    [SerializeField] private Text text1;                     // 提示文本1
    [SerializeField] private Text text2;                     // 提示文本2
    [SerializeField] private Text text3;                     // 提示文本3
    [SerializeField] private Text text4;                     // 新增：提示文本4
    [SerializeField] private Text text5;                     // 新增：提示文本5
    [SerializeField] private Text text6;                     // 新增：提示文本6
    [SerializeField] private Text text7;                     // 新增：提示文本7

    [Header("灯光管理")]
    [SerializeField] private LightManager lightManager;      // 灯光管理器
    [SerializeField] private bool initialLightMode = true;   // 初始灯光模式：true=模式1(开灯), false=模式2(关灯)

    [Header("Render Textures")]
    [SerializeField] private RenderTexture[] renderTextures; // 多个RenderTexture用于切换

    [Header("呼叫器设置")]
    [SerializeField] private List<CallDevice> callDevices = new List<CallDevice>();  // 关联的呼叫器列表

    // 状态变量
    private bool isMenuActive = false;
    private bool isMonitorActive = false;
    private int currentCameraIndex = 0;
    private bool isLightMode1 = true; // true=模式1(开灯), false=模式2(关灯)
    private int currentTextIndex = 0; // 新增：当前显示的文本索引（0:text4, 1:text5, 2:text6, 3:text7）

    private void Start()
    {
        // 初始状态：菜单关闭，监控关闭，显示文本1
        SetInitialState();
    }

    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// 设置初始状态
    /// </summary>
    private void SetInitialState()
    {
        // 关闭所有界面
        if (menuImage1 != null) menuImage1.SetActive(false);
        if (monitorImage2 != null) monitorImage2.SetActive(false);

        // 设置文本显示状态
        if (text1 != null) text1.gameObject.SetActive(true);
        if (text2 != null) text2.gameObject.SetActive(false);
        if (text3 != null) text3.gameObject.SetActive(false);

        // 新增：设置text4-text7的初始状态
        if (text4 != null) text4.gameObject.SetActive(true);  // 初始显示text4
        if (text5 != null) text5.gameObject.SetActive(false);
        if (text6 != null) text6.gameObject.SetActive(false);
        if (text7 != null) text7.gameObject.SetActive(false);

        currentTextIndex = 0; // 初始显示text4

        // 初始化状态变量
        isMenuActive = false;
        isMonitorActive = false;

        // 初始化灯光模式
        isLightMode1 = initialLightMode;
        Debug.Log($"灯光控制初始化为模式: {(isLightMode1 ? "模式1(按L开灯)" : "模式2(按L关灯)")}");

        // 设置默认的RenderTexture
        if (cameraRawImage != null && renderTextures != null && renderTextures.Length > 0)
        {
            cameraRawImage.texture = renderTextures[0];
        }
    }

    /// <summary>
    /// 处理所有按键输入
    /// </summary>
    private void HandleInput()
    {
        // M键：切换菜单
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMenu();
        }

        // Y键：打开监控（仅在菜单激活时有效）
        if (Input.GetKeyDown(KeyCode.Y) && isMenuActive && !isMonitorActive)
        {
            OpenMonitor();
        }

        // U键：激活呼叫器Call模式（仅在菜单激活时有效）
        if (Input.GetKeyDown(KeyCode.U) && isMenuActive && !isMonitorActive)
        {
            ActivateCallDevicesInCallMode();
        }

        // I键：激活呼叫器Alarm模式（仅在菜单激活时有效）或切换监控画面（仅在监控激活时有效）
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (isMenuActive && !isMonitorActive)
            {
                ActivateCallDevicesInAlarmMode();
            }
            else if (isMonitorActive)
            {
                SwitchCameraView();
            }
        }

        // L键：控制灯光（仅在菜单激活时有效）
        if (Input.GetKeyDown(KeyCode.L) && isMenuActive)
        {
            ControlLights();
            // 控制灯光后立即关闭菜单
            CloseMenuAfterLightControl();
        }

        // Q键：关闭监控（仅在监控激活时有效）
        if (Input.GetKeyDown(KeyCode.Q) && isMonitorActive)
        {
            CloseMonitor();
        }

        // 新增：F键切换text4-text7
        if (Input.GetKeyDown(KeyCode.F))
        {
            CycleText4ToText7();
        }
    }

    /// <summary>
    /// 新增：循环切换text4到text7
    /// </summary>
    private void CycleText4ToText7()
    {
        // 隐藏当前显示的文本
        switch (currentTextIndex)
        {
            case 0:
                if (text4 != null) text4.gameObject.SetActive(false);
                break;
            case 1:
                if (text5 != null) text5.gameObject.SetActive(false);
                break;
            case 2:
                if (text6 != null) text6.gameObject.SetActive(false);
                break;
            case 3:
                if (text7 != null) text7.gameObject.SetActive(false);
                break;
        }

        // 计算下一个索引（循环）
        currentTextIndex = (currentTextIndex + 1) % 4;

        // 显示下一个文本
        switch (currentTextIndex)
        {
            case 0:
                if (text4 != null) text4.gameObject.SetActive(true);
                Debug.Log("显示文本4");
                break;
            case 1:
                if (text5 != null) text5.gameObject.SetActive(true);
                Debug.Log("显示文本5");
                break;
            case 2:
                if (text6 != null) text6.gameObject.SetActive(true);
                Debug.Log("显示文本6");
                break;
            case 3:
                if (text7 != null) text7.gameObject.SetActive(true);
                Debug.Log("显示文本7");
                break;
        }
    }

    /// <summary>
    /// 切换菜单显示/隐藏
    /// </summary>
    private void ToggleMenu()
    {
        isMenuActive = !isMenuActive;

        if (menuImage1 != null)
        {
            menuImage1.SetActive(isMenuActive);

            // 激活/禁用所有子部件
            foreach (Transform child in menuImage1.transform)
            {
                child.gameObject.SetActive(isMenuActive);
            }
        }

        // 更新文本显示
        if (text1 != null) text1.gameObject.SetActive(!isMenuActive);
        if (text2 != null) text2.gameObject.SetActive(isMenuActive);

        // 如果打开了菜单，显示当前灯光模式
        if (isMenuActive)
        {
            string lightModeText = isLightMode1 ? "模式1(按L开灯)" : "模式2(按L关灯)";
            string lightStateText = lightManager != null ?
                (lightManager.AreAllLightsOn() ? "灯光: 开启" : "灯光: 关闭") : "灯光: 未连接";
            Debug.Log($"菜单已打开。{lightModeText}, {lightStateText}");
        }

        // 如果打开了菜单，确保监控关闭
        if (isMenuActive && isMonitorActive)
        {
            CloseMonitor();
        }
    }

    /// <summary>
    /// 控制灯光
    /// </summary>
    private void ControlLights()
    {
        if (lightManager == null)
        {
            Debug.LogWarning("LightManager 未设置！无法控制灯光。");
            return;
        }

        if (isLightMode1)
        {
            // 模式1：打开所有灯光，切换到模式2
            lightManager.TurnOnAllLights();
            isLightMode1 = false;
            Debug.Log("L键被按下：打开所有灯光，切换到模式2(按L关灯)");
        }
        else
        {
            // 模式2：关闭所有灯光，切换到模式1
            lightManager.TurnOffAllLights();
            isLightMode1 = true;
            Debug.Log("L键被按下：关闭所有灯光，切换到模式1(按L开灯)");
        }

        // 显示当前灯光状态
        ShowLightStatus();
    }

    /// <summary>
    /// 控制灯光后关闭菜单
    /// </summary>
    private void CloseMenuAfterLightControl()
    {
        if (isMenuActive)
        {
            isMenuActive = false;

            // 关闭菜单Image1及其所有子部件
            if (menuImage1 != null)
            {
                menuImage1.SetActive(false);

                // 禁用所有子部件
                foreach (Transform child in menuImage1.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }

            // 更新文本显示：显示text1，隐藏text2
            if (text1 != null) text1.gameObject.SetActive(true);
            if (text2 != null) text2.gameObject.SetActive(false);

            Debug.Log("L键灯光控制完成，菜单已关闭，显示文本1");
        }
    }

    /// <summary>
    /// 显示当前灯光状态
    /// </summary>
    private void ShowLightStatus()
    {
        if (lightManager != null)
        {
            int lightCount = lightManager.GetLightCount();
            bool lightsOn = lightManager.AreAllLightsOn();
            string mode = isLightMode1 ? "模式1(按L开灯)" : "模式2(按L关灯)";

            Debug.Log($"当前{mode}, 管理{lightCount}个光源, 状态: {(lightsOn ? "开启" : "关闭")}");
        }
    }

    /// <summary>
    /// 打开监控画面
    /// </summary>
    private void OpenMonitor()
    {
        if (isMenuActive)
        {
            // 关闭菜单
            isMenuActive = false;
            if (menuImage1 != null) menuImage1.SetActive(false);

            // 隐藏文本2
            if (text2 != null) text2.gameObject.SetActive(false);
        }

        // 打开监控
        isMonitorActive = true;
        if (monitorImage2 != null)
        {
            monitorImage2.SetActive(true);

            // 激活所有子部件
            foreach (Transform child in monitorImage2.transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        // 显示文本1和文本3
        if (text1 != null) text1.gameObject.SetActive(true);
        if (text3 != null) text3.gameObject.SetActive(true);

        Debug.Log("监控画面已打开，显示文本1和文本3");
    }

    /// <summary>
    /// 关闭监控画面
    /// </summary>
    private void CloseMonitor()
    {
        isMonitorActive = false;

        // 关闭监控画面
        if (monitorImage2 != null) monitorImage2.SetActive(false);

        // 隐藏文本3
        if (text3 != null) text3.gameObject.SetActive(false);

        // 显示文本1
        if (text1 != null) text1.gameObject.SetActive(true);

        Debug.Log("监控画面已关闭，显示文本1");
    }

    /// <summary>
    /// 激活所有关联的呼叫器（Call模式）
    /// </summary>
    private void ActivateCallDevicesInCallMode()
    {
        Debug.Log("Canvas U键被按下，激活呼叫器[Call模式]");

        // 激活所有关联的呼叫器
        if (callDevices.Count == 0)
        {
            Debug.LogWarning("Canvas没有设置任何呼叫器");
            return;
        }

        int activatedCount = 0;
        foreach (CallDevice callDevice in callDevices)
        {
            if (callDevice != null)
            {
                callDevice.ActivateCall();  // 使用Call模式激活
                activatedCount++;
            }
        }

        Debug.Log($"成功激活 {activatedCount}/{callDevices.Count} 个呼叫器[Call模式]");

        // 关闭菜单并显示文本1
        CloseMenuAfterActivation();
    }

    /// <summary>
    /// 激活所有关联的呼叫器（Alarm模式）
    /// </summary>
    private void ActivateCallDevicesInAlarmMode()
    {
        Debug.Log("Canvas I键被按下（菜单打开时），激活呼叫器[Alarm模式]");

        // 激活所有关联的呼叫器
        if (callDevices.Count == 0)
        {
            Debug.LogWarning("Canvas没有设置任何呼叫器");
            return;
        }

        int activatedCount = 0;
        foreach (CallDevice callDevice in callDevices)
        {
            if (callDevice != null)
            {
                callDevice.ActivateAlarm();  // 使用Alarm模式激活
                activatedCount++;
            }
        }

        Debug.Log($"成功激活 {activatedCount}/{callDevices.Count} 个呼叫器[Alarm模式]");

        // 关闭菜单并显示文本1
        CloseMenuAfterActivation();
    }

    /// <summary>
    /// 激活后关闭菜单
    /// </summary>
    private void CloseMenuAfterActivation()
    {
        if (isMenuActive)
        {
            isMenuActive = false;

            // 关闭菜单
            if (menuImage1 != null) menuImage1.SetActive(false);

            // 隐藏文本2
            if (text2 != null) text2.gameObject.SetActive(false);

            // 显示文本1
            if (text1 != null) text1.gameObject.SetActive(true);

            Debug.Log("菜单已关闭，显示文本1");
        }
    }

    /// <summary>
    /// 切换监控摄像头视图
    /// </summary>
    private void SwitchCameraView()
    {
        if (renderTextures == null || renderTextures.Length == 0 || cameraRawImage == null)
        {
            Debug.LogWarning("RenderTextures 或 RawImage 未设置！");
            return;
        }

        // 循环切换RenderTexture
        currentCameraIndex = (currentCameraIndex + 1) % renderTextures.Length;

        // 确保当前索引的RenderTexture不为空
        if (renderTextures[currentCameraIndex] != null)
        {
            cameraRawImage.texture = renderTextures[currentCameraIndex];
        }
        else
        {
            Debug.LogWarning($"RenderTexture索引{currentCameraIndex}为空！");
        }

        Debug.Log($"切换到摄像头 {currentCameraIndex + 1}/{renderTextures.Length}");
    }

    /// <summary>
    /// 获取当前显示的额外文本名称
    /// </summary>
    private string GetCurrentExtraTextName()
    {
        return currentTextIndex switch
        {
            0 => "text4",
            1 => "text5",
            2 => "text6",
            3 => "text7",
            _ => "unknown"
        };
    }

    /// <summary>
    /// 添加呼叫器到列表
    /// </summary>
    public void AddCallDevice(CallDevice device)
    {
        if (!callDevices.Contains(device))
        {
            callDevices.Add(device);
            Debug.Log($"已将呼叫器 {device.gameObject.name} 添加到Canvas {gameObject.name}");
        }
    }

    /// <summary>
    /// 从列表中移除呼叫器
    /// </summary>
    public void RemoveCallDevice(CallDevice device)
    {
        if (callDevices.Contains(device))
        {
            callDevices.Remove(device);
            Debug.Log($"已将呼叫器 {device.gameObject.name} 从Canvas {gameObject.name} 移除");
        }
    }

    /// <summary>
    /// 清除所有呼叫器
    /// </summary>
    public void ClearAllCallDevices()
    {
        callDevices.Clear();
        Debug.Log($"已清除Canvas {gameObject.name} 的所有呼叫器");
    }

    /// <summary>
    /// 设置新的RenderTexture数组（可选）
    /// </summary>
    public void SetRenderTextures(RenderTexture[] textures)
    {
        renderTextures = textures;
        currentCameraIndex = 0;

        if (cameraRawImage != null && textures != null && textures.Length > 0 && textures[0] != null)
        {
            cameraRawImage.texture = textures[0];
        }
    }

    /// <summary>
    /// 设置灯光管理器
    /// </summary>
    public void SetLightManager(LightManager manager)
    {
        lightManager = manager;
        if (lightManager != null)
        {
            Debug.Log($"已连接 LightManager，管理 {lightManager.GetLightCount()} 个光源");
        }
    }

    /// <summary>
    /// 切换灯光模式（外部调用）
    /// </summary>
    public void ToggleLightMode()
    {
        isLightMode1 = !isLightMode1;
        Debug.Log($"灯光模式切换为: {(isLightMode1 ? "模式1(按L开灯)" : "模式2(按L关灯)")}");
    }

    /// <summary>
    /// 设置灯光模式（外部调用）
    /// </summary>
    public void SetLightMode(bool mode1)
    {
        isLightMode1 = mode1;
        Debug.Log($"灯光模式设置为: {(isLightMode1 ? "模式1(按L开灯)" : "模式2(按L关灯)")}");
    }

    /// <summary>
    /// 获取当前状态（用于调试）
    /// </summary>
    public string GetCurrentState()
    {
        string state = $"菜单: {isMenuActive}, 监控: {isMonitorActive}, 当前摄像头: {currentCameraIndex + 1}/{renderTextures.Length}, 呼叫器数量: {callDevices.Count}";

        // 添加灯光状态
        state += $"\n灯光模式: {(isLightMode1 ? "模式1(按L开灯)" : "模式2(按L关灯)")}";

        if (lightManager != null)
        {
            state += $", 光源状态: {(lightManager.AreAllLightsOn() ? "开启" : "关闭")} ({lightManager.GetLightCount()}个光源)";
        }
        else
        {
            state += ", 光源状态: LightManager未连接";
        }

        // 添加文本状态
        state += $"\n文本状态: Text1({(text1 != null && text1.gameObject.activeSelf ? "显示" : "隐藏")}) ";
        state += $"Text2({(text2 != null && text2.gameObject.activeSelf ? "显示" : "隐藏")}) ";
        state += $"Text3({(text3 != null && text3.gameObject.activeSelf ? "显示" : "隐藏")})";

        // 新增：添加text4-text7状态
        state += $"\n额外文本状态: Text4({(text4 != null && text4.gameObject.activeSelf ? "显示" : "隐藏")}) ";
        state += $"Text5({(text5 != null && text5.gameObject.activeSelf ? "显示" : "隐藏")}) ";
        state += $"Text6({(text6 != null && text6.gameObject.activeSelf ? "显示" : "隐藏")}) ";
        state += $"Text7({(text7 != null && text7.gameObject.activeSelf ? "显示" : "隐藏")})";
        state += $"\n当前额外文本索引: {currentTextIndex} ({GetCurrentExtraTextName()})";

        return state;
    }

    /// <summary>
    /// 在Unity编辑器中显示连接到所有呼叫器的线
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 显示连接到所有呼叫器的线
        Gizmos.color = Color.yellow;
        foreach (CallDevice callDevice in callDevices)
        {
            if (callDevice != null)
            {
                Gizmos.DrawLine(transform.position, callDevice.transform.position);
            }
        }
    }
}