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
    //TODO: bubble up rulesengine init to caller
    //TODO: Update fields on workflow json change
    //TODO: Bug where success or error deselected and error msg appears
    partial class Workflows : ComponentBase
    {
        //[Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object> DownloadAttributes { get; set; }
        public Dictionary<string, object> DownloadInputAttributes { get; set; }

        JsonSerializerOptions jsonOptions;
        protected override async Task OnInitializedAsync()
        {
            jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                IncludeFields = true,
                IgnoreReadOnlyFields = true,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

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
            workflowJSON = "";
        }

        private void AddWorkflow()
        {
            WorkflowData workflow = new WorkflowData();
            workflow.GlobalParams = new List<ScopedParamData>();
            workflow.Rules = new List<RuleData>();
            WorkflowState.Workflows.Insert(0, workflow);
            WorkflowState.Update();
        }

        private void NewRule(WorkflowData workflow)
        {
            RuleData rule = new RuleData();
            rule.LocalParams = new List<ScopedParamData>();
            workflow.Rules.Insert(0, rule);
            WorkflowState.Update();
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
            DownloadInputs();
            RunRE();
            StateHasChanged();
        }

        private void RunRE()
        {
            try
            {
                //TODO - move this out to demo
                var bre = new RulesEngine.RulesEngine(System.Text.Json.JsonSerializer.Deserialize<List<WorkflowRules>>(workflowJSON).ToArray());

                //List<RuleParameter> ruleParameters = new List<RuleParameter>();
                //WorkflowState.Inputs.ForEach(i =>
                //{
                //    ruleParameters.Add(new RuleParameter(i.InputName, i.Parameter));
                //});


                WorkflowData workflow = WorkflowState.Workflows.First();
                List<RuleResultTree> resultList = bre.ExecuteAllRulesAsync(workflow.WorkflowName, WorkflowState.RuleParameters).Result;

                for (int i = 0; i < resultList.Count; i++)
                {
                    workflow.Rules[i].IsSuccess = resultList[i].IsSuccess;
                    if (!(bool)workflow.Rules[i].IsSuccess)
                    {
                        workflow.Rules[i].ExceptionMessage = resultList[i].ExceptionMessage;
                    }
                    else
                    {
                        workflow.Rules[i].ExceptionMessage = "Rule was successful.";
                    }
                }

                //    var jsonString = System.Text.Json.JsonSerializer.Serialize(resultList, jsonOptions);

                //    resultsJSON = JsonNormalizer.Normalize(jsonString);
            }
            catch (Exception ex)
            {
                inputJSON = ex.Message;
            }
            StateHasChanged();
        }

        private IBrowserFile selectedFile;
        private void LoadFiles(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
            this.StateHasChanged();
        }

        //List<WorkflowData> _workflow;
        //List<WorkflowData> workflow { get { return _workflow; } set { _workflow = value; DownloadFile(); } }
        string workflowJSON;
        string inputJSON;
        private async void OnSubmit(InputFileChangeEventArgs files)
        {

            var selectedFile = files.File;
            StreamReader sr = new StreamReader(selectedFile.OpenReadStream());
            //await using MemoryStream ms = new MemoryStream();

            //StreamReader sr = new StreamReader(stream);
            var foo = await sr.ReadToEndAsync();
            try
            {
                //JsonSerializerOptions options = new JsonSerializerOptions();
                //options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Always;
                //options.IncludeFields = true;
                //var bv = JsonSerializer.Deserialize<List<WorkflowRules>>(foo, null);
                WorkflowState.Workflows = JsonConvert.DeserializeObject<List<WorkflowData>>(foo);
                WorkflowState.Update();
                //await Workflowservice.ImportWorkflowAsync(workflow);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            this.StateHasChanged();
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
            var jsonString = System.Text.Json.JsonSerializer.Serialize(WorkflowState.Workflows, jsonOptions);

            workflowJSON = JsonNormalizer.Normalize(jsonString);

            //TODO - move this out to demo
            var re = new RulesEngine.RulesEngine(System.Text.Json.JsonSerializer.Deserialize<List<WorkflowRules>>(workflowJSON).ToArray());

            DownloadAttributes = new Dictionary<string, object>();
            DownloadAttributes.Add("href", "data:text/plain;charset=utf-8," + workflowJSON);
            DownloadAttributes.Add("download", "RulesEngine.json");
        }
        private async void ImportInputs(InputFileChangeEventArgs files)
        {

            var selectedFile = files.File;
            StreamReader sr = new StreamReader(selectedFile.OpenReadStream());
            inputJSON = await sr.ReadToEndAsync();
            InputJSONUpdate();
        }

        private void InputJSONUpdate()
        {
            try
            {
                var inputs = JsonConvert.DeserializeObject<dynamic>(inputJSON);

                var converter = new ExpandoObjectConverter();

                List<RuleParameter> ruleParameters = new List<RuleParameter>();
                foreach (var i in inputs)
                {
                    ruleParameters.Add(new RuleParameter(Convert.ToString(i.InputName), JsonConvert.DeserializeObject<ExpandoObject>(Convert.ToString(i.Parameter), converter)));
                }
                WorkflowState.RuleParameters = ruleParameters.ToArray();

                WorkflowState.Update();
            }
            catch (Exception ex)
            {
                inputJSON = ex.Message;
            }
            this.StateHasChanged();
        }

        private void InputJSONnChange(ChangeEventArgs args)
        {
            InputJSONUpdate();
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
