using System.Threading.Tasks;
using UnityEngine;
using System.Collections;

public class EntityView : MonoBehaviour
{
    public GEID EntityId { get; private set; }
    
    private Coroutine _moveCoroutine;

    public void Init(GEID id)
    {
        EntityId = id;
    }

    public void SetPosition(Vector3 position)
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
        transform.position = position;
    }

    public async Task MoveToAsync(Vector3 targetPosition, float duration = 0.3f)
    {
        if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        
        var tcs = new TaskCompletionSource<bool>();
        _moveCoroutine = StartCoroutine(MoveCoroutine(targetPosition, duration, tcs));
        
        await tcs.Task;
    }

    private IEnumerator MoveCoroutine(Vector3 targetPosition, float duration, TaskCompletionSource<bool> tcs)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0;
        float arcHeight = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            
            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPosition;
        tcs.SetResult(true);
    }

    public async Task PlayDamageAnimationAsync(int amount)
    {
        if (amount <= 0) return;

        Debug.Log($"[View] {gameObject.name} (ID: {EntityId}) took {amount} damage!");
        
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(ShakeCoroutine(tcs));
        
        await tcs.Task;
    }

    private IEnumerator ShakeCoroutine(TaskCompletionSource<bool> tcs)
    {
        Vector3 originalPos = transform.position;
        float duration = 0.2f;
        float elapsed = 0;
        float magnitude = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = originalPos + UnityEngine.Random.insideUnitSphere * magnitude;
            yield return null;
        }
        transform.position = originalPos;
        tcs.SetResult(true);
    }
}
