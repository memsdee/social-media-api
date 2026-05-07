using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Databases.Write;
using be.Infrastructure.Common.Appsetting;
using be.Infrastructure.Extensions;
using be.Infrastructure.Kafka.Consumer;
using be.Infrastructure.Repository.Read;
using be.Infrastructure.Repository.Write;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace be.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddInfrastructureOptions(configuration)
            .AddInfrastructureRedis(configuration)
            .AddInfrastructurePostgres(configuration)
            .AddInfrastructureMongo()
            .AddInfrastructureKafka()
            .AddInfrastructureHangfire(configuration)
            .AddInfrastructureHttpClients()
            .AddInfrastructureServices()
            .AddInfrastructureDebezium(configuration);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IConversationUserRepository, ConversationUserRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IUsernameChangeLogsRepository, UsernameChangeLogsRepository>();
        services.AddScoped<IUseridChangeLogsRepository, UseridChangeLogsRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IReportPostRepository, ReportPostRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<INotiCmtRepository, NotiCmtRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<IMessLogRepository, MessLogRepository>();
        services.AddScoped<INotiReactPostRepository, NotiReactPostRepository>();
        services.AddScoped<IReactPostRepository, ReactPostRepository>();
        services.AddScoped<IThirdPartyLoginsRepository, ThirdPartyLoginsRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPostReadRepository, PostReadRepository>();
        services.AddScoped<IReactReadRepository, ReactReadRepository>();
        services.AddScoped<IReportPostReadRepository, ReportPostReadRepository>();
        services.AddScoped<INotificationReadRepository, NotificationReadRepository>();
        services.AddScoped<IConversationReadRepository, ConversationReadRepository>();
        services.AddScoped<IMessageReadRepository, MessageReadRepository>();
        services.AddScoped<ICommentReadRepository, CommentReadRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IFollowReadRepository, FollowReadRepository>();

        services.AddSingleton(resolver =>
        {
            var infraSettings = resolver.GetRequiredService<IOptions<DefaultInfoSettings>>().Value;
            return new Application.Common.Settings.DefaultInfoSettings
            {
                Avatar = infraSettings.Avatar,
                DeletedAvatar = infraSettings.DeletedAvatar,
                DeletedName = infraSettings.DeletedName
            };
        });

        services.AddSingleton(resolver =>
        {
            var infraSettings = resolver.GetRequiredService<IOptions<UploadcareSettings>>().Value;
            return new Application.Common.Settings.UploadcareSettings
            {
                PublicKey = infraSettings.PublicKey,
                SecretKey = infraSettings.SecretKey,
                ExpMinute = infraSettings.ExpMinute,
                UrlApi = infraSettings.UrlApi,
                UrlImg = infraSettings.UrlImg
            };
        });

        services.AddHostedService<DelPostConsumer>();
        services.AddHostedService<PostConsumer>();
        services.AddHostedService<ReportPostConsumer>();
        services.AddHostedService<ConversationConsumer>();
        services.AddHostedService<MessageConsumer>();
        services.AddHostedService<MarkReadNotiConsumer>();
        services.AddHostedService<CommentConsumer>();
        services.AddHostedService<NotiCommentConsumer>();
        services.AddHostedService<FollowConsumer>();
        services.AddHostedService<UnfollowConsumer>();
        services.AddHostedService<UpdateAvatarConsumer>();
        services.AddHostedService<UpdateReadMessConsumer>();
        services.AddHostedService<NotificationConsumer>();
        
        
        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var kafkaSettings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers
            };

            if (kafkaSettings.UseSaslSsl)
            {
                config.SecurityProtocol = SecurityProtocol.SaslSsl;
                config.SaslMechanism = SaslMechanism.Plain;
                config.SaslUsername = kafkaSettings.UserName;
                config.SaslPassword = kafkaSettings.Password;
            }

            return new ProducerBuilder<string, string>(config).Build();
        });

        return services;
    }
}