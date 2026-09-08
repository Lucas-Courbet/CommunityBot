namespace CommunityBot.Tests.Integration.Support;

internal sealed class AsyncBarrier(int participantCount)
{
    private readonly Lock _lock = new();
    private readonly TaskCompletionSource _release =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private int _remaining = participantCount;

    public async Task SignalAndWaitAsync(CancellationToken ct)
    {
        Task releaseTask;

        lock (_lock)
        {
            _remaining--;

            if (_remaining == 0)
                _release.TrySetResult();

            releaseTask = _release.Task;
        }

        await releaseTask.WaitAsync(TimeSpan.FromSeconds(10), ct);
    }
}