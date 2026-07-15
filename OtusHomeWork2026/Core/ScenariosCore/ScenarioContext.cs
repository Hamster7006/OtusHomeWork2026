using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusHomeWork2026.Core.ScenariosCore
{
    public enum ScenarioType
    {
        None,
        Add
    }
    public enum ScenarioResult
    {
        Transition, //- Переход к следующему шагу. Сообщение обработано, но сценарий еще не завершен
        Completed   // - Сценарий завершен
    }
    public class ScenarioContext
    {
        internal ScenarioType currentScenario { get; set; }
        internal string? CurrentStep { get; set; }
        internal Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public ScenarioContext(ScenarioType scenario)
        {
            currentScenario = scenario;
        }
    }
}
