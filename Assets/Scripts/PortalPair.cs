using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPair : MonoBehaviour
{
    public Portal[] Portals { private set; get; }

    private void Awake()
    {
        this.Portals = this.GetComponentsInChildren<Portal>();

        if(this.Portals.Length != 2)
        {
            Debug.LogError("PortalPair children must contain exactly two Portal components in total.");
        }
    }
}
