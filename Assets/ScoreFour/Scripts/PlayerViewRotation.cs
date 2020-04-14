using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewRotation : MonoBehaviour
{
    public GameObject rotationObject;
    public UnityEngine.UI.Slider rotationHorizonal;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var euler = rotationObject.transform.transform.rotation.eulerAngles;
        rotationObject.transform.transform.rotation = Quaternion.Euler(
            euler.x,
            rotationHorizonal.value,
            euler.z
            );

    }
}
