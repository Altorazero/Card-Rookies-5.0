using System.Collections.Generic;

public static class BuiltInBindings
{
    public static readonly BindingKey<IEntity> Caster = new("Caster");
    public static readonly BindingKey<IEntity> CurrentTarget = new("CurrentTarget");
    public static readonly BindingKey<IReadOnlyList<IEntity>> Targets = new("Targets");
    public static readonly BindingKey<IEntity> SelectedPoint = new("SelectedPoint");
    public static readonly BindingKey<IEntity> ExplosionCenter = new("ExplosionCenter");
    public static readonly BindingKey<IEntity> CurrentSpender = new("CurrentSpender");
    public static readonly BindingKey<IEnumerable<IEntity>> Spenders = new("Spenders");
}
