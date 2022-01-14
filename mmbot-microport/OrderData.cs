/// <param name="Price">price where order is put. If this field is 0, recommended price is used</param>
/// <param name="Size">size of the order, +buy, -sell. If this field is 0, the order is not placed</param>
/// <param name="Alert">set true, to put alert/dust order. This needs size equal to zero</param>
internal record OrderData(
    double Price,
    double Size,
    AlertType Alert = AlertType.Enabled
);