using System;
using System.Collections.Generic;

/// <summary>
/// Вспомогательные методы для работы с Subjects в событиях IHaveSubjects.
/// </summary>
public static class SubjectsHelper
{
    /// <summary>
    /// Создаёт пустой словарь субъектов.
    /// </summary>
    public static Dictionary<Role, List<IEntity>> Empty()
    {
        return new Dictionary<Role, List<IEntity>>();
    }

    /// <summary>
    /// Создаёт словарь субъектов с предзаполненными записями.
    /// </summary>
    public static Dictionary<Role, List<IEntity>> Create(params (Role role, IEntity entity)[] entries)
    {
        var subjects = Empty();
        foreach (var (role, entity) in entries)
        {
            if (!subjects.ContainsKey(role))
            {
                subjects[role] = new List<IEntity>();
            }
            subjects[role].Add(entity);
        }
        return subjects;
    }

    /// <summary>
    /// Возвращает список сущностей для указанной роли. Если нет — пустой список.
    /// </summary>
    public static IReadOnlyList<IEntity> GetSubjects(this IHaveSubjects evt, Role role)
    {
        if (evt.Subjects == null || !evt.Subjects.TryGetValue(role, out var list))
            return Array.Empty<IEntity>();
        return list;
    }

    /// <summary>
    /// Возвращает первую сущность для указанной роли. Если нет — IEntity.Empty().
    /// </summary>
    public static IEntity GetFirstSubject(this IHaveSubjects evt, Role role)
    {
        var list = evt.GetSubjects(role);
        return list.Count > 0 ? list[0] : null;
    }

    /// <summary>
    /// Гарантирует, что Subjects инициализирован. Если null — создаёт пустой.
    /// </summary>
    public static void EnsureSubjects(this IHaveSubjects evt)
    {
        if (evt.Subjects == null)
            evt.Subjects = Empty();
    }

    /// <summary>
    /// Добавляет сущность в список для указанной роли, инициализируя Subjects при необходимости.
    /// </summary>
    public static void AddSubject(this IHaveSubjects evt, Role role, IEntity entity)
    {
        evt.EnsureSubjects();
        if (!evt.Subjects.ContainsKey(role))
        {
            evt.Subjects.Add(role, new List<IEntity>());
        }
        evt.Subjects[role].Add(entity);
    }
}
