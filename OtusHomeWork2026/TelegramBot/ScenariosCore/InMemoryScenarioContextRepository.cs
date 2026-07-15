using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.TelegramBot.Scenarios
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        Dictionary<long, ScenarioContext> _scenarioContextRepository;
        public InMemoryScenarioContextRepository()
        {
            _scenarioContextRepository = new Dictionary<long, ScenarioContext>();
        }

        public async Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            if(_scenarioContextRepository.ContainsKey(userId))
                return _scenarioContextRepository[userId];
            else
                return null;
        }

        public async Task ResetContext(long userId, CancellationToken ct)
        {
            _scenarioContextRepository.Remove(userId);
        }

        public async Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _scenarioContextRepository[userId]=context;
        }
    }
}
