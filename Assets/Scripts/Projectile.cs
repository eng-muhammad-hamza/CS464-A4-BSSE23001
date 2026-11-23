using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [SerializeField] float knockback = 5f;
    [SerializeField] float knockTime = 0.2f;
    [SerializeField] float lifetimeSeconds = 4f;
    Vector3 direction;
    float speed;
    float age;
    bool launched;

    public void launch(Vector3 aimDirection, float bulletSpeed)
    {
        direction = aimDirection.normalized;
        speed = bulletSpeed;
        launched = true;
    }

    void Update()
    {
        if (!launched) return;
        age += Time.deltaTime;
        transform.position += direction * (speed * Time.deltaTime);
        if (age >= lifetimeSeconds) pop();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!launched) return;
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out Health health) && health.takeDamage(damage))
            {
                Vector3 flat = new Vector3(direction.x, 0f, direction.z).normalized;
                if (other.TryGetComponent(out PlayerController controller)) controller.knockback(flat * knockback, knockTime);
            }
            pop();
            return;
        }
        if (other.isTrigger || other.GetComponentInParent<Turret>() != null) return;
        pop();
    }

    void pop()
    {
        JuiceFx.burst(transform.position, JuiceFx.damageColor, 8, 3f, 0.1f, 0.3f);
        Destroy(gameObject);
    }
}
