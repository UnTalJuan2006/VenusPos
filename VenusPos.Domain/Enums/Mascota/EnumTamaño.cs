using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VenusPos.Domain.Enums.Mascota
{
    public enum EnumTamaño
    {
        [Display(Name = "Pequeño")]
        Pequeno,
        Mediano,
        Grande
    }
}
