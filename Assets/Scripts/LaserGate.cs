using System.Collections;
using UnityEngine;

public class LaserGate : MonoBehaviour
{
    [SerializeField] Collider beamCollider;
    [SerializeField] Renderer beamRenderer;
    [SerializeField] float offSeconds = 1.5f;
    [SerializeField] float warnSeconds = 0.6f;
    [SerializeField] float onSeconds = 1.2f;
    [SerializeField] float startDelay = 0f;

    IEnumerator Start()
    {
        setBeam(false, false);
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);
        while (true)
        {
            setBeam(false, false);
            yield return new WaitForSeconds(offSeconds);

            float t = 0f;
            while (t < warnSeconds)
            {
                setBeam(Mathf.FloorToInt(t * 12f) % 2 == 0, false);
                t += Time.deltaTime;
                yield return null;
            }

            setBeam(true, true);
            yield return new WaitForSeconds(onSeconds);
        }
    }

    void setBeam(bool visible, bool dangerous)
    {
        beamRenderer.enabled = visible;
        beamCollider.enabled = dangerous;
    }
}
