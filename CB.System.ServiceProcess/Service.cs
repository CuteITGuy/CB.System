using System;
using System.ServiceProcess;
using System.Threading.Tasks;


namespace CB.System.ServiceProcess
{
    public class Service
    {
        #region Methods
        public static bool Continue(string serviceName, TimeSpan timeout)
            => HandleService(serviceName, service => { service.Continue(); }, ServiceControllerStatus.Running, timeout);

        public static async Task<bool> ContinueAsync(string serviceName, TimeSpan timeout)
            => await Task.Run(() => Continue(serviceName, timeout));
        
        public static bool Pause(string serviceName, TimeSpan timeout)
            => HandleService(serviceName, service => { service.Pause(); }, ServiceControllerStatus.Paused, timeout);

        public static async Task<bool> PauseAsync(string serviceName, TimeSpan timeout)
            => await Task.Run(() => Pause(serviceName, timeout));

        public static bool Restart(string serviceName, TimeSpan timeout)
            => Stop(serviceName, timeout) && Start(serviceName, timeout);

        public static async Task<bool> RestartAsync(string serviceName, TimeSpan timeout)
            => await Task.Run(() => Restart(serviceName, timeout));

        public static bool Start(string serviceName, TimeSpan timeout)
            => HandleService(serviceName, service => { service.Start(); }, ServiceControllerStatus.Running, timeout);

        public static async Task<bool> StartAsync(string serviceName, TimeSpan timeout)
            => await Task.Run(() => Start(serviceName, timeout));

        public static bool Stop(string serviceName, TimeSpan timeout)
            => HandleService(serviceName, service => { service.Stop(); }, ServiceControllerStatus.Stopped, timeout);

        public static async Task<bool> StopAsync(string serviceName, TimeSpan timeout)
            => await Task.Run(() => Stop(serviceName, timeout));

        public static ServiceControllerStatus GetStatus(string serviceName)
        {
            using (var service = new ServiceController(serviceName))
            {
                return service.Status;
            }
        }
        #endregion


        #region Implementation
        private static bool HandleService(string serviceName, Action<ServiceController> serviceHandler,
            ServiceControllerStatus desiredStatus, TimeSpan timeout)
        {
            try
            {
                using (var service = new ServiceController(serviceName))
                {
                    serviceHandler(service);
                    service.WaitForStatus(desiredStatus, timeout);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}