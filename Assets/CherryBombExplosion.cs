using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryBombExplosion : MonoBehaviour
{

    private void Start()
    {
        StartCoroutine(SpawnExplosion());
    }

    IEnumerator SpawnExplosion()
    {
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.name);
        if (other.CompareTag("Boss"))
        {
            // Will fix to handle more bosses (for orange, handle weak spots too)
            if (other.transform.name == "Orange Boss" || other.transform.parent.parent.name == "Orange Boss" || other.transform.name.Contains("Peel"))
            {
                OrangeBoss boss = GameObject.Find("Orange Boss").GetComponent<OrangeBoss>();
                if (other.transform.name.Contains("Weak Spot"))
                {
                    print("Weak Spot Damage");
                    ScreenShakeManager.Instance.ShakeCamera(6, 4, 1.5f);
                    boss.Damage(2);
                }
                else
                {
                    print("Normal Damage");
                    ScreenShakeManager.Instance.ShakeCamera(2, 1, 0.1f);
                    boss.Damage(1);
                }
            }
        }
        else if (other.name.Contains("Body"))
        {
            Destroy(other.gameObject.transform.root.gameObject);
        }
        else if (other.CompareTag("Breakable"))
        {
            Destroy(other.gameObject);
        }
    }
}
