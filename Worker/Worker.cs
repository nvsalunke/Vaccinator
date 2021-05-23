using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
//using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using System.Runtime.Caching;

namespace VaccinatorWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        static ITelegramBotClient _botClient;
        private HttpClient _httpClient;
        private readonly int _districtId = 363;
        private readonly long _channelChatId = -1001330823433;
        private static MemoryCache _memoryCache;


        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _botClient = new TelegramBotClient("1875939534:AAEHl1_b-F7e5KSNjOJ4TgfQnxW44wzqjog");
            _httpClient = new HttpClient();
            _memoryCache = new MemoryCache("donkey");

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //await _botClient.SendTextMessageAsync(
            //                          chatId: _channelChatId,
            //                          text: $"Vaccinator started at: {DateTime.Now}");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    for (int i = 0; i < 1; i++) {
                        string date = DateTime.Today.AddDays(i).ToString("dd-MM-yyyy");
                        Processs(date);
                    }

                    _logger.LogInformation("Sent message at: {time}", DateTimeOffset.Now);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
                
                await Task.Delay(3000, stoppingToken);
                
            }

            await _botClient.SendTextMessageAsync(
                                      chatId: _channelChatId,
                                      text: $"Vaccinator stopped at: {DateTime.Now}");
            _logger.LogWarning($"Stopped at: {DateTime.Now}");
        }

        private async Task Processs(string date)
        {
            var response = await _httpClient.GetAsync($"https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/calendarByDistrict?district_id={_districtId}&date={date}");
            var responseContent = await response.Content.ReadAsStringAsync();
            var vaccineCenters = JsonConvert.DeserializeObject<Response>(responseContent);
            var policy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            vaccineCenters.centers.ForEach(center =>
            {
                if (center?.sessions?.Count > 0)
                {
                    foreach (var s in center.sessions.Where(s => s.min_age_limit == 18 && s.available_capacity_dose1 > 0))
                    {
                        if (_memoryCache.Get(s.session_id) == null)
                        {
                            _memoryCache.Add(new CacheItem(s.session_id, s.session_id), policy);
                            SendMessageToChannel(center, s);
                        }
                    }
                }
            });
        }

        private void SendMessageToChannel(Center center, Session session)
        {
            _botClient.SendTextMessageAsync(
                                      chatId: _channelChatId,
                                      text: $"Age Group: {session.min_age_limit}+\nFee: {center.fee_type + " - " + center.vaccine_fees.FirstOrDefault()?.fee}\nCenter: {center.name}\nPin Code: {center.pincode}\nDate: {session.date}" +
                                      $"\nVaccine: {session.vaccine}\nAvailability - Dose 1: {session.available_capacity_dose1}\nAvailability - Dose 2: {session.available_capacity_dose2}"
                                    );
        }
    }
}
