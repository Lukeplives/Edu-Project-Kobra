using UnityEngine;

public class AiTank : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject bulletSpawn;
    public GameObject enemy;

    public Transform cannon;
    public float rotationSpeed = 2.0f;
    public float speed = 15.0f;
    public float moveSpeed = 3f;

    public float fireRate = 1f;
    private float nextFireTime;

    public float detectionRange = 20f;

    void CreateBullet()
    {
        GameObject shell = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        shell.GetComponent<Rigidbody>().linearVelocity = speed * cannon.forward;
    }

    float? CalculateAngle(bool low)
    {
        float angle = 0.0f;
        Vector3 targetDir = enemy.transform.position - transform.position;
        float y = targetDir.y;
        targetDir.y = 0;
        float x = targetDir.magnitude - 1;
        float gravity = 9.81f;
        float sSqr = speed * speed;
        float underTheRoot = sSqr * sSqr - gravity * (gravity * x * x + 2 *y * sSqr);

        if(underTheRoot >= 0)
        {
            float root = Mathf.Sqrt(underTheRoot);
            float highAngle = sSqr + root;
            float lowAngle = sSqr - root;

            if(low)
            {
                angle = Mathf.Atan2(lowAngle, gravity*x) * Mathf.Rad2Deg;
                return angle;
            }
            else
            {
                angle = Mathf.Atan2(highAngle, gravity * x) * Mathf.Rad2Deg;
                return angle;
            }
        }
        else
            return null;
    }

    float? RotateCannon()
    {
        float? angle = CalculateAngle(true);
        if(angle != null)
        {
            cannon.localEulerAngles = new Vector3(360f - (float)angle,0f,0f);
        }
        return angle;
    }

    void Update()
    {
        if(enemy == null)
            return;
        float distance = Vector3.Distance(transform.position, enemy.transform.position);

        if (distance > detectionRange)
            return;

        float? angle = RotateCannon();

        if (angle != null && Time.time >= nextFireTime)
        {
            CreateBullet();
            nextFireTime = Time.time + fireRate;
        }
    }
}
