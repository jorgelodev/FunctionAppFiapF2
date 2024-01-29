using FunctionAppFiapF2.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FunctionAppFiapF2.Data.EntityConfig
{
    public class PedidoConfig : IEntityTypeConfiguration<Pedido>
    {
        public void Configure(EntityTypeBuilder<Pedido> builder)
        {
            builder.ToTable("Pedido");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                .HasColumnType("int")
                .UseIdentityColumn();

            builder.Property(p => p.Quantidade);
            builder.Property(p => p.Aprovado);
            builder.Property(p => p.CodigoProduto);
            builder.Property(p => p.NomeCliente);
            builder.Property(p => p.NumeroCartaoCredito);
            builder.Property(p => p.Data);

            builder.HasOne(i => i.Produto)
             .WithMany()
             .HasForeignKey(i => i.CodigoProduto)
             .HasPrincipalKey(p => p.Codigo)
             .IsRequired();

            builder.Ignore(p => p.ValidationResult);

        }
    }
}
