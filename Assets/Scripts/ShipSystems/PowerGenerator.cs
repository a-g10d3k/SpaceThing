using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerGenerator : MonoBehaviour, IShipSystem
{
    public Ship Ship;
    public int CpuUsage;
    public float MaxPower;
    private bool _active = true;

    public void SetPowerGeneratorType(int type)
    {
        switch (type)
        {
            case 0:
                CpuUsage = 15;
                MaxPower = 20;
                break;
        }
        Ship.CpuUsage += CpuUsage;
        Ship.PowerGeneration += MaxPower;
        Ship.ShipSystems.Add(this);
    }

    public void SetActive(bool active)
    {
        if (active && !_active)
        {
            _active = true;
            Ship.CpuUsage += CpuUsage;
            Ship.PowerGeneration += MaxPower;
        }
        else if (_active)
        {
            _active = false;
            Ship.CpuUsage -= CpuUsage;
            Ship.PowerGeneration -= MaxPower;
        }
    }

    private void OnDestroy()
    {
        if (_active)
        {
            Ship.CpuUsage -= CpuUsage;
            Ship.PowerGeneration -= MaxPower;
        }
    }
}
