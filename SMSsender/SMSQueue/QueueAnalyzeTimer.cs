using System;
using System.Timers;

namespace SMSsender.SMSQueue
{
    internal static class QueueAnalyzeTimer
    {
        private static Timer _queueTimer;

        public static void StartTimer(int miliseconds)
        {
            _queueTimer = new Timer(miliseconds);
            _queueTimer.Elapsed += QueueTimer_Elapsed;
            _queueTimer.Start();
        }

        private static void QueueTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _queueTimer.Stop();
            OnStartQueueScan();
            _queueTimer.Start();
        }

        //event to be transfered to GUI - GUI should then refresh values of event table
        public delegate void StartSingleQueueScanEventHandler(object sender, EventArgs e);
        public static event StartSingleQueueScanEventHandler StartQueueScan;
        public static void OnStartQueueScan()
        {
            StartQueueScan?.Invoke(null, new EventArgs());
        }
    }
}
