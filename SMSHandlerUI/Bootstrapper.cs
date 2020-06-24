using Caliburn.Micro;
using RealmDBHandler.CommonClasses;
using SMSHandlerUI.RuntimeData;
using SMSHandlerUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SMSHandlerUI
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public Bootstrapper()
        {
            Initialize();

            //variables for logging unhandled exceptions
            AppDomain currentDomain = default;
            currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<IRealmProvider, RealmDBLocator>();
            _container.Singleton<IRuntimeData, RuntimeDataHandler>();
            _container.PerRequest<ShellViewModel>();
        }

        public SimpleContainer GetContaier()
        {
            return _container;
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = _container.GetInstance(service, key);
            if (instance != null)
                return instance;
            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }


        //method for saving info about application crash exception to log file (when app is crashing)
        private void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Error($"UNHANDELED EXCEPTION START");
            logger.Error($"Application crashed: {ex.Message}.");
            logger.Error($"Inner exception: {ex.InnerException}.");
            logger.Error($"Stack trace: {ex.StackTrace}.");
            logger.Error($"UNHANDELED EXCEPTION FINISH");
        }
    }
}
