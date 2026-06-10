using CustomCodeFramework.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomCodeFramework.Postgres.EntityFramework.Configurations;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("inbox_messages");
        builder.HasKey(message => message.Id);

        builder.Property(message => message.EventId).HasColumnName("event_id").IsRequired();
        builder.Property(message => message.EventType).HasColumnName("event_type").HasMaxLength(500).IsRequired();
        builder.Property(message => message.EventName).HasColumnName("event_name").HasMaxLength(500).IsRequired();
        builder.Property(message => message.SourceService).HasColumnName("source_service").HasMaxLength(200).IsRequired();
        builder.Property(message => message.ConsumerService).HasColumnName("consumer_service").HasMaxLength(200).IsRequired();
        builder.Property(message => message.CorrelationId).HasColumnName("correlation_id").HasMaxLength(100);
        builder.Property(message => message.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(message => message.ProcessedAtUtc).HasColumnName("processed_at");
        builder.Property(message => message.CreatedAtUtc).HasColumnName("created_at").IsRequired();

        builder.HasIndex(message => new { message.EventId, message.ConsumerService }).IsUnique();
        builder.HasIndex(message => new { message.Status, message.CreatedAtUtc });
    }
}
