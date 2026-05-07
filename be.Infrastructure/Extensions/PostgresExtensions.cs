using be.Domain.Enums;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.NameTranslation;

namespace be.Infrastructure.Extensions;

public static class PostgresExtensions
{
    public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services,
        IConfiguration configuration)
    {
        var writeConnections = configuration.GetConnectionString("WriteConnections")
                               ?? throw new InvalidOperationException("WriteConnections configuration đang trống");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(writeConnections);
        dataSourceBuilder.MapEnum<RoleEnum>("role_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<ReactEnum>("react_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<ImageEnum>("image_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<NotiActionEnum>("noti_action_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<NotiTargetEnum>("noti_target_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<TypeMessageEnum>("type_message_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<TypeConversationEnum>("type_conversation_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<StatusReportPostEnum>("status_report_post_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<StatusAccountEnum>("status_account_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<StatusPostEnum>("status_post_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<ThirdPartyLoginEnum>("third_party_login_enum", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<OutboxTopicEnum>("outbox_topic_enum", new NpgsqlNullNameTranslator());

        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<WriteContext>(options =>
        {
            options.UseNpgsql(dataSource, o =>
            {
                o.MapEnum<RoleEnum>("role_enum", "enum");
                o.MapEnum<ReactEnum>("react_enum", "enum");
                o.MapEnum<ImageEnum>("image_enum", "enum");
                o.MapEnum<NotiActionEnum>("noti_action_enum", "enum");
                o.MapEnum<NotiTargetEnum>("noti_target_enum", "enum");
                o.MapEnum<TypeMessageEnum>("type_message_enum", "enum");
                o.MapEnum<TypeConversationEnum>("type_conversation_enum", "enum");
                o.MapEnum<StatusReportPostEnum>("status_report_post_enum", "enum");
                o.MapEnum<StatusAccountEnum>("status_account_enum", "enum");
                o.MapEnum<StatusPostEnum>("status_post_enum", "enum");
                o.MapEnum<ThirdPartyLoginEnum>("third_party_login_enum", "enum");
                o.MapEnum<OutboxTopicEnum>("outbox_topic_enum", "enum");
            });
        });

        return services;
    }
}