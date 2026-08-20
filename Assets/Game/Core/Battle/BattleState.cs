using System.Collections.Generic;
using UnityEngine;

public sealed class BattleState
{
    // === Core registries ===

    private readonly Dictionary<GEID, IEntity> _entities = new();

    public IReadOnlyDictionary<GEID, IEntity> Entities => _entities;

    public BattleRng Rng { get; }
    public BattleState(int rngSeed)
    {
        Rng = new BattleRng(rngSeed);
    }

    public IEntity GetEntity(GEID targetId)
    {
        if (_entities.TryGetValue(targetId, out var entity))
        {
            return entity;
        }
        Debug.LogWarning($"Entity with ID {targetId} not found.");
        return null;

    }
    public void AddEntity(IEntity entity)
    {
        if (!_entities.ContainsKey(entity.Id))
        {
            _entities[entity.Id] = entity;
        }
        else
        {
            Debug.LogWarning($"Entity with ID {entity.Id} already exists in BattleState.");
        }
    }
    public IEntity GetTileAtHex(HexCoordinates hex)
    {
        foreach (var entity in _entities.Values)
        {
            var hexComp = entity.GetComponent<HexComponent>();
            if (hexComp != null && hexComp.Coordinates.Equals(hex) && entity.GetComponent<TileComponent>() != null)
            {
                return entity;
            }
        }
        return null;
    }

    public IEntity GetOccupantAtHex(HexCoordinates hex)
    {
        foreach (var entity in _entities.Values)
        {
            var hexComp = entity.GetComponent<HexComponent>();
            if (hexComp != null && hexComp.Coordinates.Equals(hex) && entity.GetComponent<TileComponent>() == null)
            {
                return entity;
            }
        }
        return null;
    }

    public IEntity GetEntityAtHex(HexCoordinates hex)
    {
        foreach (var entity in _entities.Values)
        {
            var hexComp = entity.GetComponent<HexComponent>();
            if (hexComp != null && hexComp.Coordinates.Equals(hex))
            {
                return entity;
            }
        }
        return null;
    }

    public void RemoveEntity(GEID entityId)
    {
        if (_entities.ContainsKey(entityId))
        {
            _entities.Remove(entityId);
        }
        else
        {
            Debug.LogWarning($"Entity with ID {entityId} does not exist in BattleState.");
        }
    }

    /// <summary>
    /// Создаёт глубокую копию состояния (клонирует все сущности и их компоненты).
    /// Примечание: RNG воссоздаётся с тем же начальным seed, а не с текущей позицией последовательности.
    /// Это допустимо для снимков с целью аудита/отладки, но не подходит для детерминированного воспроизведения.
    /// </summary>
    public BattleState Clone()
    {
        var clone = new BattleState(Rng.Seed);
        foreach (var kvp in _entities)
        {
            if (kvp.Value is BaseEntity baseEntity)
                clone._entities[kvp.Key] = baseEntity.Clone();
            else
                clone._entities[kvp.Key] = kvp.Value;
        }
        return clone;
    }
}
