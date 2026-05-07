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
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            // Add request context to logs
            using (LogContext.PushProperty("RequestName", requestName))
            {
                _logger.LogInformation("Handling {RequestName}", requestName);

                try
                {
                    var response = await next();

                    _logger.LogInformation("Handled {RequestName}", requestName);

                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling {RequestName}", requestName);
                    throw;
                }
            }
        }
    }
}
