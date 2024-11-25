using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Steeltoe.Common.Util;
using System;
using System.Text;
using System.Threading.Tasks;
using static StackExchange.Redis.Role;

namespace RedisSample4.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CacheController : ControllerBase
    {
        private readonly IDistributedCache _cache;

        public CacheController(IDistributedCache cache)
        {
            _cache = cache;
        }

        // GET: api/cache/{key}
        [HttpGet("{key}")]
        public async Task<IActionResult> GetCacheValue(string key)
        {
            var cachedValue = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedValue))
            {
                return NotFound($"Key '{key}' not found in the cache.");
            }

            return Ok($"Cached Value for '{key}': {cachedValue}");
        }

        // POST: api/cache
        [HttpPost]
        public async Task<IActionResult> SetCacheValue([FromQuery] string key, [FromQuery] string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return BadRequest("Both 'key' and 'value' must be provided.");
            }

            // Set cache value with a 5-minute expiration
            await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return Ok($"Cached Value for '{key}' set to '{value}' (expires in 5 minutes).");
        }

        // DELETE: api/cache/{key}
        [HttpDelete("{key}")]
        public async Task<IActionResult> RemoveCacheValue(string key)
        {
            var cachedValue = await _cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(cachedValue))
            {
                return NotFound($"Key '{key}' not found in the cache.");
            }

            await _cache.RemoveAsync(key);

            return Ok($"Removed Key '{key}' from the cache.");
        }

        // GET: api/cache/set-default
        [HttpGet("set-default")]
        public async Task<IActionResult> SetDefaultCache()
        {
            string key = "SampleKey";
            string value = "Hello from Steeltoe with Redis Cache!";

            await _cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return Ok($"Default cache set: Key='{key}', Value='{value}'");
        }

        [HttpPost("setWithExpiration")]
        public async Task<IActionResult> SetCacheValueWithExpiration([FromQuery] string key, [FromQuery] string value, [FromQuery] int expirationInSeconds)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationInSeconds)
            };

            await _cache.SetStringAsync(key, value, options);
            return Ok($"Key '{key}' set to '{value}' in cache with expiration of {expirationInSeconds} seconds.");
        }

    }
}
/*
 * Get Cache Value (GET: api/cache/{key}):

Retrieves the value associated with the given key from the cache.
Returns a 404 Not Found response if the key does not exist.
Set Cache Value (POST: api/cache?key=value):

Adds or updates a value in the cache with a 5-minute expiration.
Requires both key and value as query parameters.
Remove Cache Value (DELETE: api/cache/{key}):

Removes the key-value pair from the cache.
Returns a 404 Not Found response if the key does not exist.
Set Default Cache (GET: api/cache/set-default):

Demonstrates setting a default key-value pair in the cache.
*/

/********************************Testing expiry ***************************************/
//Manual Testing with Postman
//Set Cache with Expiration:

//Endpoint: POST http://localhost:5000/api/cache/setWithExpiration?key=testKey&value=testValue&expirationInSeconds=10
//This sets the cache value with an expiration of 10 seconds.
//Retrieve Cache Before Expiration:

//Endpoint: GET http://localhost:5000/api/cache/get/testKey
//Make this call within 10 seconds to verify that the cache value is available.
//Retrieve Cache After Expiration:

//Wait for 10 seconds (or the expiration time you set).
//Make the same GET request again. You should see a "Key not found in cache." response.