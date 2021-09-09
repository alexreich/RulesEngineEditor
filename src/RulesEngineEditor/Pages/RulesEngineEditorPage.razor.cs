using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using System.IO;

//TODO switch to System.Text.Json when it supports polymorphic deserialization w/out needing converters
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
            jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                IncludeFields = true,
                IgnoreReadOnlyFields = true,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
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

            //if (InputJSON != "[]")
            //{
            //    InputJSONUpdate();
            //}
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
        void DeleteInput(Input input)
        {
            WorkflowService.Inputs.Remove(input);
            WorkflowService.WorkflowUpdate();
        }
        void UpdateInputDelete(Input input)
        {
            WorkflowService.Inputs.Remove(input);
        }
        private void NewWorkflow()
        {
            WorkflowService.Workflows = new List<WorkflowData>();
            WorkflowService.WorkflowUpdate();
        }

        private void AddWorkflow()
        {
            WorkflowData workflow = new WorkflowData();
            workflow.GlobalParams = new List<ScopedParamData>();
            workflow.Rules = new List<RuleData>();
            WorkflowService.Workflows.Insert(0, workflow);
            StateHasChanged();
        }

        private void NewInput()
        {
            Input input = new Input();
            input.Parameter = new List<InputParam>();
            input.Parameter.Add(new InputParam());
            WorkflowService.Inputs.Insert(0, input);
            WorkflowService.WorkflowUpdate();
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
            inputJSONErrors = "";
            List<InputDict> newInputs = new List<InputDict>();
            WorkflowService.Inputs.ForEach(i =>
            {
                InputDict newInput = new InputDict();
                newInput.InputName = i.InputName;
                newInput.Parameter = new Dictionary<string, object>();
                foreach (var p in i.Parameter)
                {
                    try
                    {
                        newInput.Parameter.Add(p.Name, JsonConvert.DeserializeObject<dynamic>(p.Value, new ExpandoObjectConverter()));
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
                InputJSON = System.Text.Json.JsonSerializer.Serialize(newInputs, jsonOptions);
            }
        }

        private void RunRE(bool UpdateWorkflows = true)
        {
            try
            {
                var serializationOptions = new System.Text.Json.JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };

                Workflows = System.Text.Json.JsonSerializer.Deserialize<WorkflowRules[]>(WorkflowJSON, serializationOptions);
                if (UpdateWorkflows)
                {
                    WorkflowsChanged.InvokeAsync(Workflows);
                }

                if (WorkflowService.RuleParameters.Length == 0) return;

                _rulesEngine.ClearWorkflows();
                _rulesEngine.AddOrUpdateWorkflow(Workflows);

                WorkflowService.Workflows.ForEach(async workflow =>
                {
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
                //inputJSONErrors = ex.Message;
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
                WorkflowService.Workflows = JsonConvert.DeserializeObject<List<WorkflowData>>(WorkflowJSON);
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
            var jsonString = System.Text.Json.JsonSerializer.Serialize(WorkflowService.Workflows, jsonOptions);
            if (jsonString == "[]")
            {
                return;
            }
            WorkflowJSON = JsonNormalizer.Normalize(jsonString);

            try
            {
                var re = new RulesEngine.RulesEngine(System.Text.Json.JsonSerializer.Deserialize<List<WorkflowRules>>(WorkflowJSON).ToArray());

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
                var inputs = JsonConvert.DeserializeObject<dynamic>(InputJSON);

                var converter = new ExpandoObjectConverter();
                WorkflowService.Inputs = new List<Input>();

                List<RuleParameter> ruleParameters = new List<RuleParameter>();
                foreach (var i in inputs)
                {
                    var key = Convert.ToString(i.InputName);
                    var value = Convert.ToString(i.Parameter);

                    Input input = new Input();
                    input.InputName = key;
                    input.Parameter = new List<InputParam>();

                    var values = JsonConvert.DeserializeObject<ExpandoObject>(value, converter);

                    foreach (KeyValuePair<string, object> v in values)
                    {
                        InputParam param = new InputParam();
                        param.Name = v.Key;
                        if (v.Value is string)
                        {
                            param.Value = @"""" + v.Value.ToString() + @"""";
                        }
                        else
                        {
                            param.Value = v.Value.ToString();
                        }

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
            var jsonString = System.Text.Json.JsonSerializer.Serialize(InputJSON, jsonOptions);

            DownloadInputAttributes = new Dictionary<string, object>();
            DownloadInputAttributes.Add("href", "data:text/plain;charset=utf-8," + JsonNormalizer.Normalize(jsonString));
            DownloadInputAttributes.Add("download", "RulesEngineInputs.json");
        }
    }
}
