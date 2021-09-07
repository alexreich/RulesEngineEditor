using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using System.IO;

//TODO switch to System.Text.Json when it supports polymorphic deserialization w/out needing converters
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using RulesEngineEditor.Shared;
using RulesEngineEditor.Models;
using System.Dynamic;
using RulesEngine.Models;
using RulesEngineEditor.Services;

namespace RulesEngineEditor.Pages
{
    //TODO: allow more elements to be styled
    //TODO: show ef on server demo
    partial class Workflows : ComponentBase
    {
        private bool ShowWorkflows { get; set; } = true;
        //[Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> DownloadAttributes { get; set; }
        public Dictionary<string, object> DownloadInputAttributes { get; set; }

        JsonSerializerOptions jsonOptions;

        private RulesEngine.RulesEngine _rulesEngine = new RulesEngine.RulesEngine(null, null);
        [Parameter]
        public RulesEngine.RulesEngine EditorRulesEngine { get { return _rulesEngine; } set { _rulesEngine = value; } }

        [Parameter]
        public WorkflowRules[] Workflow { get; set; }

        [Parameter]
        public EventCallback<WorkflowRules[]> WorkflowChanged { get; set; }

        [Parameter]
        public EventCallback<RulesEngine.RulesEngine> OnRulesEngineInitialize { get; set; }

        protected override async Task OnInitializedAsync()
        {
            jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                IncludeFields = true,
                IgnoreReadOnlyFields = true,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //Converters ={
                //    new JsonStringEnumConverter()
                //}
            };

            if (Workflow!=null)
            {
                workflowJSON = System.Text.Json.JsonSerializer.Serialize(Workflow, jsonOptions);
                Update();
            }

            WorkflowState.OnWorkflowChange += Update;
            //WFObj = await Task.Run(() => Workflowservice.GetAllWorkflowsAsync());

            //var foo = WFObj.First();
            //var bar = (WorkflowRules)foo;

            //var workflow = JsonSerializer.Deserialize<List<WorkflowRules>>(fileData);

            //RulesEngineDemoContext db = new RulesEngineDemoContext();
            //if (db.Database.EnsureCreated())
            //{
            //    db
            //    db.Workflows.AddRange(workflow);
            //    db.SaveChanges();
            //}
            await base.OnInitializedAsync();
        }
        public void Dispose()
        {
            WorkflowState.OnWorkflowChange -= Update;
        }

        void DeleteWorkflow(WorkflowData workflow)
        {
            WorkflowState.Workflows.Remove(workflow);
            WorkflowState.Update();
        }
        void DeleteInput(Input input)
        {
            WorkflowState.Inputs.Remove(input);
            WorkflowState.Update();
        }
        void DeleteRule(Rule rule)
        {
            //WorkflowState.Workflow.Remove(rule);
            //WorkflowState.Update();
        }
        void UpdateInputDelete(Input input)
        {
            WorkflowState.Inputs.Remove(input);
        }
        private void NewWorkflow()
        {
            WorkflowState.Workflows = new List<WorkflowData>();
            WorkflowState.Update();
        }

        private void AddWorkflow()
        {
            WorkflowData workflow = new WorkflowData();
            workflow.GlobalParams = new List<ScopedParamData>();
            workflow.Rules = new List<RuleData>();
            WorkflowState.Workflows.Insert(0, workflow);
            StateHasChanged();
        }

        private void NewInput()
        {
            Input input = new Input();
            input.Parameter = new List<InputParam>();
            input.Parameter.Add(new InputParam());
            WorkflowState.Inputs.Insert(0, input);
            WorkflowState.Update();
        }

        private void Update()
        {
            DownloadFile();
            UpdateInputs();
            DownloadInputs();
            RunREAsync();
            StateHasChanged();
        }

        private void UpdateInputs()
        {
            inputJSONErrors = "";
            List<NewInput> newInputs = new List<NewInput>();
            WorkflowState.Inputs.ForEach(i =>
            {
                NewInput newInput = new NewInput();
                newInput.InputName = i.InputName;
                newInput.Parameter = new Dictionary<string, object>();
                foreach (var p in i.Parameter)
                {
                    //if (p.Name != null && p.Value != null)
                    //{
                    try
                    {
                        newInput.Parameter.Add(p.Name, JsonConvert.DeserializeObject<dynamic>(p.Value, new ExpandoObjectConverter()));
                    }
                    catch (Exception ex)
                    {
                        inputJSONErrors += ex.Message + " ";
                    }
                    //}
                }
                newInputs.Add(newInput);
            });

            if (inputJSONErrors == "")
            {
                inputJSON = System.Text.Json.JsonSerializer.Serialize(newInputs, jsonOptions);
            }
        }

