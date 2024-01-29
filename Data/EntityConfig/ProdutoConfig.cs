using FunctionAppFiapF2.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FunctionAppFiapF2.Data.EntityConfig
{
    public class ProdutoConfig : IEntityTypeConfiguration<Produto>
    {
        public void Configure(EntityTypeBuilder<Produto> builder)
        {
            builder.ToTable("Produto");
            builder.HasKey(p => p.Codigo);
            builder.Property(p => p.Codigo)
               .HasColumnType("varchar(4)");
            builder.Property(p => p.Nome).HasColumnType("varchar(100)");


            
        }
    }
}
