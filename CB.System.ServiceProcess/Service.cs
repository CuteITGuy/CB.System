using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Win32;


namespace CB.System.ServiceProcess
{
    public class Service: IDisposable
    {
        #region Fields
        private const string REGISTRY_SERVICE_PATH = @"SYSTEM\CurrentControlSet\Services\";
        private const string REGISTRY_STARTUP_KEYNAME = "Start";
        private ServiceStartupType _startupType;
        #endregion


        #region  Constructors & Destructor
        public Service(string serviceName)
        {
            Controller = new ServiceController(serviceName);
            RegistryKey = Registry.LocalMachine.OpenSubKey(REGISTRY_SERVICE_PATH + serviceName, true);
            StartupType = GetRegistryStartupType();
        }
        #endregion


        #region  Properties & Indexers
        public ServiceController Controller { get; set; }

        public RegistryKey RegistryKey { get; set; }

        public ServiceStartupType StartupType
        {
            get { return _startupType; }
            set
            {
                if (_startupType == value) return;

                _startupType = value;
                SetRegistryStarupType(value);
            }
        }
        #endregion


        #region Methods
        public bool Continue(TimeSpan timeout) => Handle(() =>
        {
            Controller.Continue();
            Controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
        });

        public async Task<bool> ContinueAsync(TimeSpan timeout)
            => await Task.Run(() => Continue(timeout));

        public void Dispose()
        {
            Controller?.Dispose();
            RegistryKey?.Dispose();
        }

        public bool Pause(TimeSpan timeout) => Handle(() =>
        {
            Controller.Pause();
            Controller.WaitForStatus(ServiceControllerStatus.Paused, timeout);
        });

        public async Task<bool> PauseAsync(TimeSpan timeout)
            => await Task.Run(() => Pause(timeout));

        public bool Restart(TimeSpan timeout) => Stop(timeout) && Start(timeout);

        public async Task<bool> RestartAsync(TimeSpan timeout)
            => await Task.Run(() => Restart(timeout));

        public bool Start(TimeSpan timeout) => Handle(() =>
        {
            Controller.Start();
            Controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
        });

        public async Task<bool> StartAsync(TimeSpan timeout)
            => await Task.Run(() => Start(timeout));

        public bool Stop(TimeSpan timeout) => Handle(() =>
        {
            Controller.Stop();
            Controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
        });

        public async Task<bool> StopAsync(TimeSpan timeout)
            => await Task.Run(() => Stop(timeout));
        #endregion


        #region Implementation
        private ServiceStartupType GetRegistryStartupType()
        {
            var value = RegistryKey.GetValue(REGISTRY_STARTUP_KEYNAME);
            return (ServiceStartupType)value;
        }

        private static bool Handle(Action handler)
        {
            try
            {
                handler();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SetRegistryStarupType(ServiceStartupType value)
            => RegistryKey.SetValue(REGISTRY_STARTUP_KEYNAME, (int)value);
        #endregion
    }
}