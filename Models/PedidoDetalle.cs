using System.ComponentModel.DataAnnotations;

namespace Repaso3.Models
{
    public class PedidoDetalle
    {
        [Key]
        public int DetalleId { get; set; }

        public int PedidoId { get; set; }

        [Required]
        public int ProductoId { get; set; }


        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }
    }
}
