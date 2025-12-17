using UnityEngine;

public class test : MonoBehaviour 
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 150f;

    [Header("References")]
    [SerializeField] private Transform playerBody; // 將人物拖到這裡

    private float xAxisClamp = 0f;
    private bool m_cursorIsLocked = true;

    private void Awake()
    {
        ApplyCursorLock();
    }

    private void Update()
    {
        HandleCursorLock();
        CameraRotation();
    }

    private void HandleCursorLock()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) m_cursorIsLocked = false;
        else if (Input.GetMouseButtonUp(0)) m_cursorIsLocked = true;

        ApplyCursorLock();
    }

    private void ApplyCursorLock()
    {
        if (m_cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void CameraRotation()
    {
        if (!m_cursorIsLocked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 垂直旋轉 (限制範圍在正負 90 度之間)
        xAxisClamp -= mouseY;
        xAxisClamp = Mathf.Clamp(xAxisClamp, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xAxisClamp, 0f, 0f);

        // 水平旋轉 (直接作用在人物身上)
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}