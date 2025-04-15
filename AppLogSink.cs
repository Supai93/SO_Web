using System;
using Serilog.Core;
using Serilog.Events;
using SO_Web.Data;
using SO_Web.Models;

namespace SO_Web.Serilog.Sink
{
    public class AppLogSink : ILogEventSink
    {
        private readonly AppDbContext _dbContext;

        public AppLogSink(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public void Emit(LogEvent logEvent)
        {
            var newLog = new SystemLog
            {                
                LogDate = DateTime.Now,
                Description = logEvent.RenderMessage()
            };

            if (logEvent.Properties.TryGetValue("UserID", out var userID) && userID != null)
            {
                newLog.UserID = userID.ToString().Trim('"');
            }
            if (logEvent.Properties.TryGetValue("UserAddress", out var userAddr) && userAddr != null)
            {
                newLog.UserAddress = userAddr.ToString().Trim('"');
            }
            if (logEvent.Properties.TryGetValue("Action", out var action) && action != null)
            {
                newLog.Action = action.ToString().Trim('"');
            }
            if (logEvent.Properties.TryGetValue("Table", out var tableName) && tableName != null)
            {
                newLog.TableName = tableName.ToString().Trim('"');
            }
            if (logEvent.Properties.TryGetValue("State", out var state) && state != null)
            {
                newLog.State = state.ToString().Trim('"');
            }
            

            _dbContext.SystemLogs.Add(newLog);
        }
    }
}