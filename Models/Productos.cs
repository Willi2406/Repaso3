using System.ComponentModel.DataAnnotations;

namespace Repaso3.Models
{
    public class Productos
    {
        [Key]
        public int ProductoId { get; set; }
        public string Descripcion { get; set; }
        public int Existencia { get; set; }
        public decimal Precio { get; set; }

    }
}
