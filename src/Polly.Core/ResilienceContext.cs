using System.Diagnostics.CodeAnalysis;

namespace Polly;

/// <summary>
/// A context assigned to a single execution of <see cref="ResilienceStrategy"/>. It is created manually or automatically
/// when the user calls the various extensions on top of <see cref="ResilienceStrategy"/>. After every execution the context should be discarded and returned to the pool.
/// </summary>
/// <remarks>
/// Do not re-use an instance of <see cref="ResilienceContext"/> across more than one execution. The <see cref="ResilienceContext"/> is retrieved from the pool
/// by calling the <see cref="Get"/> method. After you are done with it you should return it to the pool by calling the <see cref="Return"/> method.
/// </remarks>
public sealed class ResilienceContext
{
    internal const bool ContinueOnCapturedContextDefault = false;

    private ResilienceContext()
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="CancellationToken"/> associated with the execution.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Gets a value indicating whether the execution is synchronous.
    /// </summary>
    public bool IsSynchronous { get; private set; }

    /// <summary>
    /// Gets the type of the result associated with the execution.
    /// </summary>
    public Type ResultType { get; private set; } = typeof(UnknownResult);

    /// <summary>
    /// Gets a value indicating whether the execution represents a void result.
    /// </summary>
    public bool IsVoid => ResultType == typeof(VoidResult);

    /// <summary>
    /// Gets or sets a value indicating whether the execution should continue on the captured context.
    /// </summary>
    public bool ContinueOnCapturedContext { get; set; }

    /// <summary>
    /// Gets a value indicating whether the context is initialized.
    /// </summary>
    internal bool IsInitialized => ResultType != typeof(UnknownResult);

    /// <summary>
    /// Gets a <see cref="ResilienceContext"/> instance from the pool.
    /// </summary>
    /// <returns>An instance of <see cref="ResilienceContext"/>.</returns>
    /// <remarks>
    /// After the execution is finished you should return the <see cref="ResilienceContext"/> back to the pool
    /// by calling <see cref="Return(ResilienceContext)"/> method.
    /// </remarks>
    public static ResilienceContext Get() => new();

    /// <summary>
    /// Returns a <paramref name="context"/> back to the pool.
    /// </summary>
    /// <param name="context">The context instance.</param>
    public static void Return(ResilienceContext context)
    {
        Guard.NotNull(context);

        context.Reset();
    }

    [ExcludeFromCodeCoverage]
    [Conditional("DEBUG")]
    internal void AssertInitialized()
    {
        Debug.Assert(IsInitialized, "The resilience context is not initialized.");
    }

    internal ResilienceContext Initialize<TResult>(bool isSynchronous)
    {
        IsSynchronous = isSynchronous;
        ResultType = typeof(TResult);
        ContinueOnCapturedContext = ContinueOnCapturedContextDefault;

        return this;
    }

    private void Reset()
    {
        IsSynchronous = false;
        ResultType = typeof(UnknownResult);
        ContinueOnCapturedContext = false;
        CancellationToken = default;
    }

    /// <summary>
    /// Marker class that represents an unknown result.
    /// </summary>
    private static class UnknownResult
    {
    }
}