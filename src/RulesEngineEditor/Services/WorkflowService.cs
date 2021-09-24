// Copyright (c) Alex Reich.
// Licensed under the CC BY 4.0 License.

using RulesEngine.Models;
using RulesEngineEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RulesEngineEditor.Services
{
    public class WorkflowService
    {
        public List<InputRuleParameter> Inputs { get; set; } = new List<InputRuleParameter>();
        public RuleParameter[] RuleParameters { get; set; } = new RuleParameter[0];
        public List<WorkflowData> Workflows { get; set; } = new List<WorkflowData>();

        public event Action OnInputChange;
        public event Action OnWorkflowChange;

        public void InputUpdate()
        {
            OnInputChange.Invoke();
        }

        public void WorkflowUpdate()
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
            }
            ruleParent.Rules.Insert(0, rule);
            WorkflowUpdate();
        }

        public void Sort<T>(List<T> listToSort)
        {
            int x = 1;
            listToSort.ForEach(item => {
                ((dynamic)item).Seq = x++;
            });
        }

        public void DeleteRule(dynamic ruleParent, RuleData rule)
        {
            if (rule.LocalParams != null)
                rule.LocalParams.ToList().ForEach(lp => rule.LocalParams.Remove(lp));

            if (rule.Rules != null)
                rule.Rules.ToList().ForEach(r => DeleteRule(rule, r));

            if (ruleParent is List<RuleData>)
            {
                ruleParent.Remove(rule);
            }
            else
            {
                ruleParent.Rules.Remove(rule);
            }
        }
    }
}