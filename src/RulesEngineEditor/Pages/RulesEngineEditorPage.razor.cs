using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using System.IO;

using System.Text.Json;
using System.Text.Json.Serialization;
using RulesEngineEditor.Shared;
using RulesEngineEditor.Models;
using System.Dynamic;
using RulesEngine.Models;
using RulesEngineEditor.Services;

namespace RulesEngineEditor.Pages
{
    partial class RulesEngineEditorPage : ComponentBase
    {
        private bool ShowWorkflows { get; set; } = true;
        public Dictionary<string, object> DownloadAttributes { get; set; }
        public Dictionary<string, object> DownloadInputAttributes { get; set; }

        JsonSerializerOptions jsonOptions;

        private RulesEngine.RulesEngine _rulesEngine = new RulesEngine.RulesEngine(null, null);
        [Parameter]
        public RulesEngine.RulesEngine EditorRulesEngine { get { return _rulesEngine; } set { _rulesEngine = value; } }

        [Parameter]
        public WorkflowRules[] Workflows { get; set; }

        [Parameter]
        public EventCallback<WorkflowRules[]> WorkflowsChanged { get; set; }

        [Parameter]
        public List<WorkflowData> WorkflowDatas { get { return WorkflowService.Workflows; } set { WorkflowService.Workflows = value; WorkflowUpdate(); } }

        [Parameter]
        public EventCallback<List<WorkflowData>> WorkflowDatasChanged { get; set; }

        [Parameter]
        public EventCallback<RulesEngine.RulesEngine> OnRulesEngineInitialize { get; set; }


        string workflowJSONErrors;
        string _workflowJSON;
        string WorkflowJSON { get { return _workflowJSON; } set { _workflowJSON = value; WorkflowJSONChange(); } }

        string inputJSONErrors;
        string _inputJSON;
        [Parameter]
        public string InputJSON { get { return _inputJSON; } set { _inputJSON = value; InputJSONUpdate(); RunRE(UpdateWorkflows: false); } }

        protected override async Task OnInitializedAsync()
        {
            jsonOptions = new JsonSerializerOptions {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                IncludeFields = true,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true
            };

            WorkflowService.OnInputChange += InputUpdate;
            WorkflowService.OnWorkflowChange += WorkflowUpdate;
            await base.OnInitializedAsync();
        }
        protected override void OnParametersSet()
        {
            if (Workflows != null)
            {
                WorkflowJSON = System.Text.Json.JsonSerializer.Serialize(Workflows, jsonOptions);
            }
        }

        public void Dispose()
        {
            WorkflowService.OnInputChange -= InputUpdate;
            WorkflowService.OnWorkflowChange -= WorkflowUpdate;
        }

        private void DeleteWorkflow(WorkflowData workflow)
        {
            WorkflowService.Workflows.Remove(workflow);
            WorkflowService.WorkflowUpdate();
        }
        void DeleteInput(InputRuleParameter input)
        {
            WorkflowService.Inputs.Remove(input);
            WorkflowService.WorkflowUpdate();
        }
        void UpdateInputDelete(InputRuleParameter input)
        {
            WorkflowService.Inputs.Remove(input);
        }
        private void NewWorkflows()
        {
            WorkflowService.Workflows = new List<WorkflowData>();
            WorkflowService.RuleParameters = new RuleParameter[0];
            StateHasChanged();
        }

        private void AddWorkflow()
        {
            WorkflowData workflow = new WorkflowData();
            workflow.GlobalParams = new List<ScopedParamData>();
            workflow.Rules = new List<RuleData>();
            WorkflowService.Workflows.Insert(0, workflow);
            StateHasChanged();
        }
        
        private void NewInputs()
        {
            WorkflowService.Inputs = new List<InputRuleParameter>();
            StateHasChanged();
        }

        private void AddInput()
        {
            InputRuleParameter input = new InputRuleParameter();
            input.InputRule = $"Input{WorkflowService.Inputs.Count + 1}";
            InputParameter parameter = new InputParameter();
            parameter.Name = "param1";
            input.Parameter.Add(parameter);
            WorkflowService.Inputs.Insert(0, input);
            StateHasChanged();
        }


        private void WorkflowUpdate()
        {
            DownloadFile();
            UpdateInputs();
            DownloadInputs();
            RunRE();
            StateHasChanged();
        }

        private void InputUpdate()
        {
            UpdateInputs();
            DownloadInputs();
            RunRE(UpdateWorkflows: false);
            StateHasChanged();
        }

        private void UpdateInputs()
        {
            var serializationOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };

            inputJSONErrors = "";
            List<InputRuleParameterDictionary> newInputs = new List<InputRuleParameterDictionary>();
            WorkflowService.Inputs.ForEach(i => {
                InputRuleParameterDictionary newInput = new InputRuleParameterDictionary();
                newInput.InputRule = i.InputRule;
                newInput.Parameter = new Dictionary<string, object>();
                foreach (var p in i.Parameter)
                {
                    try
                    {
                        newInput.Parameter.Add(p.Name, JsonSerializer.Deserialize<dynamic>(p.Value));
                    }
                    catch (Exception ex)
                    {
                        inputJSONErrors += ex.Message + " ";
                    }
                }
                newInputs.Add(newInput);
            });

