using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit3D;
public class WaterBomb : GrenadierGrenade
{
   public  LayerMask explodeLayer;
    protected override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);

        if ((explodeLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (explosionTimer < 0)
            Explosion();
    }
}
