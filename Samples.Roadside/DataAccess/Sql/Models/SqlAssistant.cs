namespace Samples.Roadside.DataAccess.Sql.Models;

public class SqlAssistant
{
    public string Id { get; set; }
    public string Name { get; set; }

    // Normally would likely be elsewhere, however again for purposes of this excercise and simplicity, keeping them here
    // Lat/Long of the "base" location (i.e. business, home, parking spot, etc.) for this assistant - where the truck
    // is parked or available to be dispatched from when not out roaming. Likely would be considered optional.
    public double BaseLatitude { get; set; }
    public double BaseLongitude { get; set; }

    // This might normally be a more complex type like a status or similar, but for purposes of this
    // excercise, just a bool flag
    public bool IsAvailable { get; set; }
}
