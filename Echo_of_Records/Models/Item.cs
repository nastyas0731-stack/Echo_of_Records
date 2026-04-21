using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo_of_Records.Models
{
    public enum ItemType { Wax, ForgottenObject }

    public class Item
    {
        public RectangleF Bounds { get; set; }
        public ItemType Type { get; set; }
        public bool IsCollected { get; set; } = false;
        public int ObjectId { get; set; } // ID для визуализации (шляпа, шарф и т.д.)

        public Item(float x, float y, ItemType type, int id = 0)
        {
            Bounds = new RectangleF(x, y, 40, 40);
            Type = type;
            ObjectId = id;
        }
    }
}
