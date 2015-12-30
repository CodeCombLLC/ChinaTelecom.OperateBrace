using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ChinaTelecom.OperateBrace.Models
{
    public class Building
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }

        public int TopLayers { get; set; }

        public int BottomLayers { get; set; }

        public string Doors { get; set; }

        [NotMapped]
        private Dictionary<int, int> doorCount;

        [NotMapped]
        public virtual Dictionary<int, int> DoorCount
        {
            get
            {
                if (doorCount == null)
                    doorCount = JsonConvert.DeserializeObject<Dictionary<int, int>>(Doors);
                return doorCount;
            }
        }

        public void SetDoors(Dictionary<int, int> doors)
        {
            var max = doors.Max(x => x.Key);
            for (var i = 1; i <= max; i++)
                if (!doors.ContainsKey(i))
                    doors[i] = 2;
            Doors = JsonConvert.SerializeObject(doors);
        }

        public int Units { get; set; }

        [ForeignKey("Estate")]
        public Guid EstateId { get; set; }

        public virtual Estate Estate { get; set; }

        public virtual ICollection<House> Houses { get; set; } = new List<House>();
    }
}
