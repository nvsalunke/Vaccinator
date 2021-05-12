using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;

namespace VaccinatorWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        static ITelegramBotClient _botClient;
        private HttpClient _httpClient;
        private readonly int _districtId = 363;


        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _botClient = new TelegramBotClient("1875939534:AAEHl1_b-F7e5KSNjOJ4TgfQnxW44wzqjog");
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                string date = DateTime.Today.ToString("dd-MM-yyyy");
                try
                {
                    var response = await _httpClient.GetAsync($"https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/calendarByDistrict?district_id={_districtId}&date={date}");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var vaccineCenters = JsonConvert.DeserializeObject<Response>(responseContent);
                    _botClient.StartReceiving();

                    vaccineCenters.centers.ForEach(center =>
                    {
                        if (center?.sessions?.Count > 0 && center.sessions.Any(s => s.available_capacity > 0))
                        {
                            SendMessageToChannel(center);
                        }
                    });

                    _logger.LogInformation("Sent message at: {time}", DateTimeOffset.Now);
                    _botClient.StopReceiving();
                }
                catch(Exception e)
                {
                    _logger.LogError(e.Message);
                }
                
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void SendMessageToChannel(Center center)
        {
            center.sessions.Where(s => s.available_capacity > 0).ToList().ForEach(async s =>
            {
                await _botClient.SendTextMessageAsync(
                                      chatId: -1001330823433,
                                      text: $"Age Group: {s.min_age_limit}+\nCenter: {center.name}\nPin Code: {center.pincode}\n " +
                                      $"Available Capacity: {s.available_capacity}\nVaccine: {s.vaccine}\n Address: {center.address}"
                                    );
            });
        }
    }
}
