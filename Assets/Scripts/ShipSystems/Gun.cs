using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour, IShipSystem
{
    public Ship Ship;
    GameObject Projectile;
    GameObject Recticle;
    float ProjectileSpeed;
    public float ProjectileLife;
    int projectileDamage;
    float RateOfFire;
    float RotationSpeed;
    float Inaccuracy;
    public int CpuUsage;
    public float PowerUsage;
    [SerializeField] float AimDirection = 0f;
    Vector3 Target;
    float cooldown = 0f;
    private bool _active = true;
    public bool HoldFire = true;

    public void SetGunType(int type)
    {
        switch (type)
        {
            case 0:
                RateOfFire = 60;
                RotationSpeed = 90;
                Inaccuracy = 3f;
                Projectile = Resources.Load("Prefabs/projectile") as GameObject;
                ProjectileSpeed = 10f;
                ProjectileLife = 3f;
                projectileDamage = 1;
                Recticle = Instantiate(Resources.Load("Prefabs/Recticle")) as GameObject;
                CpuUsage = 10;
                PowerUsage = 2.5f;
                break;
        }
        Ship.CpuUsage += CpuUsage;
        Ship.PowerUsage += PowerUsage;
        Ship.ShipSystems.Add(this);
    }

    public void SetActive(bool active)
    {
        if (active && !_active)
        {
            _active = true;
            Ship.CpuUsage += CpuUsage;
            Ship.PowerUsage += PowerUsage;
        }
        else if (_active)
        {
            _active = false;
            Ship.CpuUsage -= CpuUsage;
            Ship.PowerUsage -= PowerUsage;
        }
    }

    public void SetTarget(Vector3 target)
    {
        Target = target;
    }


    public Vector4 CalculateAimAssist(GameObject aimAssistTarget) //returns the position at which the gun should fire, and the time it will take the projectile to get there.
    {
        Vector2 targetVel = aimAssistTarget.GetComponent<Rigidbody2D>().velocity;
        Vector2 targetToShip = Ship.gameObject.transform.position - aimAssistTarget.transform.position;
        Vector4 ret;
        if(targetVel.magnitude == 0)
        {
            ret = aimAssistTarget.transform.position;
            ret.w = targetToShip.magnitude / ProjectileSpeed;
            return ret;
        }
        float shipAngle = Vector2.Angle(targetToShip, targetVel);
        float aimAngle = Mathf.Rad2Deg * Mathf.Asin(targetVel.magnitude * Mathf.Sin(Mathf.Deg2Rad * shipAngle) / ProjectileSpeed);
        float timeToRendezvous = targetToShip.magnitude / (ProjectileSpeed * Mathf.Sin(Mathf.Deg2Rad * (180 - shipAngle - aimAngle)) / Mathf.Sin(Mathf.Deg2Rad * shipAngle));
        ret = aimAssistTarget.transform.position + ((Vector3)targetVel * timeToRendezvous);
        ret.w = timeToRendezvous;
        return ret;
    }

    private void Start()
    {
        gameObject.GetComponent<ShipPixel>().Ship.Guns.Add(this);
        Recticle.GetComponent<SpriteRenderer>().color = gameObject.GetComponent<SpriteRenderer>().color;
        if (!gameObject.GetComponent<ShipPixel>().Ship.Controllable) { Destroy(Recticle); }
    }

    private void Fire()
    {
        if (cooldown <= 0 && _active && !HoldFire)
        {
            //fire
            cooldown = 60 / RateOfFire;
            var projectile = Instantiate(Projectile, transform.position, Quaternion.Euler(0,0,AimDirection+Random.Range(-Inaccuracy, Inaccuracy)));
            projectile.GetComponent<Rigidbody2D>().velocity = ProjectileSpeed * projectile.transform.up;
            projectile.GetComponent<Projectile>().Damage = projectileDamage;
            Destroy(projectile, ProjectileLife);
            foreach(var pixel in gameObject.GetComponent<ShipPixel>().Ship.Grid)
            {
                if (pixel == null) { continue; }
                Physics2D.IgnoreCollision(projectile.GetComponent<BoxCollider2D>(), pixel.GetComponent<BoxCollider2D>());
            }
        }
    }

    private void Aim()
    {
        if (Target == null || !_active) { return; }
        float targetAngle = Quaternion.FromToRotation(Vector3.up, Target - transform.position).eulerAngles.z;
        float deltaAngle = Mathf.DeltaAngle(AimDirection, targetAngle);
        if (Mathf.Abs(deltaAngle) < RotationSpeed * Time.deltaTime)
        {
            AimDirection = AimDirection + deltaAngle;
            Fire();
        }
        else if (deltaAngle < 0) { AimDirection -= RotationSpeed * Time.deltaTime; }
        else if (deltaAngle > 0) { AimDirection += RotationSpeed * Time.deltaTime; }

        AimDirection = AimDirection % 360;
        if (AimDirection < 0) { AimDirection += 360; }

        if (gameObject.GetComponent<ShipPixel>().Ship.Controllable)
        {
            Recticle.transform.position = Vector3.Lerp(Recticle.transform.position, transform.position + Quaternion.Euler(0, 0, AimDirection) * ((Target - transform.position).magnitude * Vector3.up), 5f * Time.deltaTime);
        }
    }
    private void Update()
    {
        Aim();
        if (cooldown >= 0) { cooldown -= Time.deltaTime; }
    }

    private void OnDestroy()
    {
        if (_active)
        {
            Ship.CpuUsage -= CpuUsage;
            Ship.PowerUsage -= PowerUsage;
        }
        if (Recticle != null)
        {
            Destroy(Recticle);
        }
    }
}
