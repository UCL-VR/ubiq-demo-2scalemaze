using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_LOD_bias : MonoBehaviour
{
    public float LodBiasFactor = 1F;

    void Awake()
    {
        QualitySettings.lodBias = QualitySettings.lodBias * LodBiasFactor;
    }

}