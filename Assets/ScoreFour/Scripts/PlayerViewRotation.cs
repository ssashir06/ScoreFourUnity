﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewRotation : MonoBehaviour
{
    public UnityEngine.UI.Slider rotationHorizonal;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(rotationHorizonal.value);
        var euler = this.transform.rotation.eulerAngles;
        this.transform.rotation = Quaternion.Euler(
            euler.x,
            rotationHorizonal.value,
            euler.z
            );

    }
}
