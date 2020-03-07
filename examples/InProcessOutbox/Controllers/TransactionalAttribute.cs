using System;
using System.Threading.Tasks;
using InProcessOutbox.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace InProcessOutbox.Controllers
{
    public class TransactionalAttribute : Attribute, IFilterFactory
    {
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var transactionalOutbox = serviceProvider.GetRequiredService<TransactionalOutbox>();
            return new TransactionalOutboxFilter(transactionalOutbox);
        }

        public bool IsReusable => false;

        private class TransactionalOutboxFilter : IAsyncActionFilter
        {
            private readonly TransactionalOutbox _transactionalOutbox;

            public TransactionalOutboxFilter(TransactionalOutbox transactionalOutbox)
            {
                _transactionalOutbox = transactionalOutbox;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                await _transactionalOutbox.Execute(async () => await next());
            }
        }
    }
}