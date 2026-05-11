using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo_of_Records.Models
{
    public class MemoryNote
    {
        public RectangleF Bounds { get; set; }
        public string Content { get; set; } // Текст записки
        public bool IsCollected { get; set; } = false;

        public MemoryNote(float x, float y, string content)
        {
            // Делаем записку размером чуть меньше книги, например 40x50
            Bounds = new RectangleF(x, y, 40, 50);
            Content = content;
        }
    }
}
