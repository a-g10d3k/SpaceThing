using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPixel : MonoBehaviour
{
    public Vector2Int GridPosition;
    public Ship Ship;
    Color Color;
    public float Mass;
    public int HitPoints;

    void Start()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color;
    }

    public void SetPixelType(int type)
    {
        switch (type)
        {
            case 1:
                Color = Color.white;
                HitPoints = 6;
                Mass = 10;
                break;
            case 2:
                Color = new Color(1f, 0.5f, 0);
                HitPoints = 6;
                Mass = 20;
                Gun gun = gameObject.AddComponent<Gun>();
                gun.Ship = Ship;
                gun.SetGunType(0);
                break;
            case 3:
                Color = Color.blue;
                HitPoints = 3;
                Mass = 20;
                Thruster thruster = gameObject.AddComponent<Thruster>();
                thruster.Ship = Ship;
                thruster.SetThrusterType(0);
                break;
            case 4:
                Color = Color.magenta;
                HitPoints = 3;
                Mass = 25;
                CommandModule commandModule = gameObject.AddComponent<CommandModule>();
                commandModule.Ship = Ship;
                commandModule.SetCommandModuleType(0);
                break;
            case 5:
                Color = Color.green;
                HitPoints = 3;
                Mass = 20;
                PowerGenerator powerGenerator = gameObject.AddComponent<PowerGenerator>();
                powerGenerator.Ship = Ship;
                powerGenerator.SetPowerGeneratorType(0);
                break;
        }
    }

    private void TakeDamage(int damage, Collider2D collider)
    {
        HitPoints -= damage;
        if (HitPoints <= 0)
        {
            DestroyPixel(collider.gameObject.GetComponent<Rigidbody2D>().velocity * collider.GetComponent<Projectile>().Mass);
        }
    }

    public void DestroyPixel(Vector2 velocity)
    {
        gameObject.transform.SetParent(null);
        Ship.Mass -= Mass;
        Destroy(gameObject.GetComponent<BoxCollider2D>());
        var rb = gameObject.AddComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = (velocity/Mass) + Ship.GetComponent<Rigidbody2D>().velocity + new Vector2(Random.Range(-0.2f,0.2f), Random.Range(-0.2f, 0.2f));
            rb.angularVelocity = Random.Range(-360f, 360f);
        }
        if(gameObject.GetComponent<Gun>() != null)
        {
            Destroy(gameObject.GetComponent<Gun>());
        }
        if (gameObject.GetComponent<Thruster>() != null)
        {
            Destroy(gameObject.GetComponent<Thruster>());
        }
        if (gameObject.GetComponent<CommandModule>() != null)
        {
            Destroy(gameObject.GetComponent<CommandModule>());
        }
        if (gameObject.GetComponent<PowerGenerator>() != null)
        {
            Destroy(gameObject.GetComponent<PowerGenerator>());
        }
        Destroy(gameObject, 300f);
        Ship.Grid[GridPosition.x, GridPosition.y] = null;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == 10)
        {
            TakeDamage(collider.gameObject.GetComponent<Projectile>().Damage, collider);
            collider.gameObject.GetComponent<ParticleSystem>().Play();
            Destroy(collider.gameObject.GetComponent<SpriteRenderer>());
            Destroy(collider.gameObject, 1f);
            Destroy(collider);
        }
    }
}
