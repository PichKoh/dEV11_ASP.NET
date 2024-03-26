using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication11.Filters
{
    public class FileLogger : ILogger, IDisposable
    {
        string path;
        static object locker = new object();

        public FileLogger(string path)
        {
            this.path = path;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {

        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            lock (locker)
            {
                File.AppendAllText(path, formatter(state, exception) + Environment.NewLine);
            }
        }
    }

    public class FileLoggerFilter : Attribute, IAsyncActionFilter
    {
        private readonly FileLogger logger;
        private readonly string filename = "log.txt";

        public FileLoggerFilter()
        {
            this.logger = new FileLogger(this.filename);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            logger.LogInformation($"Method: {context.RouteData.Values["action"] as string}; " +
                $"Time: {DateTime.Now.ToLongTimeString()}");
            await next();
        }
    }

    public class UserLoggerFilter : Attribute, IAsyncResourceFilter
    {
        private readonly FileLogger logger;
        private readonly string filename = "userlog.txt";
        public UserLoggerFilter()
        {
            this.logger = new FileLogger(this.filename);
        }
        private string GetIP()
        {
            var hostname = System.Net.Dns.GetHostName();
            return System.Net.Dns.GetHostEntry(hostname).AddressList.GetValue(0).ToString();
        }
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            bool isListed = false;
            var IP = GetIP();
            string[] ips = File.ReadAllLines(this.filename);
            foreach (string ip in ips)
                if (ip.Equals(IP))
                    isListed = true;
            if (!isListed)
                logger.LogInformation($"{IP}");
            await next();
        }
    }
}