using FunctionAppFiapF2.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FunctionAppFiapF2.Data.EntityConfig
{
    internal class EstoqueConfig : IEntityTypeConfiguration<Estoque>
    {
        public void Configure(EntityTypeBuilder<Estoque> builder)
        {
            builder.ToTable("Estoque");
            builder.HasKey(p => p.CodigoProduto);
            builder.Property(p => p.CodigoProduto)
               .HasColumnType("varchar(4)");

            builder.Property(p => p.Quantidade);

            builder.HasOne(e => e.Produto)
            .WithOne()
            .HasForeignKey<Estoque>(e => e.CodigoProduto)
            .IsRequired();

        }
    }
}
