using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInfo : MonoBehaviour
{
    public static MapInfo main;
    public readonly List<Ship> Ships = new List<Ship>();

    private void Awake()
    {
        main = this;
    }
}
