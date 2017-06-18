using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Patcher3
{
    public class Patcher
    {
        public struct LoopDistance
        {
            public bool isLooping;
            public int loopStart;
            public int loopEnd;
            public int currentIteration;
            public string iteratorString;

            public LoopDistance(bool isLooping, int loopStart = 0, int loopEnd = 0, int currentIteration = 0, string iteratorString = "i")
            {
                this.isLooping = isLooping;
                this.loopStart = loopStart;
                this.loopEnd = loopEnd;
                this.currentIteration = currentIteration;
                this.iteratorString = iteratorString;
            }
        }

        public async static void PatchFromFile(string file, bool verbose)
        {
            StreamReader sr = new StreamReader(Path.GetFullPath(file));
            string versionInfo = sr.ReadLine();
            if (versionInfo != "pat3")
            {
                Console.WriteLine("file is not marked as pat3. exiting.");
                return;
            }
            List<string> metaData = Regex.Matches(sr.ReadLine(), @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            Dictionary<string, AssemblyDefinition> assemblyReferenceList = new Dictionary<string, AssemblyDefinition>();
            Dictionary<string, TypeDefinition> typeList = new Dictionary<string, TypeDefinition>();
            Dictionary<string, MethodDefinition> methodList = new Dictionary<string, MethodDefinition>();
            Dictionary<string, MethodReference> methodReferenceList = new Dictionary<string, MethodReference>();
            Dictionary<string, Instruction> instructionList = new Dictionary<string, Instruction>();
            Dictionary<string, ILProcessor> processorList = new Dictionary<string, ILProcessor>();
            Dictionary<string, int> variableList = new Dictionary<string, int>();

            string currentLine = "";
            List<string> lineData;
            Console.WriteLine("");
            PatchActions patchActions = new PatchActions();
            patchActions.verbose = verbose;
            LoopDistance isLooping = new LoopDistance(false);
            currentLine = sr.ReadLine();
            while (currentLine != null)
            {
                lineData = Regex.Matches(currentLine, @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();
                if (currentLine == "") { currentLine = sr.ReadLine(); continue; }
                isLooping = ParseLine(patchActions, true, isLooping, lineData);
                currentLine = sr.ReadLine();
                if (isLooping.isLooping)
                {
                    lineData = Regex.Matches(currentLine, @"[\""].+?[\""]|[^ ]+").Cast<Match>().Select(m => m.Value).ToList();
                    while (isLooping.isLooping)
                    {
                        isLooping = ParseLine(patchActions, true, isLooping, lineData);
                    }
                    currentLine = sr.ReadLine();
                }
            }
        }

        public static LoopDistance ParseLine(PatchActions patchActions, bool verbose, LoopDistance isLooping, List<string> lineData)
        {
            if (isLooping.isLooping)
            {
                patchActions.SetVariable(isLooping.iteratorString, isLooping.currentIteration);
            }
            switch (lineData[0])
            {
                case "loadassembly": //load main assembly
                    patchActions.LoadAssembly(lineData[1]);
                    break;
                case "loadassemblyreference": //load assembly reference
                    patchActions.LoadAssemblyReference(lineData[1], lineData[2]);
                    break;
                case "writeassembly": //write assembly
                    if (lineData.Count == 1)
                    {
                        patchActions.WriteAssembly(true);
                    }
                    else
                    {
                        patchActions.WriteAssembly(lineData[1], true);
                    }
                    break;
                case "writeassemblynoclose": //write assembly without close
                    if (lineData.Count == 1)
                    {
                        patchActions.WriteAssembly(false);
                    }
                    else
                    {
                        patchActions.WriteAssembly(lineData[1], false);
                    }
                    break;
                case "settype": //set type
                    patchActions.SetType(lineData[1], lineData[2], lineData[3]);
                    break;
                case "setfield": //set field
                    patchActions.SetField(lineData[1], lineData[2], lineData[3]);
                    break;
                case "setmethod": //set method
                    patchActions.SetMethod(lineData[1], lineData[2], lineData[3]);
                    break;
                case "setmethodreference": //set method reference
                    patchActions.SetMethodReference(lineData[1], lineData[2], lineData[3], lineData[4]);
                    break;
                case "loop": //loop (only add field with variable is supported atm)
                    string varName = lineData[3];
                    if (isLooping.isLooping == true)
                    {
                        if (verbose) { Console.WriteLine("nested loops are not supported."); }
                    } else
                    {
                        return new LoopDistance(true, int.Parse(lineData[1]), int.Parse(lineData[2]), int.Parse(lineData[1]), lineData[3]);
                    }
                    //if (verbose) { Console.WriteLine("finished loop (" + lineData[1] + "-" + lineData[2] + ")"); }
                    break;
                case "addfieldwithvariable": //add field with variable
                    patchActions.Loop_AddFieldWithVariable(lineData[1], lineData[2], lineData[3], lineData[4]);
                    break;
                case "settypeproperty":
                    patchActions.SetTypeProperty(lineData[1], lineData[2], lineData[3]);
                    break;
                case "setmethodproperty":
                    patchActions.SetMethodProperty(lineData[1], lineData[2], lineData[3]);
                    break;
                case "setfieldproperty":
                    patchActions.SetFieldProperty(lineData[1], lineData[2], lineData[3]);
                    break;
                case "setinstruction": //set instruction from number or !f/!l
                    patchActions.SetInstruction(lineData[1], lineData[2], lineData[3]);
                    break;
                case "runinstruction": //create instruction from il and run
                    patchActions.SetRunInstruction(lineData[1], lineData[2], lineData[3], lineData[4], lineData[5], lineData.Count < 7 ? "" : lineData[6]);
                    break;
                case "cmd":
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/C " + lineData[1].Replace("`","\"").Replace("$cdir", AppDomain.CurrentDomain.BaseDirectory);
                    process.StartInfo = startInfo;
                    process.Start();
                    break;
                case "#":
                    if (verbose)
                    {
                        Console.WriteLine(string.Join(" ", lineData.Skip(1)));
                    }
                    break;
            }
            if (isLooping.isLooping == false || isLooping.currentIteration >= isLooping.loopEnd)
            {
                return new LoopDistance(false);
            } else
            {
                isLooping.currentIteration++;
                return isLooping;
            }
        }

        public struct PatMetadata
        {
            public string name;
            public string author;
        }

        public static PatMetadata GetMetadata(string filename)
        {
            string filedata = File.ReadLines(filename).Skip(1).Take(1).First();
            PatMetadata metadata = new PatMetadata();
            if (filedata == "nometadata" || filedata == "nometadata ")
            {
                metadata.name = Path.GetFileName(filename);
                metadata.author = "unknown";
            } else {
                metadata.name = filedata.Split('|')[0];
                metadata.author = filedata.Split('|')[1];
            }
            return metadata;
        }
    }
}
