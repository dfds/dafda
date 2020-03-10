using System;
using System.Threading.Tasks;
using Academia.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Academia.Controllers
{
    public class TransactionalAttribute : Attribute, IFilterFactory
    {
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var transactionalOutbox = serviceProvider.GetRequiredService<ITransactionalOutbox>();
            return new TransactionalOutboxFilter(transactionalOutbox);
        }

        public bool IsReusable => false;

        private class TransactionalOutboxFilter : IAsyncActionFilter
        {
            private readonly ITransactionalOutbox _transactionalOutbox;

            public TransactionalOutboxFilter(ITransactionalOutbox transactionalOutbox)
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