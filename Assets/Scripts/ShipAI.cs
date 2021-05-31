using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAI : MonoBehaviour
{
    private enum BehaviourModes { None, SeekAndDestroy, GoToPosition }
    private enum MovementModes { Idle, Follow, GoToPosition }
    private enum GunModes { Idle, OnlyAim, FireOnTarget }

    private struct ShipInfo
    {
        public Ship Ship;
        public float Distance;
    }
    public Vector2 TargetPosition;
    public Ship Ship;
    public float SightRange;
    private GameObject _followTarget;
    private List<ShipInfo> _enemyShips;
    private GameObject _gunsTarget;
    private MovementModes _movementMode = MovementModes.Idle;
    private GunModes _gunMode = GunModes.Idle;
    public float FollowDistance = 0f;
    [SerializeField]private BehaviourModes _behaviourMode;

    private void Start()
    {
        Ship = gameObject.GetComponent<Ship>();
    }

    private List<ShipInfo> GetEnemyShipsWithinSight()//returns a list of ships within the sight range, sorted by distance from the nearest to the farthest
    {
        var shipList = new List<ShipInfo>();
        foreach (Ship ship in MapInfo.main.Ships)
        {
            if (ship == null) { continue; }
            float distance;
            if (!ship.Dead && ship.Team != Ship.Team && ship.gameObject != gameObject && (distance = (ship.transform.position - transform.position).magnitude) <= SightRange)
            {
                var shipInfo = new ShipInfo();
                shipInfo.Ship = ship;
                shipInfo.Distance = distance;
                shipList.Add(shipInfo);
            }
        }
        shipList.Sort((x, y) => x.Distance.CompareTo(y.Distance));
        return shipList;
    }

    public void Follow(GameObject target)
    {
        _movementMode = MovementModes.Follow;
        _followTarget = target;
    }

    private void FollowUpdate()
    {
        if (_followTarget != null)
        {
            if ((transform.position - _followTarget.transform.position).magnitude > FollowDistance)
            {
                Ship.SetTargetPos(_followTarget.transform.position);
            }
            else { Ship.Stop = true; }
        }
        else { _movementMode = MovementModes.Idle; }
    }

    private void FireOnTargetUpdate()
    {
        Ship.AimAssistTarget = _gunsTarget;
    }

    private void DecideMovement()
    {
        switch (_behaviourMode)
        {
            case (BehaviourModes.None):
                _movementMode = MovementModes.Idle;
                break;
            case (BehaviourModes.SeekAndDestroy):
                if (_movementMode == MovementModes.Idle || _followTarget.GetComponent<Ship>().Dead)
                {
                    var ships = GetEnemyShipsWithinSight();
                    if (ships.Count > 0)
                    {
                        Follow(ships[0].Ship.gameObject);
                    }
                    else
                    {
                        _followTarget = null;
                        _movementMode = MovementModes.Idle;
                    }
                }
                break;
            case (BehaviourModes.GoToPosition):
                _movementMode = MovementModes.GoToPosition;
                break;
        }

    }

    private void Move()
    {
        switch (_movementMode)
        {
            case (MovementModes.Idle):
                gameObject.GetComponent<Ship>().Stop = true;
                break;
            case (MovementModes.Follow):
                FollowUpdate();
                break;
            case (MovementModes.GoToPosition):
                Ship.SetTargetPos(TargetPosition);
                break;
        }
    }

    private void DecideGuns()
    {
        switch (_behaviourMode)
        {
            case BehaviourModes.None:
                _gunMode = GunModes.Idle;
                break;
            case BehaviourModes.SeekAndDestroy:
                if (_gunMode == GunModes.Idle)
                {
                    _gunsTarget = _followTarget;
                    _gunMode = GunModes.FireOnTarget;
                }
                break;
            case BehaviourModes.GoToPosition:
                if (_gunMode == GunModes.Idle)
                {
                    _enemyShips = GetEnemyShipsWithinSight();
                    _gunMode = GunModes.FireOnTarget;
                }
                if (_gunMode == GunModes.FireOnTarget)
                {
                    _enemyShips = GetEnemyShipsWithinSight();
                    if (_enemyShips.Count > 0)
                    {
                        _gunsTarget = _enemyShips[0].Ship.gameObject;
                    }
                    else { _gunMode = GunModes.Idle; }
                }
                break;
        }
    }

    private void ControlGuns()
    {
        switch (_gunMode)
        {
            case GunModes.Idle:
                Ship.AimAssistTarget = null;
                break;
            case GunModes.FireOnTarget:
                FireOnTargetUpdate();
                break;
        }
    }

    void Update()
    {
        DecideMovement();
        Move();
        DecideGuns();
        ControlGuns();
    }
}
