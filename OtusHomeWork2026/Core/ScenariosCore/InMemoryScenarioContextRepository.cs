using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.ScenariosCore
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        ConcurrentDictionary<long, ScenarioContext> _scenarioContextRepository;
        public InMemoryScenarioContextRepository()
        {
            _scenarioContextRepository = new ConcurrentDictionary<long, ScenarioContext>();
        }

        public async Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            if (_scenarioContextRepository.ContainsKey(userId))
                return _scenarioContextRepository[userId];
            else
                return null;
        }

        public async Task ResetContext(long userId, CancellationToken ct)
        {
            _scenarioContextRepository.TryRemove(userId, out var sc);
        }

        public async Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _scenarioContextRepository[userId] = context;
        }
    }
}
