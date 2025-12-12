using UnityEngine;

public class SectionViewController : MonoBehaviour
{
    public Camera playerCamera;                   // FPS 摄像机
    public Camera topDownCamera;                  // 俯视摄像机（正交）
    public TemperatureLayerRenderer tempLayer;    // 你的温度层控制
                                                  
    int mode = 0; // 0 = FPS, 1 = TopView, 2 = Temperature

    Vector3 fpsPos;
    Quaternion fpsRot;

    void Start()
    {
        // 保存初始 FPS 摄像机位置
        fpsPos = playerCamera.transform.localPosition;
        fpsRot = playerCamera.transform.localRotation;

        // 默认只开 FPS，关掉其他
        playerCamera.enabled = true;
        topDownCamera.enabled = false;
        tempLayer.SetVisible(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            SwitchMode();
        }
    }

    void SwitchMode()
    {
        mode = (mode + 1) % 3;

        Debug.Log("切换模式 = " + mode);

        if (mode == 0)
        {
            // =====================
            // 回到 FPS 第一人称视角
            // =====================
            playerCamera.enabled = true;
            topDownCamera.enabled = false;
            tempLayer.SetVisible(false);

            // 恢复相机位置（避免卡进模型）
            playerCamera.transform.localPosition = fpsPos;
            playerCamera.transform.localRotation = fpsRot;
        }
        else if (mode == 1)
        {
            // =====================
            // 俯视正交视角（剖面图）
            // =====================
            playerCamera.enabled = false;
            topDownCamera.enabled = true;
            tempLayer.SetVisible(false);
        }
        else if (mode == 2)
        {
            // =====================
            // 温度可视化层（平面图）
            // =====================
            playerCamera.enabled = false;
            topDownCamera.enabled = true;
            tempLayer.SetVisible(true);
        }
    }
}
