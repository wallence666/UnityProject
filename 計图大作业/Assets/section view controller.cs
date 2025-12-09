using UnityEngine;

public class SectionViewController : MonoBehaviour
{
    public Camera fpsCam;          // 玩家FPS摄像机
    public Camera sectionCam;      // 新建的剖面图摄像机

    bool sectionMode = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleSection();
        }
    }

    void ToggleSection()
    {
        sectionMode = !sectionMode;
        Debug.Log("Section mode = " + sectionMode);

        if (sectionMode)
        {
            fpsCam.enabled = false;
            sectionCam.enabled = true;
        }
        else
        {
            sectionCam.enabled = false;
            fpsCam.enabled = true;
        }
    }
}
