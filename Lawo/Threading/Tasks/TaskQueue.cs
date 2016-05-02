﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.Threading.Tasks
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>Represents a virtual queue for tasks.</summary>
    /// <remarks>Enqueued tasks are guaranteed to be executed sequentially, in the order in which they were enqueued.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This is a queue and we want the name to express that fact.")]
    public sealed class TaskQueue
    {
        /// <summary>Initializes a new instance of the <see cref="TaskQueue"/> class.</summary>
        public TaskQueue()
        {
        }

        /// <summary>Enqueues <paramref name="function"/>.</summary>
        /// <returns>A <see cref="Task"/> object that represents a proxy for the <see cref="Task"/> returned by
        /// <paramref name="function"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> equals <c>null</c>.</exception>
        /// <remarks><paramref name="function"/> is executed on the calling thread.</remarks>
        public Task Enqueue(Func<Task> function)
        {
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }

            return this.Enqueue(
                async () =>
                {
                    await function();
                    return false;
                });
        }

        /// <summary>Enqueues <paramref name="function"/>.</summary>
        /// <typeparam name="TResult">The type of the result returned by the <see cref="Task{T}"/> returned by
        /// <paramref name="function"/>.</typeparam>
        /// <returns>A <see cref="Task{T}"/> object that represents a proxy for the <see cref="Task{T}"/>
        /// returned by <paramref name="function"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="function"/> equals <c>null</c>.</exception>
        /// <remarks><paramref name="function"/> is executed on the calling thread.</remarks>
        public Task<TResult> Enqueue<TResult>(Func<Task<TResult>> function)
        {
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }

            var result = this.previousTask.IsCompleted ? function() : EnqueueCore(this.previousTask, function);
            this.previousTask = result;
            return result;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Task previousTask = Task.FromResult(false);

        private static async Task<TResult> EnqueueCore<TResult>(Task previousTask, Func<Task<TResult>> func)
        {
            await previousTask;
            return await func();
        }
    }
}