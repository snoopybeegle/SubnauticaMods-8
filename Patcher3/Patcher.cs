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
        //todo: cleanup arguments to actual variables
        public static void PatchFromFile(string file, bool verbose)
        {
            bool superVerbose = false;
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
            AssemblyDefinition assembly = null;

            string currentLine = "";
            List<string> lineData;
            Console.WriteLine("");
            while ((currentLine = sr.ReadLine()) != null)
            {
                lineData = Regex.Matches(currentLine, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();
                if (currentLine == "") { continue; }
                switch (lineData[0])
                {
                    case "loadassembly": //load main assembly
                        if (assembly != null)
                        {
                            assembly.Write(assembly.Name.Name + ".pat.dll");
                        }
                        assembly = AssemblyDefinition.ReadAssembly(lineData[1]);
                        if (verbose) { Console.WriteLine("loading main assembly " + lineData[1]); }
                        break;
                    case "loadassemblyreference": //load assembly reference
                        assemblyReferenceList[lineData[1]] = AssemblyDefinition.ReadAssembly(lineData[2]);
                        if (verbose) { Console.WriteLine("loading assembly reference " + lineData[2]); }
                        break;
                    case "writeassembly": //write assembly
                        if (verbose) { Console.WriteLine("writing to " + assembly.Name.Name + ".pat.dll"); }
                        assembly.Write(assembly.Name.Name + ".pat.dll");
                        if (verbose) { Console.WriteLine("done writing to " + assembly.Name.Name + ".pat.dll"); }
                        assembly = null;
                        break;
                    case "writeassemblynoclose": //write assembly without close
                        if (verbose) { Console.WriteLine("writing to " + assembly.Name.Name + ".pat.dll"); }
                        assembly.Write(assembly.Name.Name + ".pat.dll");
                        if (verbose) { Console.WriteLine("done writing to " + assembly.Name.Name + ".pat.dll"); }
                        break;
                    case "settype": //set type
                        if (lineData[2] == "!c")
                        {
                            typeList[lineData[1]] = assembly.MainModule.Types.Where(t => t.Name.Equals(lineData[3])).Select(t => t).First();
                            if (verbose) { Console.WriteLine("loading type " + lineData[3] + " from " + assembly.Name.Name); }
                        } else
                        {
                            typeList[lineData[1]] = assemblyReferenceList[lineData[2]].MainModule.Types.Where(t => t.Name.Equals(lineData[3])).Select(t => t).First();
                            if (verbose) { Console.WriteLine("loading type " + lineData[3] + " from " + lineData[2]); }
                        }
                        break;
                    case "setmethod": //set method
                        methodList[lineData[1]] = typeList[lineData[2]].Methods.Where(t => t.Name.Equals(lineData[3])).Select(t => t).First();
                        if (verbose) { Console.WriteLine("loading method " + lineData[3] + " from " + lineData[2]); }
                        break;
                    case "setmethodreference": //set method reference
                        methodReferenceList[lineData[1]] = methodList[lineData[2]].Module.Import(typeList[lineData[3]].Methods.Where(t => t.Name.Equals(lineData[4])).Select(t => t).First());
                        if (verbose) { Console.WriteLine("loading method reference " + lineData[1] + " from " + lineData[3]); }
                        break;
                    case "loop": //loop (PLEASE FIX FOR ALL COMMANDS)
                        string varName = lineData[3];
                        string nextCommand = sr.ReadLine();
                        List<string> lineData_nc;
                        lineData_nc = Regex.Matches(nextCommand, @"[\""].+?[\""]|[^ ]+")
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToList();
                        for (int i = int.Parse(lineData[1]); i < int.Parse(lineData[2]); i++)
                        {
                            variableList[varName] = i;
                            switch (lineData_nc[0])
                            {
                                case "addfieldwithvariable": //add field with variable
                                    List<string> fieldAttributes = lineData_nc[2].Split(new [] { "|" }, StringSplitOptions.None).ToList();
                                    List<string> variableData = lineData_nc[4].Split(new[] { "|" }, StringSplitOptions.None).ToList();
                                    string[] variableDataArgs = variableData.Skip(1).ToArray();
                                    for (int j = 0; j < variableDataArgs.Length; j++)
                                    {
                                        if (variableDataArgs[j].StartsWith("$"))
                                        {
                                            variableDataArgs[j] = variableList[lineData_nc[3].Substring(1)].ToString();
                                        }
                                    }
                                    FieldAttributes fa = 0; //acAfOmdMrilnIpPrsS
                                    foreach (string fieldAttrib in fieldAttributes)
                                    {
                                        fa |= (FieldAttributes)Enum.Parse(typeof(FieldAttributes), fieldAttrib);
                                    }
                                    //todo: replace with method

                                    FieldDefinition fd = new FieldDefinition(string.Format(variableData[0], variableDataArgs), fa, typeList[lineData_nc[1]]);
                                    int constant = 0;
                                    if (lineData_nc[3].StartsWith("$"))
                                    {
                                        constant = variableList[lineData_nc[3].Substring(1)];
                                    } else
                                    {
                                        constant = int.Parse(lineData_nc[3]);
                                    }
                                    if (!(typeList[lineData_nc[1]].Fields.Skip(1).Where(n => n.Constant.Equals(constant)).Any()))
                                    {
                                        if (verbose && superVerbose) { Console.WriteLine("adding " + string.Format(variableData[0], variableDataArgs) + " to " + lineData_nc[1] + " = " + constant); }
                                        typeList[lineData_nc[1]].Fields.Add(fd);
                                        fd.Constant = constant;
                                    }
                                    break;
                            }
                        }
                        if (verbose) { Console.WriteLine("finished loop (" + lineData[1] + "-" + lineData[2] + ")"); }
                        break;
                    case "setmethodproperty":
                        List<string> methodAttributes = lineData[2].Split(new[] { "|" }, StringSplitOptions.None).ToList();
                        List<string> methodData = lineData[3].Split(new[] { "|" }, StringSplitOptions.None).ToList();
                        //shouldn't be too bad right ( ͡o ͜ʖ ͡o)
                        for (int i = 0; i < methodAttributes.Count; i++)
                        {
                            methodList[lineData[1]].GetType().GetProperty(methodAttributes[i]).SetValue(methodList[lineData[1]], bool.Parse(methodData[i]), null);
                        }
                        break;
                    case "setfieldproperty":

                        break;
                    case "setinstruction": //set instruction from number or !f/!l
                        if (lineData[3] == "!f")
                        {
                            instructionList[lineData[1]] = methodList[lineData[2]].Body.Instructions.First();
                        } else if (lineData[3] == "!l")
                        {
                            instructionList[lineData[1]] = methodList[lineData[2]].Body.Instructions.Last();
                        } else
                        {
                            instructionList[lineData[1]] = methodList[lineData[2]].Body.Instructions[int.Parse(lineData[3])];
                        }
                        break;
                    case "runinstruction": //create instruction from il and run
                        ILProcessor processor_ciilr = methodList[lineData[1]].Body.GetILProcessor();
                        Instruction instruction_ciilr = null;
                        switch (lineData[5])
                        {
                            case "none":
                                instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(lineData[4]).GetValue(null));
                                break;
                            case "byte":
                                instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(lineData[4]).GetValue(null), byte.Parse(lineData[6]));
                                break;
                            case "double":
                                instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(lineData[4]).GetValue(null), double.Parse(lineData[6]));
                                break;
                            case "float":
                                instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(lineData[4]).GetValue(null), float.Parse(lineData[6]));
                                break;
                            case "instruction":
                                instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(lineData[4]).GetValue(null), instructionList[lineData[6]]);
                                break;
                            case "int":
                                instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(lineData[4]).GetValue(null), int.Parse(lineData[6]));
                                break;
                            case "long":
                                instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(lineData[4]).GetValue(null), long.Parse(lineData[6]));
                                break;
                            case "method":
                                instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(lineData[4]).GetValue(null), methodReferenceList[lineData[6]]);
                                break;
                            case "string":
                                instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(lineData[4]).GetValue(null), lineData[6]);
                                break;
                        }
                        if (verbose) { Console.WriteLine("creating instruction " + lineData[4]); }
                        if (lineData[2] == "b")
                        {
                            processor_ciilr.InsertBefore(processor_ciilr.Body.Instructions[int.Parse(lineData[3])], instruction_ciilr);
                        } else if (lineData[2] == "a")
                        {
                            processor_ciilr.InsertAfter(processor_ciilr.Body.Instructions[int.Parse(lineData[3])], instruction_ciilr);
                        } else if (lineData[2] == "ap")
                        {
                            processor_ciilr.Append(instruction_ciilr);
                        }
                        break;
                }
            }
        }
    }
}
