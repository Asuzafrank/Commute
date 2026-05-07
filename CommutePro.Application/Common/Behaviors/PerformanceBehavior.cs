using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Common.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            var response = await next();

            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            using (LogContext.PushProperty("RequestName", requestName))
            using (LogContext.PushProperty("ElapsedMs", elapsedMilliseconds))
            {
                if (elapsedMilliseconds > 500)
                {
                    _logger.LogWarning(
                        "Long Running Request: {RequestName} took {ElapsedMilliseconds}ms",
                        requestName, elapsedMilliseconds);
                }
                else if (elapsedMilliseconds > 200)
                {
                    _logger.LogInformation(
                        "Medium Running Request: {RequestName} took {ElapsedMilliseconds}ms",
                        requestName, elapsedMilliseconds);
                }
            }

            return response;
        }
    }
}
