using System;
using System.Collections.Generic;

/// <summary>
/// Вспомогательные методы для работы с Subjects в событиях IHaveSubjects.
/// </summary>
public static class SubjectsHelper
{
    private static readonly int RoleCount = Enum.GetValues(typeof(SubjectRole)).Length;

    /// <summary>
    /// Создаёт пустой список списков субъектов (по одному слоту на каждый SubjectRole).
    /// </summary>
    public static List<List<Geid>> Empty()
    {
        var list = new List<List<Geid>>(RoleCount);
        for (int i = 0; i < RoleCount; i++)
            list.Add(new List<Geid>());
        return list;
    }

    /// <summary>
    /// Создаёт список субъектов с предзаполненными записями.
    /// </summary>
    public static List<List<Geid>> Create(params (SubjectRole role, Geid entity)[] entries)
    {
        var subjects = Empty();
        foreach (var (role, entity) in entries)
            subjects[(int)role].Add(entity);
        return subjects;
    }

    /// <summary>
    /// Возвращает список сущностей для указанной роли. Если нет — пустой список.
    /// </summary>
    public static IReadOnlyList<Geid> GetSubjects(this IHaveSubjects evt, SubjectRole role)
    {
        int idx = (int)role;
        if (evt.Subjects == null || idx >= evt.Subjects.Count || evt.Subjects[idx] == null)
            return Array.Empty<Geid>();
        return evt.Subjects[idx];
    }

    /// <summary>
    /// Возвращает первую сущность для указанной роли. Если нет — Geid.Empty.
    /// </summary>
    public static Geid GetFirstSubject(this IHaveSubjects evt, SubjectRole role)
    {
        var list = evt.GetSubjects(role);
        return list.Count > 0 ? list[0] : Geid.Empty;
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
    public static void AddSubject(this IHaveSubjects evt, SubjectRole role, Geid entity)
    {
        evt.EnsureSubjects();
        int idx = (int)role;
        while (evt.Subjects.Count <= idx)
            evt.Subjects.Add(new List<Geid>());
        evt.Subjects[idx].Add(entity);
    }
}
