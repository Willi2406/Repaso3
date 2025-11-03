using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repaso3.Models
{
    public class Pedido
    {
        [Key]
        public int PedidoId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "EL nombre del cliente es obligatorio")]
        public string ClienteNombre { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El total debe ser mayor a 0")]
        public decimal Total { get; set; }


        [ForeignKey("PedidoId")]
        public virtual ICollection<PedidoDetalle> Detalle { get; set; } = new List<PedidoDetalle>();
    }
}
