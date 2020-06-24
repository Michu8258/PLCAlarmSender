using NLog;
using RealmDBHandler.CommonClasses;
using S7Connections.TagsReading;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace S7AlarmsReader.CycleScan
{
    public static class CycleScanTimer
    {
        #region Fields and properties

        private static System.Timers.Timer _scanTimer;
        private static int _scanPeriod;
        private static readonly Logger _logger;
        private static readonly SynchronizationContext _synchCont;
        private static bool _scanIsRunning;
        private static Dictionary<int, List<AlarmDataModel>> _alarmsData;
        private static IRealmProvider _realmProvider;

        public static long LastS7ScanTimeMiliseconds { get; private set; }

        #endregion

        #region Constructor

        //ststic construvtor - assigning synchronization context for
        //collecting ended tasks of scanning alarms of S7
        static CycleScanTimer()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _synchCont = SynchronizationContext.Current;
            _scanIsRunning = false;
            _alarmsData = new Dictionary<int, List<AlarmDataModel>>();
            _logger.Info($"Static scan timer created.");
        }

        //thi method must be called to start scanning alarms
        public static void StartChecking(IRealmProvider realmProvider)
        {
            _realmProvider = realmProvider;

            GetTimerPeriod();
            _scanTimer = new System.Timers.Timer(_scanPeriod * 1000);
            _scanTimer.Elapsed += ScanTimer_Elapsed;
            _scanTimer.Start();
        }

        #endregion

        #region Change scan period

        //for now, the scan period is set to 5 seconds.
        //probably to be parametrized in the future
        private static void GetTimerPeriod()
        {
            _scanPeriod = 5;
            //TODO - parametrize this
        }

        //method for changing scan cycle in runtime
        public static bool ChangeScanPeriod(int seconds)
        {
            if (seconds >= 1 && seconds <= 20)
            {
                _scanTimer.Interval = seconds * 1000;
                return true;
            }
            return false;
        }

        #endregion

        #region Timer elapsed - start new scan

        //when timer elapses - start new tstk for reading alarms
        private static void ScanTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_scanIsRunning)
            {
                _scanTimer.Stop();
                _scanTimer.Start();

                _scanIsRunning = true;
                Task.Run(() => StartNewScanTask());
            }
        }

        //start scan as new task
        private static void StartNewScanTask()
        {
            SingleS7ScanTask scan = new SingleS7ScanTask(_alarmsData, _realmProvider);
            scan.TaskFinished += Scan_TaskFinished;
            scan.Start();
        }

        //scan finished - event from task to produce event that is subscripted in GUI
        private static void Scan_TaskFinished(object sender, TaskFinishedEventArgs e)
        {
            _synchCont.Post(_ => _alarmsData = e.AlarmsMemory, null);
            _synchCont.Post(_ => LastS7ScanTimeMiliseconds = e.ScanTimeMiliseconds, null);
            _synchCont.Post(_ => OnSS7canFinished(), null);
            _synchCont.Post(_ => _scanIsRunning = false, null);
        }

        #endregion

        #region Event for returning scan finishd to the GUI

        //event to be transfered to GUI - GUI should then refresh values of event table
        public delegate void SingleScanFinishedEventHandler(object sender, EventArgs e);
        public static event SingleScanFinishedEventHandler S7ScanFinished;
        public static void OnSS7canFinished()
        {
            S7ScanFinished?.Invoke(null, new EventArgs());
        }

        #endregion
    }
}
