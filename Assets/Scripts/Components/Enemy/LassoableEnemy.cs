using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LassoableEnemy : LassoObject
{
    bool isDestroyed = false;
    public bool thrown = false;

    private void OnCollisionEnter(Collision collision)
    {
        // TODO: Handle orange, blueberry, strawberry enemies and bosses, and blender
        if (thrown && !isDestroyed)
        {
            if (collision.collider.CompareTag("Boss"))
            {
                // Will fix to handle more bosses (for orange, handle weak spots too)
                if (collision.transform.name == "Orange Boss" || collision.transform.parent.parent.name == "Orange Boss" || collision.transform.name.Contains("Peel")){
                    OrangeBoss boss = GameObject.Find("Orange Boss").GetComponent<OrangeBoss>();
                    if (collision.transform.name.Contains("Weak Spot"))
                    {
                        print("Weak Spot Damage");
                        ScreenShakeManager.Instance.ShakeCamera(6, 4, 1.5f);
                        boss.Damage(2);
                        DestroySelf(); // TODO: There's a bug where sometimes you hit multiple collision. Need a better(?) way to fix. Test isDestroyed
                    } 
                    else
                    {
                        print("Normal Damage");
                        ScreenShakeManager.Instance.ShakeCamera(2, 1, 0.1f);
                        boss.Damage(1);
                        DestroySelf();
                    }
                }
            }
            else
            {
                DestroySelf();
            }
        }
    }

    void DestroySelf()
    {
        isDestroyed = true;
        Destroy(gameObject);
        SoundManager.Instance().PlaySFX("EnemySplat");
    }
}
