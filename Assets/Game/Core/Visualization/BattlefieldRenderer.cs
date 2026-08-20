using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Реестр всех визуальных объектов. Отвечает за спавн, деспавн и хранение ссылок на EntityView.
/// </summary>
public class BattlefieldRenderer : MonoBehaviour
{
    [Header("Settings")]
    public float HexSize = 1f;
    public float ElevationStep = 0.5f;

    [Header("Prefabs")]
    public GameObject HexPrefab; 
    public GameObject UnitPrefab;

    [Header("Materials")]
    public Material GrassMat;
    public Material SandMat;
    public Material StoneMat;
    public Material WaterMat;
    public Material AllyMat;
    public Material EnemyMat;

    private readonly Dictionary<GEID, EntityView> _views = new Dictionary<GEID, EntityView>();
    private BattleState _state;

    public void Init(BattleState state)
    {
        _state = state;
    }

    public EntityView GetView(GEID id)
    {
        _views.TryGetValue(id, out var view);
        return view;
    }

    public Vector3 GetWorldPositionForHex(HexCoordinates hex)
    {
        Vector3 basePos = hex.ToWorld(HexSize);
        int elevation = 0;
        
        if (_state != null)
        {
            var tile = _state.GetTileAtHex(hex);
            if (tile != null)
            {
                var elevComp = tile.GetComponent<ElevationComponent>();
                if (elevComp != null) elevation = elevComp.Elevation;
            }
        }
        
        return basePos + new Vector3(0, elevation * ElevationStep, 0);
    }

    public void RenderInitialState(BattleState state)
    {
        ClearView();

        foreach (var kvp in state.Entities)
        {
            var entity = kvp.Value;
            var hexComp = entity.GetComponent<HexComponent>();
            if (hexComp == null) continue;

            Vector3 basePosition = hexComp.Coordinates.ToWorld(HexSize);

            // Отрисовка Тайлов
            var tileComp = entity.GetComponent<TileComponent>();
            if (tileComp != null)
            {
                var elevComp = entity.GetComponent<ElevationComponent>();
                int elevation = elevComp != null ? elevComp.Elevation : 0;
                
                Vector3 hexPos = basePosition + new Vector3(0, elevation * ElevationStep, 0);

                if (HexPrefab != null)
                {
                    var hexGo = Instantiate(HexPrefab, hexPos, Quaternion.identity, transform);
                    hexGo.name = $"Hex_{hexComp.Coordinates.Q}_{hexComp.Coordinates.R}";
                    
                    var view = hexGo.AddComponent<EntityView>();
                    view.Init(entity.Id);
                    _views[entity.Id] = view;

                    if (hexGo.GetComponent<Collider>() == null) hexGo.AddComponent<BoxCollider>();

                    var renderer = hexGo.GetComponentInChildren<Renderer>();
                    if (renderer != null) renderer.material = GetTerrainMaterial(tileComp.Terrain);
                }
                
                continue;
            }

            // Отрисовка Юнитов
            var teamComp = entity.GetComponent<TeamComponent>();
            if (teamComp != null)
            {
                int tileElevation = 0;
                var tileEntity = state.GetTileAtHex(hexComp.Coordinates);
                if (tileEntity != null)
                {
                    var tileElevComp = tileEntity.GetComponent<ElevationComponent>();
                    if (tileElevComp != null) tileElevation = tileElevComp.Elevation;
                }

                Vector3 unitPos = basePosition + new Vector3(0, tileElevation * ElevationStep + 0.5f, 0);

                if (UnitPrefab != null)
                {
                    var unitGo = Instantiate(UnitPrefab, unitPos, Quaternion.identity, transform);
                    unitGo.name = $"Unit_{entity.Id}";

                    var view = unitGo.AddComponent<EntityView>();
                    view.Init(entity.Id);
                    _views[entity.Id] = view;

                    if (unitGo.GetComponent<Collider>() == null) unitGo.AddComponent<BoxCollider>();

                    var renderer = unitGo.GetComponentInChildren<Renderer>();
                    if (renderer != null)
                    {
                        // Используем компонент-маркер PlayerControlledComponent вместо хардкодного GEID
                        renderer.material = entity.HasComponent<PlayerControlledComponent>() ? AllyMat : EnemyMat;
                    }
                }
            }
        }
    }

    private Material GetTerrainMaterial(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Grass: return GrassMat;
            case TerrainType.Sand: return SandMat;
            case TerrainType.Stone: return StoneMat;
            case TerrainType.Water: return WaterMat;
            default: return GrassMat;
        }
    }

    public void ClearView()
    {
        foreach (var view in _views.Values)
        {
            if (view != null) Destroy(view.gameObject);
        }
        _views.Clear();
    }
}
