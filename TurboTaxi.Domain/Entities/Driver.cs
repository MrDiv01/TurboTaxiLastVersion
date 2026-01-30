using System;
using System.Collections.Generic;
using System.Text;
using TurboTaxi.Domain.BaseEntities;
using TurboTaxi.Domain.Enums;

namespace TurboTaxi.Domain.Entities
{
    public class Driver : BaseEntity
    {
        public int UserId { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool IsAvailable { get; set; } = false;

        public int? VehicleId { get; set; }

        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public User User { get; set; } = null!;
        public Vehicle? Vehicle { get; set; }
        public ICollection<Ride> Rides { get; set; } = new List<Ride>();
        public ICollection<DriverStatusHistory> StatusHistory { get; set; } = new List<DriverStatusHistory>();
        public ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>();
    }

}
