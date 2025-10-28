using BD_Assignment.Models.Responses;

namespace BD_Assignment.Services
{
    public interface IGeolocationService
    {
        Task<CountryInfoResponse?> GetCountryInfoAsync(string ipAddress);
    }
}
