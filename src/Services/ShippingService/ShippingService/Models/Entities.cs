using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShippingService.Models;

public class Region
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<ShipperRegion> ShipperRegions { get; set; } = new List<ShipperRegion>();
}

public class ShipperRegion
{
    public int Id { get; set; }

    public int Region_Id { get; set; } // FK -> Region
    public int Shipper_Id { get; set; } // FK -> Shipper

    public bool Active { get; set; } = true;

    public Region? Region { get; set; }
    public Shipper? Shipper { get; set; }
}

public class Shipper
{
    public int Id { get; set; }

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string EmailId { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Contact_Person { get; set; } = string.Empty;

    public ICollection<ShipperRegion> Regions { get; set; } = new List<ShipperRegion>();
    public ICollection<ShippingDetail> Shipments { get; set; } = new List<ShippingDetail>();
}

public enum ShippingStatus
{
    Pending = 0,
    Shipped = 1,
    Delivered = 2,
    Cancelled = 3,
}

public class ShippingDetail
{
    public int Id { get; set; }

    public int Order_Id { get; set; } // comes from Order microservice
    public int Shipper_Id { get; set; } // FK -> Shipper

    public ShippingStatus Shipping_Status { get; set; } = ShippingStatus.Pending;

    [MaxLength(100)]
    public string? Tracking_Number { get; set; }

    public Shipper? Shipper { get; set; }
}
