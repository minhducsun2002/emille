#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Webhook;
using Emille.Models;
using Emille.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Emille.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        private readonly RecordService recordService;
        private readonly IWebhookClient? webhookClient;

        public DataController(RecordService recordService, IWebhookClientFactory clientFactory)
        {
            this.recordService = recordService;
            var webhook = Environment.GetEnvironmentVariable("LOG_WEBHOOK");
            if (!string.IsNullOrWhiteSpace(webhook))
                webhookClient = clientFactory.CreateClient(webhook);
        }
        
        [HttpGet("[action]")]
        public List<Record> List()
        {
            return recordService.Collection.FindSync(FilterDefinition<Record>.Empty).ToList();
        }
        
        [HttpGet("[action]/{key:minlength(1)}")]
        public ActionResult<Record>  Get(string key)
        {
            var result = recordService.Collection.FindSync(record => record.Key == key.ToLowerInvariant())
                .FirstOrDefault();
            if (result == default) return NotFound();
            return result;
        }
        
        [HttpPost("[action]/{key:minlength(1)}")]
        public async Task<ActionResult<Record>> Push(string key)
        {
            using var reader = new StreamReader(Request.Body);
            var value = await reader.ReadToEndAsync();
            if (value.Length == 0) return BadRequest();
            var overwriting = await recordService.Collection
                .CountDocumentsAsync<Record>(record => record.Key == key.ToLowerInvariant()) != 0;
                
            var result = recordService.Collection.FindOneAndReplace<Record>(
                record => record.Key == key.ToLowerInvariant(),
                new Record { Key = key.ToLowerInvariant(), Value = value, Timestamp = DateTime.Now },
                new FindOneAndReplaceOptions<Record> { IsUpsert = true, ReturnDocument = ReturnDocument.After }
            );
            
            webhookClient?.ExecuteAsync(new LocalWebhookMessage
            {
                Embeds = new List<LocalEmbed>
                {
                    new()
                    {
                        Description = overwriting ? $"Created key `{key}`." : $"Key `{key}` was updated.",
                        Timestamp = result.Timestamp,
                        Color = Color.LightGreen,
                        Footer = new LocalEmbedFooter().WithText($"IP : {HttpContext.Connection.RemoteIpAddress}")
                    }
                },
            })?.GetAwaiter().GetResult();
            
            return result;
        }

        [HttpDelete("[action]/{key:minlength(1)}")]
        public ActionResult<bool> Delete(string key)
        {
            var result = recordService.Collection.FindOneAndDelete<Record>(record => record.Key == key.ToLowerInvariant()) != null;
            webhookClient?.ExecuteAsync(new LocalWebhookMessage
            {
                Embeds = new List<LocalEmbed>
                {
                    new()
                    {
                        Description = result ? $"Key `{key}` was deleted." : $"Attempted to delete `{key}`, but no such key exists.",
                        Timestamp = DateTimeOffset.Now,
                        Color = result ? Color.Red : Color.Yellow,
                        Footer = new LocalEmbedFooter().WithText($"IP : {HttpContext.Connection.RemoteIpAddress}")
                    }
                },
            })?.GetAwaiter().GetResult();
            return result;
        }
    }
}