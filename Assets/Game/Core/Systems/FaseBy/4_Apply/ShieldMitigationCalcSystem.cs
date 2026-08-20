// Modify: вычисляем итоговый Amount, ничего не мутируем
using System;



// Apply: сначала списываем щит...
public sealed class ShieldConsumeApplySystem : IEventListener<DamageEvent, IApplyPhaseEvent>
{
    public GEID SystemId { get; } = GEID.New;
    public int Priority => 0;

    public void OnEvent(EventContext context, DamageEvent evt)
    {
        int absorbed = evt.Scratch.GetOrDefault(BuiltinScratchKeys.ShieldAbsorbed);
        if (absorbed <= 0) return;

        var target = evt.GetFirstSubject(Role.Target);
        var shieldBefore = target.GetComponent<ShieldComponent>();

        evt.Scratch.Set(BuiltinScratchKeys.ShieldValueBeforeApply, shieldBefore.Value);

        context.Mutate<ShieldComponent>(target.Id, s => s with { Value = s.Value - absorbed });
    }
}

// ...затем применяем то, что осталось, к здоровью
