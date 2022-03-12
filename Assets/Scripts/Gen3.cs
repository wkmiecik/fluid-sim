using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gen3 : MonoBehaviour
{
    Texture3D texture;

    void Start()
    {
        texture = CreateTexture3D(64);
        UnityEditor.AssetDatabase.CreateAsset(texture, "Assets/test/t3d.asset");
        Debug.Log(UnityEditor.AssetDatabase.GetAssetPath(texture));
    }

    Texture3D CreateTexture3D(int size)
    {
        Color[] colorArray = new Color[size * size * size];
        texture = new Texture3D(size, size, size, TextureFormat.RGB24, true);
        float r = 1 / (size - 1);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    colorArray[i + (j * size) + (k * size * size)] = new Color(i * r, j * r, k * r, 1);
                }
            }
        }
        texture.SetPixels(colorArray);
        texture.Apply();
        return texture;
    }
}