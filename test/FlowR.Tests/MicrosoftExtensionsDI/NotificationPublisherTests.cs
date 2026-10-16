using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace FlowR.Tests.MicrosoftExtensionsDI;

public class NotificationPublisherTests
{
    public class MockPublisher : INotificationPublisher
    {
        public int CallCount { get; set; }

        public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
        {
            foreach (var handlerExecutor in handlerExecutors)
            {
                await handlerExecutor.HandlerCallback(notification, cancellationToken);
                CallCount++;
            }
        }
    }

    [Fact]
    public void ShouldResolveDefaultPublisher()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Logger());
        services.AddFlowR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();

        mediator.ShouldNotBeNull();

        var publisher = provider.GetService<INotificationPublisher>();

        publisher.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldSubstitutePublisherInstance()
    {
        var publisher = new MockPublisher();
        var services = new ServiceCollection();
        services.AddSingleton(new Logger());
        services.AddFlowR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
            cfg.NotificationPublisher = publisher;
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();

        mediator.ShouldNotBeNull();

        await mediator.Publish(new Pinged(), CancellationToken.None);
        
        publisher.CallCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldSubstitutePublisherServiceType()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Logger());
        services.AddFlowR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
            cfg.NotificationPublisherType = typeof(MockPublisher);
            cfg.Lifetime = ServiceLifetime.Singleton;
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();
        var publisher = provider.GetService<INotificationPublisher>();

        mediator.ShouldNotBeNull();
        publisher.ShouldNotBeNull();

        await mediator.Publish(new Pinged(), CancellationToken.None);

        var mock = publisher.ShouldBeOfType<MockPublisher>();

        mock.CallCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldSubstitutePublisherServiceTypeWithWhenAll()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new Logger());
        services.AddFlowR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(CustomMediatorTests));
            cfg.NotificationPublisherType = typeof(TaskWhenAllPublisher);
            cfg.Lifetime = ServiceLifetime.Singleton;
        });

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();
        var publisher = provider.GetService<INotificationPublisher>();

        mediator.ShouldNotBeNull();
        publisher.ShouldNotBeNull();

        await Should.NotThrowAsync(mediator.Publish(new Pinged(), CancellationToken.None));

        publisher.ShouldBeOfType<TaskWhenAllPublisher>();
    }
}