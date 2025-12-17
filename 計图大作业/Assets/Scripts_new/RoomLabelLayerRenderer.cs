using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RoomLabelLayerRenderer : MonoBehaviour
{
    public RoomDataManager roomManager;
    public GameObject roomLabelPrefab; // 3D TextMeshPro 预制体

    private List<GameObject> labels = new List<GameObject>();

    void Start()
    {
        GenerateLabels();
    }

    void GenerateLabels()
    {
        if (roomManager == null || roomLabelPrefab == null)
        {
            Debug.LogError("RoomLabelLayerRenderer: 缺少引用");
            return;
        }

        // 清理旧的
        foreach (var l in labels)
            Destroy(l);
        labels.Clear();

        foreach (var room in roomManager.GetAllRooms())
        {
            GameObject label = Instantiate(roomLabelPrefab, transform);

            // 放在房间中心稍微抬高
            label.transform.position =
                room.Bounds.center + Vector3.up * (room.Bounds.size.y + 0.3f);

            // 朝上（俯视）
            label.transform.rotation = Quaternion.Euler(90, 0, 0);

            var text = label.GetComponent<TextMeshPro>();
            text.text = room.RoomType.ToString();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 3;

            labels.Add(label);
        }
    }

    public void SetVisible(bool visible)
    {
        foreach (var l in labels)
            l.SetActive(visible);
    }
}
