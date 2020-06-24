using NLog;
using System;
using System.Timers;

namespace NLogHandler
{
    internal class NewLogFileCreatorTimer
    {
        #region Fields and properties

        private readonly Timer _oneHourTimer;
        private int _amountofHours;
        private int _hoursThatPassed;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public int HoursToElapse { get { return _amountofHours; } }

        #endregion

        #region Constructor

        public NewLogFileCreatorTimer(int amountOfHours)
        {
            if (amountOfHours > 0) _amountofHours = amountOfHours;
            else _amountofHours = 24;
            _hoursThatPassed = 0;
            _oneHourTimer = new Timer(3600000 * _amountofHours);
            _oneHourTimer.Elapsed += OneHourTimer_Elapsed;

            _oneHourTimer.Start();

            _logger.Info($"New Log File Creator Timer object constructed.");
        }

        #endregion

        #region Timer elapsed event

        private void OneHourTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _hoursThatPassed++;

            _logger.Info($"New log file creation timer elapsed hours: {_hoursThatPassed}. Total amount of hours to create new log file: {_amountofHours}.");

            _oneHourTimer.Stop();
            _oneHourTimer.Start();
            if (_hoursThatPassed >= _amountofHours)
            {
                _hoursThatPassed = 0;
                OnTimerElapsed();
            }

        }

        #endregion

        #region Changing time base

        public void ChangeTimerBase(int hours)
        {
            if (hours > 0)
            {
                if (_hoursThatPassed >= hours) OnTimerElapsed();
                _amountofHours = hours;
                _oneHourTimer.Interval = 3600000 * _amountofHours;
            }
        }

        #endregion

        #region Timer elapsed event

        public delegate void TimerElapsedEventHandler(object sender, EventArgs e);
        public event TimerElapsedEventHandler TimerElapsed;
        protected virtual void OnTimerElapsed()
        {
            _logger.Info($"New log file creation procedure event fired.");

            TimerElapsed?.Invoke(this, new EventArgs { });
        }

        #endregion
    }
}
