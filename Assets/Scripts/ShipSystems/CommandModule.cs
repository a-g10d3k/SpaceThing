using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandModule : MonoBehaviour, IShipSystem
{
    public Ship Ship;
    public int Cpu;
    public float PowerUsage;
    private bool _active = true;

    public void SetCommandModuleType(int type)
    {
        switch (type)
        {
            case 0:
                Cpu = 100;
                PowerUsage = 3;
                break;
        }
        Ship.MaxCpu += Cpu;
        Ship.PowerUsage += PowerUsage;
        Ship.ShipSystems.Add(this);
    }

    public void SetActive(bool active)
    {
        if (active && !_active)
        {
            _active = true;
            Ship.MaxCpu += Cpu;
            Ship.PowerUsage += PowerUsage;
        }
        else if (_active)
        {
            _active = false;
            Ship.MaxCpu -= Cpu;
            Ship.PowerUsage -= PowerUsage;
        }
    }

    public void OnDestroy()
    {
        if (_active)
        {
            Ship.MaxCpu -= Cpu;
            Ship.PowerUsage -= PowerUsage;
            Ship.OnDestroyCommandModule();
        }
    }
}
