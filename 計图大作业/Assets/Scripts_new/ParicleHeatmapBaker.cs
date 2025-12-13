using UnityEngine;

public class ParticleHeatmapBaker : MonoBehaviour
{
    public ParticleSystem sourceParticles;

    [Header("Texture")]
    public int textureWidth = 256;
    public int textureHeight = 256;

    [Header("World Range (XZ)")]
    public Vector2 worldMin = new Vector2(-5, -7);
    public Vector2 worldMax = new Vector2(14, 9);

    [Header("Heat Settings")]
    public float baseIntensity = 10f;       // 每个粒子贡献强度
    public int influenceRadius = 100;        // 粒子扩散半径（像素）
    public float sigma = 2f;               // 高斯衰减
    public float decay = 0.99f;            // 每帧衰减

    Texture2D heatmapTexture;
    float[] heatValues;                    // 内部累积热力值
    Color[] pixels;

    void Start()
    {
        heatmapTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        heatmapTexture.wrapMode = TextureWrapMode.Clamp;

        heatValues = new float[textureWidth * textureHeight];
        pixels = new Color[textureWidth * textureHeight];

        // 初始化为全黑
        for (int i = 0; i < heatValues.Length; i++)
            heatValues[i] = 0f;

        // ⚠️ 这个脚本必须挂在一个有 Renderer 的 Quad 上
        GetComponent<Renderer>().material.mainTexture = heatmapTexture;
    }

    void Update()
    {
        DecayHeat();
        BakeParticles();
        ApplyTexture();
    }

    void DecayHeat()
    {
        // 每帧衰减，避免单点长期保持高值
        for (int i = 0; i < heatValues.Length; i++)
            heatValues[i] *= decay;
    }

    void BakeParticles()
    {
        if (sourceParticles == null) return;

        var particles = new ParticleSystem.Particle[sourceParticles.main.maxParticles];
        int count = sourceParticles.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            Vector3 p = particles[i].position;

            float u = Mathf.InverseLerp(worldMin.x, worldMax.x, p.x);
            float v = Mathf.InverseLerp(worldMin.y, worldMax.y, p.z);

            int cx = Mathf.RoundToInt(u * (textureWidth - 1));
            int cy = Mathf.RoundToInt(v * (textureHeight - 1));

            // 扩散影响
            for (int dx = -influenceRadius; dx <= influenceRadius; dx++)
            {
                for (int dy = -influenceRadius; dy <= influenceRadius; dy++)
                {
                    int x = cx + dx;
                    int y = cy + dy;

                    if (x < 0 || x >= textureWidth || y < 0 || y >= textureHeight)
                        continue;

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > influenceRadius)
                        continue;

                    float weight = Mathf.Exp(- (dist * dist) / (2f * sigma * sigma));
                    int idx = y * textureWidth + x;
                    heatValues[idx] += baseIntensity * weight * Time.deltaTime;
                }
            }
        }
    }

    void ApplyTexture()
    {
        // 找到当前最大值，用于归一化
        // float maxHeat = 0f;
        // for (int i = 0; i < heatValues.Length; i++)
        //     if (heatValues[i] > maxHeat) maxHeat = heatValues[i];

        // maxHeat = Mathf.Max(maxHeat, 0.0001f); // 防止除零
        float maxHeat = 5f; // 固定最大值，避免颜色跳动过大
        // 生成颜色
        for (int i = 0; i < pixels.Length; i++)
        {
            float normalized = Mathf.Clamp01(Mathf.Pow(heatValues[i] / maxHeat, 0.6f));

            // 蓝-绿-红渐变
            Color c;
            if (normalized < 0.5f)
                c = Color.Lerp(Color.blue, Color.green, normalized * 2f);
            else
                c = Color.Lerp(Color.green, Color.red, (normalized - 0.5f) * 2f);

            c.a = 0.75f;
            pixels[i] = c;
        }

        heatmapTexture.SetPixels(pixels);
        heatmapTexture.Apply();
    }
}
