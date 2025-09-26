namespace ShippingService.DTO;

public record CreateShipperDto(string Name, string EmailId, string Phone, string Contact_Person);

public record UpdateShipperDto(
    int Id,
    string Name,
    string EmailId,
    string Phone,
    string Contact_Person
);

public record CreateShippingDetailDto(int Order_Id, int Shipper_Id, string? Tracking_Number);

public record UpdateShippingStatusDto(
    ShippingService.Models.ShippingStatus Status,
    string? Tracking_Number
);
