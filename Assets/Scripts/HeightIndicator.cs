using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightIndicator : ManagedMonobehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GetAllRefs();
    }

    // Update is called once per frame
    void Update()
    {
        if(MPlayer().PlayerInWorld())
        {
            float h = MPlayer().GetVerticalPosition();
            h = MPlayer().ApplyHeightCoefficent(h);
            h /= MPlayer().GetMaxHeight();
            h = Mathf.Clamp01(h);

            transform.localScale = new Vector3(1, h, 1);
        }
    }
}
