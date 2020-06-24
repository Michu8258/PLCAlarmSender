using System;
using System.Timers;
using NLog;

namespace SMSHandlerUI.RuntimeData
{
    public static class RuntimeLogoutTimer
    {
        //field that holds timer
        private static Timer _logoutTimer;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        //main method for starting the timer
        public static void StartLogoutTimer(bool start, int minutes)
        {
            if (start)
            {
                _logoutTimer = new Timer(minutes * 60000);
                _logoutTimer.Elapsed += LogoutTimer_Elapsed;
                _logoutTimer.Start();

                _logger.Info($"Automatic logout timer started. Amount of minutes to logout: {minutes}.");
            }
            else
            {
                _logoutTimer = null;
            }
        }

        //event, when timer elapsed, because it was no user activity
        private static void LogoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _logger.Info($"Automati logout timer elapsed - firing event for auto log out.");

            OnLogoutCurrentUser();
        }

        //method called from various windows - activity of user
        public static void UserActivityDetected()
        {
            if (_logoutTimer != null)
            {
                _logoutTimer.Stop();
                _logoutTimer.Start();
            }
        }

        //turn off timer - logout
        public static void TurnOffTimer()
        {
            _logger.Info($"Turning off automatic logou timer.");

            if (_logoutTimer != null) _logoutTimer.Stop();
            _logoutTimer = null;
        }

        //event - when timer elapsed occurs - send event to logout current user
        public delegate void LogoutTimerElapsedEventHandler(object sender, EventArgs e);
        public static event LogoutTimerElapsedEventHandler LogoutCurrentUser;
        private static void OnLogoutCurrentUser()
        {
            LogoutCurrentUser?.Invoke(typeof(RuntimeLogoutTimer), EventArgs.Empty);
        }
    }
}
