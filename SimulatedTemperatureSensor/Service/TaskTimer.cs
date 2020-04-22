namespace SimulatedTemperatureSensor
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class TaskTimer
    {
        readonly Action onTimer;
        readonly TimeSpan timerPeriod;
        readonly Action onError;
        readonly ILogger logger;

        public TaskTimer(Action onTimer,
            TimeSpan timerPeriod,
            ILogger logger,
            Action onError = null)
        {
            this.timerPeriod = timerPeriod;
            this.onTimer = onTimer;
            this.logger = logger;
            this.onError = onError;
        }

        public void Start(CancellationToken token)
        {
            Task elapsedTask = null;
            elapsedTask = new Task((x) =>
            {
                OnTimer(elapsedTask, token);
            }, token);

            HandleError(elapsedTask, token);

            elapsedTask.Start();
        }

        private void OnTimer(Task task, object objParam)
        {
            var start = DateTime.Now;
            var token = (CancellationToken)objParam;

            if (token.IsCancellationRequested)
            {
                logger.LogInformation("A cancellation has been requested.");
                return;
            }

            onTimer();

            var delay = timerPeriod - (DateTime.Now - start);
            if (delay.Ticks > 0)
            {
                task = Task.Delay(delay);
            }
            HandleError(task.ContinueWith(OnTimer, token), token);
        }

        private void HandleError(Task task, CancellationToken token)
        {
            task.ContinueWith((e) =>
            {
                logger.LogError(
                    $"Exception when running timer callback: {e.Exception}");

                onError?.Invoke();
                if (!token.IsCancellationRequested)
                    task.ContinueWith(OnTimer, token);

            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}