using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository;
using log4net;

namespace STORE_DataTransfer
{
    public class LogConfigurator
    {
        public static void ConfigureLogging()
        {
            // Check if the 'A://' drive exists
            string logPath;
            if (Directory.Exists("A://"))
            {
                logPath = "A://DataTransferlog/logfile.log";
            }
            else if(Directory.Exists("D://"))
            {
                logPath = "D://DataTransferlog/logfile.log";
            }
            else if (Directory.Exists("E://"))
            {
                logPath = "E://DataTransferlog/logfile.log";
            }
            else if (Directory.Exists("F://"))
            {
                logPath = "F://DataTransferlog/logfile.log";
            }
            else
            {
                logPath = "C://DataTransferlog/logfile.log";
            }
            // Create a rolling file appender
            var fileAppender = new RollingFileAppender
            {
                AppendToFile = true,
                File = logPath,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                MaxSizeRollBackups = 5,
                MaximumFileSize = "100MB",
                StaticLogFileName = true
            };

            // Define a pattern layout
            var layout = new PatternLayout
            {
                ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
            };
            layout.ActivateOptions(); // Activate the layout

            // Set the layout for the appender
            fileAppender.Layout = layout;
            fileAppender.ActivateOptions(); // Activate the appender

            // Create the logger repository
            ILoggerRepository logRepository = LogManager.GetRepository();
            BasicConfigurator.Configure(logRepository, fileAppender);

            // Optionally, you can set the logging level globally
            ((log4net.Repository.Hierarchy.Hierarchy)logRepository).Root.Level = log4net.Core.Level.All;
        }
    }
}
