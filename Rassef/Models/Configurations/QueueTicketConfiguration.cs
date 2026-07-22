using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rassef.Models.Entities;

namespace Rassef.Models.Configurations
{
    public class QueueTicketConfiguration : IEntityTypeConfiguration<QueueTicket>
    {
        public void Configure(EntityTypeBuilder<QueueTicket> builder)
        {

        }
    }
}
