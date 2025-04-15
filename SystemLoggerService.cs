using System;
using Serilog;
using Serilog.Context;
using Unity;

namespace SO_Web.Services
{
    public class SystemLoggerService : ILoggingService
    {
        private readonly ILogger _logger;

        public SystemLoggerService([Dependency("LoggerSystem")]ILogger logger)
        {
            this._logger = logger;
        }

        public void LogInfo(string message, string table = null, string state = null)
        {
            IDisposable _table = null, _state = null;
            if (!string.IsNullOrEmpty(table)) _table = LogContext.PushProperty("Table", table);
            if (!string.IsNullOrEmpty(state)) _state = LogContext.PushProperty("State", state);
            
            _logger.Information(message);

            if (_table != null) _table.Dispose();
            if (_state != null) _state.Dispose();
        }
    }

    public interface ILoggingService
    {
        void LogInfo(string message, string table = null, string state = null);
    }
}