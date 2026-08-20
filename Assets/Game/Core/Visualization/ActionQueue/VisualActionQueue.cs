using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Асинхронный координатор времени визуальных эффектов (Visual Playback Queue).
/// Воспроизводит анимации последовательно, разделяя логику и представление.
/// </summary>
public sealed class VisualActionQueue : MonoBehaviour
{
    public static VisualActionQueue Instance { get; private set; }

    [SerializeField] private BattlefieldRenderer _renderer;

    private readonly Queue<IVisualTask> _queue = new Queue<IVisualTask>();
    private bool _isPlaying = false;

    public bool IsPlaying => _isPlaying || _queue.Count > 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Нам нужно связать очередь с рендерером
            if (_renderer == null) _renderer = FindFirstObjectByType<BattlefieldRenderer>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Enqueue(IVisualTask task)
    {
        if (task == null) return;
        _queue.Enqueue(task);
        
        if (!_isPlaying)
        {
            _ = PlayNextTaskAsync();
        }
    }

    private async Task PlayNextTaskAsync()
    {
        _isPlaying = true;

        while (_queue.Count > 0)
        {
            var task = _queue.Dequeue();
            try
            {
                if (_renderer != null)
                {
                    await task.PlayAnimationAsync(_renderer);
                }
                else
                {
                    Debug.LogWarning("[VisualActionQueue] BattlefieldRenderer is missing! Skipping task.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[VisualActionQueue] Error during visual task playback: {e.Message}\n{e.StackTrace}");
            }
        }

        _isPlaying = false;
    }
}
