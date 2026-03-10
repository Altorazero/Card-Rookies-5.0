using System.Collections.Generic;
using UnityEngine;

public sealed class BattleState
{
    // === Core registries ===

    private readonly Dictionary<Geid, IEntity> _entities = new();

    public IReadOnlyDictionary<Geid, IEntity> Entities => _entities;

    public BattleRng Rng { get; }

    public BattleState(int rngSeed)
    {
        Rng = new BattleRng(rngSeed);
    }

    public IEntity GetEntity(Geid targetId)
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
    public void RemoveEntity(Geid entityId)
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
}