using CustomCodeFramework.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomCodeFramework.Postgres.EntityFramework.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.EventId).HasColumnName("event_id").IsRequired();
        builder.Property(message => message.EventType).HasColumnName("event_type").HasMaxLength(500).IsRequired();
        builder.Property(message => message.EventName).HasColumnName("event_name").HasMaxLength(500).IsRequired();
        builder.Property(message => message.SourceService).HasColumnName("source_service").HasMaxLength(200).IsRequired();
        builder.Property(message => message.PayloadJson).HasColumnName("payload_json").HasColumnType("jsonb").IsRequired();
        builder.Property(message => message.HeadersJson).HasColumnName("headers_json").HasColumnType("jsonb");
        builder.Property(message => message.CorrelationId).HasColumnName("correlation_id").HasMaxLength(100);
        builder.Property(message => message.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(message => message.RetryCount).HasColumnName("retry_count").IsRequired();
        builder.Property(message => message.ErrorMessage).HasColumnName("error_message");
        builder.Property(message => message.CreatedAtUtc).HasColumnName("created_at").IsRequired();
        builder.Property(message => message.ProcessedAtUtc).HasColumnName("processed_at");

        builder.HasIndex(message => message.EventId).IsUnique();
        builder.HasIndex(message => new { message.Status, message.CreatedAtUtc });
    }
}
