// ===== 3.  оманда как элементарна€ обратима€ операци€ над BattleState =====
public interface IReversibleCommand
{
    void Execute(BattleState state);
    void Undo(BattleState state);
}

public sealed class SetComponentCommand<T> : IReversibleCommand where T : class, IComponent
{
    private readonly GEID _entityId;
    private readonly T _oldValue; // null, если компонента не было
    private readonly T _newValue;

    public SetComponentCommand(GEID entityId, T oldValue, T newValue)
    {
        _entityId = entityId;
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public void Execute(BattleState state) =>
        state.GetEntity(_entityId).AddComponent(_newValue);

    public void Undo(BattleState state)
    {
        var entity = state.GetEntity(_entityId);
        if (_oldValue != null) entity.AddComponent(_oldValue);
        else entity.RemoveComponent<T>();
    }
}

public sealed class AddEntityCommand : IReversibleCommand
{
    private readonly IEntity _entity;
    public AddEntityCommand(IEntity entity) => _entity = entity;
    public void Execute(BattleState state) => state.AddEntity(_entity);
    public void Undo(BattleState state) => state.RemoveEntity(_entity.Id);
}

public sealed class RemoveEntityCommand : IReversibleCommand
{
    private readonly IEntity _entity; // держим ссылку, чтобы вернуть при Undo
    public RemoveEntityCommand(IEntity entity) => _entity = entity;
    public void Execute(BattleState state) => state.RemoveEntity(_entity.Id);
    public void Undo(BattleState state) => state.AddEntity(_entity);
}