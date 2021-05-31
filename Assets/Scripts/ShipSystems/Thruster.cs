using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour, IShipSystem
{
    public Ship Ship;
    float Thrust;
    float PowerUsage;
    int CpuUsage;
    private bool _active = true;

    public void SetActive(bool active)
    {
        if (active && !_active)
        {
            _active = true;
            Ship.Thrust += Thrust;
            Ship.PowerUsage += PowerUsage;
            Ship.CpuUsage += CpuUsage;
        }
        else if (_active)
        {
            _active = false;
            Ship.Thrust -= Thrust;
            Ship.PowerUsage -= PowerUsage;
            Ship.CpuUsage -= CpuUsage;
        }
    }

    public void SetThrusterType(int type)
    {
        switch (type)
        {
            case 0:
                Thrust = 100;
                CpuUsage = 3;
                PowerUsage = 6;
                break;
        }

        Ship.Thrust += Thrust;
        Ship.CpuUsage += CpuUsage;
        Ship.PowerUsage += PowerUsage;
        Ship.ShipSystems.Add(this);
    }

    private void OnDestroy()
    {
        if (_active)
        {
            Ship.Thrust -= Thrust;
            Ship.CpuUsage -= CpuUsage;
            Ship.PowerUsage -= PowerUsage;
        }
    }
}