            if (inputJSONErrors == "")
            {
                InputJSON = JsonSerializer.Serialize(newInputs, jsonOptions);
            }
        }

        private void RunRE(bool UpdateWorkflows = true)
        {
            try
            {
                var serializationOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };

                Workflows = JsonSerializer.Deserialize<WorkflowRules[]>(WorkflowJSON, serializationOptions);
                if (UpdateWorkflows)
                {
                    WorkflowsChanged.InvokeAsync(Workflows);
                    WorkflowDatasChanged.InvokeAsync(WorkflowService.Workflows);
                }

                if (WorkflowService.RuleParameters.Length == 0) return;

                _rulesEngine.ClearWorkflows();
                _rulesEngine.AddOrUpdateWorkflow(Workflows);

                WorkflowService.Workflows.ForEach(async workflow => {
                    List<RuleResultTree> resultList = await _rulesEngine.ExecuteAllRulesAsync(workflow.WorkflowName, WorkflowService.RuleParameters);

                    for (int i = 0; i < resultList.Count; i++)
                    {
                        var rule = workflow.Rules.FirstOrDefault(r => r.RuleName == resultList[i].Rule.RuleName);
                        rule.IsSuccess = resultList[i].IsSuccess;
                        if (!(bool)rule.IsSuccess)
                        {
                            rule.ExceptionMessage = resultList[i].ExceptionMessage;
                        }
                        else
                        {
                            rule.ExceptionMessage = "Rule was successful.";
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                workflowJSONErrors = ex.Message;
            }
            StateHasChanged();
        }

        private async void OnSubmit(InputFileChangeEventArgs files)
        {
            var selectedFile = files.File;
            StreamReader sr = new StreamReader(selectedFile.OpenReadStream());
            WorkflowJSON = JsonNormalizer.Normalize(await sr.ReadToEndAsync());

            WorkflowJSONChange();
        }

        private void WorkflowJSONChange()
        {
            workflowJSONErrors = "";
            try
            {
                WorkflowService.Workflows = JsonSerializer.Deserialize<List<WorkflowData>>(WorkflowJSON);
                RunRE(UpdateWorkflows: false);
            }
            catch (Exception ex)
            {
                workflowJSONErrors = ex.Message;
            }
            StateHasChanged();
        }

        private void DownloadFile()
        {
            workflowJSONErrors = "";
            var jsonString = JsonSerializer.Serialize(WorkflowService.Workflows, jsonOptions);
            if (jsonString == "[]")
            {
                return;
            }
            WorkflowJSON = JsonNormalizer.Normalize(jsonString);

            try
            {
                //ensure no serialzable errors in JSON before enabling download
                var re = new RulesEngine.RulesEngine(JsonSerializer.Deserialize<List<WorkflowRules>>(WorkflowJSON).ToArray());

                DownloadAttributes = new Dictionary<string, object>();
                DownloadAttributes.Add("href", "data:text/plain;charset=utf-8," + WorkflowJSON);
                DownloadAttributes.Add("download", "RulesEngine.json");
            }
            catch (Exception ex)
            {
                workflowJSONErrors = ex.Message;
            }
        }
        private async void ImportInputs(InputFileChangeEventArgs files)
        {
            var selectedFile = files.File;
            StreamReader sr = new StreamReader(selectedFile.OpenReadStream());
            InputJSON = await sr.ReadToEndAsync();
            InputJSONUpdate();
            ShowWorkflows = true;
            WorkflowService.WorkflowUpdate();
        }

        private void InputJSONUpdate()
        {
            inputJSONErrors = "";
            try
            {


                var inputs = JsonSerializer.Deserialize<dynamic>(InputJSON);

                WorkflowService.Inputs = new List<InputRuleParameter>();

                List<RuleParameter> ruleParameters = new List<RuleParameter>();
                foreach (var i in inputs.EnumerateArray())
                {
                    var key = i.GetProperty("InputRule").GetString();
                    var value = i.GetProperty("Parameter");

                    InputRuleParameter input = new InputRuleParameter();
                    input.InputRule = key;
                    input.Parameter = new List<InputParameter>();

                    var values = JsonSerializer.Deserialize<dynamic>(
                        JsonSerializer.Serialize(value), new JsonSerializerOptions {
                            Converters = { new DynamicJsonConverter() }
                        });

                    foreach (KeyValuePair<string, object> v in values)
                    {
                        InputParameter param = new InputParameter();
                        param.Name = v.Key;
                        param.Value = JsonSerializer.Serialize(v.Value);

                        input.Parameter.Add(param);
                    }
                    WorkflowService.Inputs.Add(input);
                    ruleParameters.Add(new RuleParameter(key, values));
                }
                WorkflowService.RuleParameters = ruleParameters.ToArray();
            }
            catch (Exception ex)
            {
                inputJSONErrors = ex.Message;
            }
        }

        private void DownloadInputs()
        {
            DownloadInputAttributes = new Dictionary<string, object>();
            DownloadInputAttributes.Add("href", "data:text/plain;charset=utf-8," + JsonNormalizer.Normalize(InputJSON));
            DownloadInputAttributes.Add("download", "RulesEngineInputs.json");
        }
    }
}
