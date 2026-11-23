using UnityEngine;

[RequireComponent(typeof(AudioListener))]
public class ListenerOnPlayer : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 0.5f, 0f);

    void Start()
    {
        if (target == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null) target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        transform.SetPositionAndRotation(target.position + offset, Quaternion.identity);
    }
}
