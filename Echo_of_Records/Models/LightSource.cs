using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Echo_of_Records.Models
{
    // Этот класс просто хранит координаты нашей магической свечи
    public class LightSource
    {
        public float X { get; set; }
        public float Y { get; set; }

        public LightSource(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}