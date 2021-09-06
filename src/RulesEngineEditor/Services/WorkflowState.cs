using RulesEngine.Models;
using RulesEngineEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RulesEngineEditor.Services
{
    public class WorkflowState
    {
        public List<Input> Inputs { get; set; } = new List<Input>();

        public RuleParameter[] RuleParameters { get; set; } = new RuleParameter[0];

        public List<WorkflowData> Workflows { get; set; } = new List<WorkflowData>();

        public event Action OnWorkflowChange;

        public void Update()
        {
            OnWorkflowChange.Invoke();
        }

        public void NewRule(dynamic ruleParent)
        {
            RuleData rule = new RuleData();
            rule.LocalParams = new List<ScopedParamData>();
            if (ruleParent.Rules == null)
            {
                ruleParent.Rules = new List<RuleData>();
            }
            if (ruleParent.GetType() == typeof(RuleData))
            {
                ruleParent.Operator = "And";
                //ruleParent.RuleExpressionType = null;
            }
            ruleParent.Rules.Insert(0, rule);
            Update();
        }
    }
}
