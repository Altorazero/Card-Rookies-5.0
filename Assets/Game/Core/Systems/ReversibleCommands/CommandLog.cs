// ===== 4. Журнал команд с возможностью отката к отметке =====
using System;
using System.Collections.Generic;

public sealed class CommandLog
{
    private readonly BattleState _state;
    private readonly List<IReversibleCommand> _executed = new();

    public CommandLog(BattleState state) => _state = state;

    public int Checkpoint => _executed.Count;

    public void Apply(IReversibleCommand command)
    {
        command.Execute(_state);
        _executed.Add(command);
    }

    public void UndoTo(int checkpoint)
    {
        if (checkpoint > _executed.Count)
            throw new ArgumentOutOfRangeException(nameof(checkpoint));

        for (int i = _executed.Count - 1; i >= checkpoint; i--)
            _executed[i].Undo(_state);

        _executed.RemoveRange(checkpoint, _executed.Count - checkpoint);
    }

    // Точка невозврата — например, ход подтверждён и разыгран по-настоящему.
    public void Commit() => _executed.Clear();
}