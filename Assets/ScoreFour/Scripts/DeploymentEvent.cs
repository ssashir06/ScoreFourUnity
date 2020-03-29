using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeploymentEvent : MonoBehaviour
{
    public int x = 0;
    public int y = 0;
    public DeploymentOrganizer deploymentOrganizer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var layerMask = ~gameObject.layer;

        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, float.MaxValue, layerMask)
            && hit.collider.gameObject == this.gameObject)
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.deploymentOrganizer.TryDeploy(this.x, this.y);
            }
        }
        
    }
}