        private async Task RunREAsync()
        {
            try
            {

                //var workflowStr = JsonConvert.SerializeObject(workflowJSON);

                //var serializationOptions = new System.Text.Json.JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };

                ////TODO - move this out to demo
                //var bre = new RulesEngine.RulesEngine(System.Text.Json.JsonSerializer.Deserialize<List<WorkflowRules>>(workflowStr, serializationOptions).ToArray());

                //TODO - move this out to demo
                //var bre = new RulesEngine.RulesEngine(System.Text.Json.JsonSerializer.Deserialize<WorkflowRules[]>(workflowJSON).ToArray());

                //var workflowStr = JsonConvert.SerializeObject(workflowJSON);

                var serializationOptions = new System.Text.Json.JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };


                Workflow = System.Text.Json.JsonSerializer.Deserialize<WorkflowRules[]>(workflowJSON, serializationOptions);
                await WorkflowChanged.InvokeAsync(Workflow);

                if (WorkflowState.RuleParameters.Length == 0) return;

                _rulesEngine.ClearWorkflows();
                _rulesEngine.AddOrUpdateWorkflow(Workflow);


                //List<RuleParameter> ruleParameters = new List<RuleParameter>();
                //WorkflowState.Inputs.ForEach(i =>
                //{
                //    ruleParameters.Add(new RuleParameter(i.InputName, i.Parameter));
                //});

                WorkflowState.Workflows.ForEach(workflow =>
                {
                    List<RuleResultTree> resultList = _rulesEngine.ExecuteAllRulesAsync(workflow.WorkflowName, WorkflowState.RuleParameters).Result;

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
                //    var jsonString = System.Text.Json.JsonSerializer.Serialize(resultList, jsonOptions);

                //    resultsJSON = JsonNormalizer.Normalize(jsonString);
            }
            catch (Exception ex)
            {
                workflowJSONErrors = ex.Message;
                inputJSONErrors = ex.Message;
            }
            StateHasChanged();
        }

        private IBrowserFile selectedFile;
        private void LoadFiles(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
            this.StateHasChanged();
        }

        string workflowJSONErrors;
        string _workflowJSON;
        string workflowJSON { get { return _workflowJSON; } set { _workflowJSON = value; WorkflowJSONChange(); } }

        string inputJSONErrors;
        string _inputJSON;
        string inputJSON { get { return _inputJSON; } set { _inputJSON = value; InputJSONUpdate(); } }

        private async void OnSubmit(InputFileChangeEventArgs files)
        {

            var selectedFile = files.File;
            StreamReader sr = new StreamReader(selectedFile.OpenReadStream());
            //await using MemoryStream ms = new MemoryStream();

            //StreamReader sr = new StreamReader(stream);
            workflowJSON = JsonNormalizer.Normalize(await sr.ReadToEndAsync());

            WorkflowJSONChange();
        }

        //DateTime lastWorkflowJSONChange = DateTime.Now;
        private void WorkflowJSONChange()
        {
            workflowJSONErrors = "";
            try
            {
                //if (lastWorkflowJSONChange.AddSeconds(1) < DateTime.Now)
                //{
                //    return;
                //}
                //lastWorkflowJSONChange = DateTime.Now;
                //JsonSerializerOptions options = new JsonSerializerOptions();
                //options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Always;
                //options.IncludeFields = true;
                //var bv = JsonSerializer.Deserialize<List<WorkflowRules>>(foo, null);
                WorkflowState.Workflows = JsonConvert.DeserializeObject<List<WorkflowData>>(workflowJSON);
                //WorkflowState.Workflows = System.Text.Json.JsonSerializer.Deserialize<List<WorkflowData>>(workflowJSON);
                //WorkflowState.Update();
                //await Workflowservice.ImportWorkflowAsync(workflow);
            }
            catch (Exception ex)
            {
                workflowJSONErrors = ex.Message;
            }
            StateHasChanged();
        }

        ////[HttpGet("/download")]
        //private IActionResult Download()
        //{
        //    return new FileContentResult(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Workflowservice.GetAllWorkflowsAsync())), "application/force-download")
        //    {
        //        FileDownloadName = "RulesEngineWorkflow.json"
        //    };
        //}
        public void Download()
        {
            NavigationManager.NavigateTo($"/download", true);
        }

        public void DownloadFile()
        {
            workflowJSONErrors = "";
            var jsonString = System.Text.Json.JsonSerializer.Serialize(WorkflowState.Workflows, jsonOptions);
            if (jsonString == "[]")
            {
                return;
            }
            workflowJSON = JsonNormalizer.Normalize(jsonString);

            try
            {
                //TODO - move this out to demo
                var re = new RulesEngine.RulesEngine(System.Text.Json.JsonSerializer.Deserialize<List<WorkflowRules>>(workflowJSON).ToArray());

                DownloadAttributes = new Dictionary<string, object>();
                DownloadAttributes.Add("href", "data:text/plain;charset=utf-8," + workflowJSON);
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
            inputJSON = await sr.ReadToEndAsync();
            InputJSONUpdate();
            WorkflowState.Update();
        }

        private void InputJSONUpdate()
        {
            inputJSONErrors = "";
            try
            {
                var inputs = JsonConvert.DeserializeObject<dynamic>(inputJSON);

                var converter = new ExpandoObjectConverter();
                WorkflowState.Inputs = new List<Input>();

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
                    WorkflowState.Inputs.Add(input);
                    ruleParameters.Add(new RuleParameter(key, values));
                }
                WorkflowState.RuleParameters = ruleParameters.ToArray();
            }
            catch (Exception ex)
            {
                inputJSONErrors = ex.Message;
            }
            StateHasChanged();
        }

        public void DownloadInputs()
        {
            var jsonString = System.Text.Json.JsonSerializer.Serialize(inputJSON, jsonOptions);

            DownloadInputAttributes = new Dictionary<string, object>();
            DownloadInputAttributes.Add("href", "data:text/plain;charset=utf-8," + JsonNormalizer.Normalize(jsonString));
            DownloadInputAttributes.Add("download", "RulesEngineInputs.json");
        }
    }
}
