namespace PrimS.Telnet
{
  using System;
#if ASYNC
  using System.Threading.Tasks;
#endif

  internal static class PreprocessorAsyncAdapter
  //#pragma warning disable format
  {
    /// <summary>
    /// The preferred Execute which just returns the task, no internal async await .ConfigureAwait(false)
    /// Add:
    /// #if ASYNC
    ///     return
    /// #endif
    /// before the call.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns></returns>
    internal static
#if ASYNC
    Task
#else
        void
#endif
      Execute(
#if ASYNC
    Func<Task>
#else
        Action
#endif
      action)
    {
#if ASYNC
      return
#endif
      action()
        ;
    }

#if ASYNC
#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
#endif
    /// <summary>
    /// The Execute which awaits the task internally with .ConfigureAwait(false). Prefer Execute which just returns the Task.
    /// Add:
    /// #if ASYNC
    ///     await
    /// #endif
    /// before the call.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns></returns>
    internal static
#if ASYNC
    async Task
#else
        void
#endif
      ExecuteWithConfigureAwait(
#if ASYNC
    Func<Task>
#else
        Action
#endif
      action)
    {
#if ASYNC
      await
#endif
      action()
#if ASYNC
        .ConfigureAwait(false)
#endif
      ;
    }

    /// <summary>
    /// The preferred Execute which just returns the task, no internal async await .ConfigureAwait(false)
    /// Add:
    /// #if ASYNC
    ///     return
    /// #endif
    /// before the call.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns></returns>
    internal static
#if ASYNC
        Task<TOut>
#else
        TOut
#endif
          Execute<TOut>(
#if ASYNC
        Func<Task<TOut>>
#else
        Func<TOut>
#endif
          action)
    {
      return action();
    }

    //internal async static Task<TOut> ExecuteWithConfigureAwait<TOut>(Func<TOut> value)
    internal static
#if ASYNC
    async Task<TOut>
#else
    TOut
#endif
      ExecuteWithConfigureAwait<TOut>(
#if ASYNC
    Func<Task<TOut>>
#else
    Func<TOut>
#endif
    action)
    {
      return
#if ASYNC
      await
#endif
      action()
#if ASYNC
        .ConfigureAwait(false)
#endif
      ;
    }
#if ASYNC
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
#endif
  }
  //#pragma warning restore format
}
