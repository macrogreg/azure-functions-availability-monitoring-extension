using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace Microsoft.Azure.WebJobs.Extensions.AvailabilityMonitoring
{
    internal class AvailabilityTestBinding : IBinding
    {
        private readonly ParameterDescriptor _parameter;
        private readonly IValueBinder _binder;

        public AvailabilityTestBinding(IValueBinder binder, ParameterDescriptor parameter)
        {
            Validate.NotNull(binder, nameof(binder));
            Validate.NotNull(parameter, nameof(parameter));

            _binder = binder;
            _parameter = parameter;
        }

        public bool FromAttribute
        {
            get { return true; }
        }

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            return Task.FromResult((IValueProvider) _binder);
        }

        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            return Task.FromResult((IValueProvider) _binder);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return _parameter;
        }
    }
}