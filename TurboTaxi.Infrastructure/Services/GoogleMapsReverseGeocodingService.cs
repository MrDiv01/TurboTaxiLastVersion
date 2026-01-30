using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TurboTaxi.Application.Interfaces;

namespace TurboTaxi.Infrastructure.Services
{
    public class GoogleMapsReverseGeocodingService : IReverseGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleMapsReverseGeocodingService> _logger;
        private readonly string _apiKey;

        public GoogleMapsReverseGeocodingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleMapsReverseGeocodingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["GoogleMaps:ApiKey"] ?? throw new InvalidOperationException("GoogleMaps:ApiKey not configured");
        }

        public async Task<string?> GetCityFromCoordinatesAsync(double latitude, double longitude, CancellationToken ct = default)
        {
            try
            {
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={_apiKey}&language=ru";

                _logger.LogInformation("🌍 Google Maps API Request: {Url}", url);

                var response = await _httpClient.GetAsync(url, ct);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(ct);

                // LOG FULL RESPONSE (with proper encoding)
                _logger.LogInformation("📦 Google Maps API FULL Response: {Json}", json);

                var geocodeResponse = JsonSerializer.Deserialize<GoogleGeocodeResponse>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (geocodeResponse?.Results == null || geocodeResponse.Results.Length == 0)
                {
                    _logger.LogWarning("❌ No geocoding results for coordinates: {Lat}, {Lng}", latitude, longitude);
                    return null;
                }

                _logger.LogInformation("📍 Found {Count} results from Google Maps", geocodeResponse.Results.Length);

                // Try to find city from address components
                for (int i = 0; i < geocodeResponse.Results.Length; i++)
                {
                    var result = geocodeResponse.Results[i];

                    _logger.LogInformation("🔎 Examining Result #{Index}", i);

                    if (result.AddressComponents == null)
                    {
                        _logger.LogWarning("   No address components in this result");
                        continue;
                    }

                    // LOG ALL COMPONENTS with index
                    _logger.LogInformation("   📍 Address Components: {Count}", result.AddressComponents.Length);
                    for (int j = 0; j < result.AddressComponents.Length; j++)
                    {
                        var comp = result.AddressComponents[j];
                        _logger.LogInformation("      [{Index}] {Name} | Types: [{Types}]", 
                            j, 
                            comp.LongName ?? "NULL", 
                            string.Join(", ", comp.Types ?? Array.Empty<string>()));
                    }

                    // Try locality first (most specific)
                    foreach (var component in result.AddressComponents)
                    {
                        if (component.Types != null && component.Types.Contains("locality"))
                        {
                            _logger.LogInformation("✅ FOUND locality: {City}", component.LongName);
                            return component.LongName;
                        }
                    }

                    // Try administrative_area_level_2 (district/city)
                    foreach (var component in result.AddressComponents)
                    {
                        if (component.Types != null && component.Types.Contains("administrative_area_level_2"))
                        {
                            _logger.LogInformation("✅ FOUND administrative_area_level_2: {City}", component.LongName);
                            return component.LongName;
                        }
                    }

                    // Fallback to administrative_area_level_1 (region)
                    foreach (var component in result.AddressComponents)
                    {
                        if (component.Types != null && component.Types.Contains("administrative_area_level_1"))
                        {
                            _logger.LogInformation("⚠️ FOUND administrative_area_level_1 (fallback): {City}", component.LongName);
                            return component.LongName;
                        }
                    }
                }

                _logger.LogWarning("Could not extract city from geocoding response for: {Lat}, {Lng}", latitude, longitude);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during reverse geocoding for coordinates: {Lat}, {Lng}", latitude, longitude);
                return null;
            }
        }

        private class GoogleGeocodeResponse
        {
            [JsonPropertyName("results")]
            public GoogleGeocodeResult[]? Results { get; set; }

            [JsonPropertyName("status")]
            public string? Status { get; set; }
        }

        private class GoogleGeocodeResult
        {
            [JsonPropertyName("address_components")]
            public AddressComponent[]? AddressComponents { get; set; }
        }

        private class AddressComponent
        {
            [JsonPropertyName("long_name")]
            public string? LongName { get; set; }

            [JsonPropertyName("short_name")]
            public string? ShortName { get; set; }

            [JsonPropertyName("types")]
            public string[]? Types { get; set; }
        }
    }
}
