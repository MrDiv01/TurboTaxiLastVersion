using Microsoft.Extensions.Logging;
using TurboTaxi.Application.Interfaces;

namespace TurboTaxi.Infrastructure.Services
{
    public class RussianCityResolver : ICityResolver
    {
        private readonly ILogger<RussianCityResolver> _logger;

        public RussianCityResolver(ILogger<RussianCityResolver> logger)
        {
            _logger = logger;
        }

        private static readonly Dictionary<string, string> CityMappings = new(StringComparer.OrdinalIgnoreCase)
        {
            // Moscow variants
            { "Москва", "moscow" },
            { "Moscow", "moscow" },
            { "Moskva", "moscow" },
            { "Moskwa", "moscow" },
            
            // Saint Petersburg variants
            { "Санкт-Петербург", "saint_petersburg" },
            { "Saint Petersburg", "saint_petersburg" },
            { "Sankt-Peterburg", "saint_petersburg" },
            { "St Petersburg", "saint_petersburg" },
            { "SPb", "saint_petersburg" },
            { "Петербург", "saint_petersburg" },
            { "Leningrad", "saint_petersburg" },
            
            // Yekaterinburg
            { "Екатеринбург", "yekaterinburg" },
            { "Yekaterinburg", "yekaterinburg" },
            { "Ekaterinburg", "yekaterinburg" },
            { "Sverdlovsk", "yekaterinburg" },
            
            // Novosibirsk
            { "Новосибирск", "novosibirsk" },
            { "Novosibirsk", "novosibirsk" },
            
            // Kazan
            { "Казань", "kazan" },
            { "Kazan", "kazan" },
            
            // Krasnoyarsk
            { "Красноярск", "krasnoyarsk" },
            { "Krasnoyarsk", "krasnoyarsk" },
            
            // Nizhny Novgorod
            { "Нижний Новгород", "nizhny_novgorod" },
            { "Nizhny Novgorod", "nizhny_novgorod" },
            { "Nizhniy Novgorod", "nizhny_novgorod" },
            { "Gorky", "nizhny_novgorod" },
            { "Gorki", "nizhny_novgorod" },
            
            // Chelyabinsk
            { "Челябинск", "chelyabinsk" },
            { "Chelyabinsk", "chelyabinsk" },
            
            // Ufa
            { "Уфа", "ufa" },
            { "Ufa", "ufa" },
            
            // Samara
            { "Самара", "samara" },
            { "Samara", "samara" },
            { "Kuybyshev", "samara" },
            
            // Rostov-on-Don
            { "Ростов-на-Дону", "rostov_on_don" },
            { "Rostov-on-Don", "rostov_on_don" },
            { "Rostov", "rostov_on_don" },
            
            // Krasnodar
            { "Краснодар", "krasnodar" },
            { "Krasnodar", "krasnodar" },
            
            // Omsk
            { "Омск", "omsk" },
            { "Omsk", "omsk" },
            
            // Voronezh
            { "Воронеж", "voronezh" },
            { "Voronezh", "voronezh" },
            
            // Perm
            { "Пермь", "perm" },
            { "Perm", "perm" },
            { "Molotov", "perm" },
            
            // Volgograd
            { "Волгоград", "volgograd" },
            { "Volgograd", "volgograd" },
            { "Stalingrad", "volgograd" },
            { "Tsaritsyn", "volgograd" },
            
            // Tyumen
            { "Тюмень", "tyumen" },
            { "Tyumen", "tyumen" },
            { "Tiumen", "tyumen" },
            
            // Saratov
            { "Саратов", "saratov" },
            { "Saratov", "saratov" },
            
            // Dagestan region
            { "Махачкала", "dagestan_avg" },
            { "Makhachkala", "dagestan_avg" },
            { "Дагестан", "dagestan_avg" },
            { "Dagestan", "dagestan_avg" },
            { "Дербент", "dagestan_avg" },
            { "Derbent", "dagestan_avg" },
            { "Каспийск", "dagestan_avg" },
            { "Kaspiysk", "dagestan_avg" },
            
            // Norilsk
            { "Норильск", "norilsk" },
            { "Norilsk", "norilsk" },
            { "Noril'sk", "norilsk" },
            { "городской округ Норильск", "norilsk" }
        };

        public string ResolveCityKey(string? cityName)
        {
            _logger.LogInformation("🔍 Resolving city name: '{CityName}'", cityName ?? "NULL");

            if (string.IsNullOrWhiteSpace(cityName))
            {
                _logger.LogWarning("⚠️ City name is null or empty, using default_ru");
                return "default_ru";
            }

            // Try exact match
            if (CityMappings.TryGetValue(cityName.Trim(), out var cityKey))
            {
                _logger.LogInformation("✅ Exact match found: '{CityName}' -> '{CityKey}'", cityName, cityKey);
                return cityKey;
            }

            // Try partial match (contains)
            foreach (var mapping in CityMappings)
            {
                if (cityName.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase) ||
                    mapping.Key.Contains(cityName, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("✅ Partial match found: '{CityName}' contains '{MappingKey}' -> '{CityKey}'", 
                        cityName, mapping.Key, mapping.Value);
                    return mapping.Value;
                }
            }

            // Default fallback
            _logger.LogWarning("❌ No match found for '{CityName}', using default_ru", cityName);
            return "default_ru";
        }
    }
}
