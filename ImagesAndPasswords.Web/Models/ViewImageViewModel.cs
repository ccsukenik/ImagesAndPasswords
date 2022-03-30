using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImagesAndPasswords.Data;

namespace ImagesAndPasswords.Web.Models
{
    public class ViewImageViewModel
    {
        public bool HasPermissionToView { get; set; }
        public Image Image { get; set; }
        public string Message { get; set; }
    }
}
