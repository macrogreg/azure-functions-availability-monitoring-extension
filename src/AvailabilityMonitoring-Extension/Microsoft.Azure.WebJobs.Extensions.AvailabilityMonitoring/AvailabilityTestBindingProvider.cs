using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Newtonsoft.Json.Linq;


namespace Microsoft.Azure.WebJobs.Extensions.AvailabilityMonitoring
{
    internal class AvailabilityTestBindingProvider : IBindingProvider
    {
        private TelemetryClient _telemetryClient;

        public AvailabilityTestBindingProvider()
        {
        }

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            Validate.NotNull(context, nameof(context));

            ParameterInfo parameter = context.Parameter;
            AvailabilityTestAttribute attribute = parameter.GetCustomAttribute<AvailabilityTestAttribute>(inherit: false);

            if (attribute == null)
            {
                throw new InvalidOperationException($"Parameter \"{parameter.ParameterType.Name} {parameter.Name}\" is expected to have an"
                                                  + $" attribute of type \"{nameof(AvailabilityTestAttribute)}\", but no such attribute was found.");
            }

            var parameterDesc = new ParameterDescriptor()
            {
                Name = parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Description = "value"
                }
            };

            IValueBinder binder = CreateAvailabilityTestInvocationBinder(attribute, parameter);

            var binding = new AvailabilityTestBinding(binder, parameterDesc);
            return Task.FromResult((IBinding) binding);
        }

        private IValueBinder CreateAvailabilityTestInvocationBinder(AvailabilityTestAttribute attribute, ParameterInfo parameter)
        {
            if (AvailabilityTestInvocationBinder.BoundValueType.IsAssignableFrom(parameter.ParameterType))
            {
                var binder = new AvailabilityTestInvocationBinder(attribute, _telemetryClient);
                return binder;
            }
            else if (ConverterBinder<AvailabilityTelemetry, AvailabilityTestInvocation>.BoundValueType.IsAssignableFrom(parameter.ParameterType))
            {
                var binder = new ConverterBinder<AvailabilityTelemetry, AvailabilityTestInvocation>(
                                        new AvailabilityTestInvocationBinder(attribute, _telemetryClient),
                                        Convert.AvailabilityTestInvocationToAvailabilityTelemetry,
                                        Convert.AvailabilityTelemetryToAvailabilityTestInvocation);
                return binder;
            }
            else if (ConverterBinder<JObject, AvailabilityTestInvocation>.BoundValueType.IsAssignableFrom(parameter.ParameterType))
            {
                var binder = new ConverterBinder<JObject, AvailabilityTestInvocation>(
                                        new AvailabilityTestInvocationBinder(attribute, _telemetryClient),
                                        Convert.AvailabilityTestInvocationToJObject,
                                        Convert.JObjectToAvailabilityTestInvocation);
                return binder;
            }
            else
            {
                // @ToDo Test that IsAssignableFrom stuff!

                throw new InvalidOperationException($"Trying to use {nameof(AvailabilityTestAttribute)} to bind the parameter \"{parameter.ParameterType.Name} {parameter.Name}\"."
                                                  + $" This attribute can only bind values of the following types:"
                                                  + $" \"{AvailabilityTestInvocationBinder.BoundValueType.FullName}\","
                                                  + $" \"{ConverterBinder<AvailabilityTelemetry, AvailabilityTestInvocation>.BoundValueType.FullName}\","
                                                  + $" \"{ConverterBinder<JObject, AvailabilityTestInvocation>.BoundValueType.FullName}\".");
            }
        }
    }
}