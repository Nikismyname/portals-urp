using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [SerializeField]
    private PortalPair portalPair;

    [SerializeField]
    private Image inPortalImg;

    [SerializeField]
    private Image outPortalImg;

    private void Start()
    {
        var portals = this.portalPair.Portals;

        this.inPortalImg.color = portals[0].PortalColour;
        this.outPortalImg.color = portals[1].PortalColour;

        this.inPortalImg.gameObject.SetActive(false);
        this.outPortalImg.gameObject.SetActive(false);
    }

    public void SetPortalPlaced(int portalID, bool isPlaced)
    {
        if(portalID == 0)
        {
            this.inPortalImg.gameObject.SetActive(isPlaced);
        }
        else
        {
            this.outPortalImg.gameObject.SetActive(isPlaced);
        }
    }
}
