using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patcher3
{
    public class PatchActions
    {
        AssemblyDefinition assembly;
        public bool verbose = false;
        bool superVerbose = false;

        Dictionary<string, AssemblyDefinition> assemblyReferenceList = new Dictionary<string, AssemblyDefinition>();
        Dictionary<string, TypeDefinition> typeList = new Dictionary<string, TypeDefinition>();
        Dictionary<string, FieldDefinition> fieldList = new Dictionary<string, FieldDefinition>();
        Dictionary<string, MethodDefinition> methodList = new Dictionary<string, MethodDefinition>();
        Dictionary<string, MethodReference> methodReferenceList = new Dictionary<string, MethodReference>();
        Dictionary<string, Instruction> instructionList = new Dictionary<string, Instruction>();
        Dictionary<string, ILProcessor> processorList = new Dictionary<string, ILProcessor>();
        Dictionary<string, int> variableList = new Dictionary<string, int>();

        public void LoadAssembly(string fileName)
        {
            if (verbose) { Console.WriteLine("loading main assembly " + fileName); }
            if (assembly != null)
            {
                assembly.Write(assembly.Name.Name + ".pat.dll");
            }
            assembly = AssemblyDefinition.ReadAssembly(fileName);
        }
        
        public void LoadAssemblyReference(string assemblyVariable, string fileName)
        {
            if (verbose) { Console.WriteLine("loading assembly reference " + fileName); }
            assemblyReferenceList[assemblyVariable] = AssemblyDefinition.ReadAssembly(fileName);
        }

        public void WriteAssembly(bool close)
        {
            if (verbose) { Console.WriteLine("writing to " + assembly.Name.Name + ".pat.dll"); }
            assembly.Write(assembly.Name.Name + ".pat.dll");
            if (verbose) { Console.WriteLine("done writing to " + assembly.Name.Name + ".pat.dll"); }
            assembly = close ? null : assembly;
        }

        public void WriteAssembly(string fileName, bool close)
        {
            if (verbose) { Console.WriteLine("writing to " + fileName); }
            assembly.Write(fileName);
            if (verbose) { Console.WriteLine("done writing to " + fileName); }
            assembly = close ? null : assembly;
        }

        public void SetType(string typeVariable, string assemblyVariable, string typeName)
        {
            if (assemblyVariable == "!c")
            {
                if (verbose) { Console.WriteLine("loading type " + typeName + " from " + assembly.Name.Name); }
                typeList[typeVariable] = assembly.MainModule.Types.Where(t => t.Name.Equals(typeName)).Select(t => t).First();
            }
            else
            {
                if (verbose) { Console.WriteLine("loading type " + typeName + " from " + assemblyVariable); }
                typeList[typeVariable] = assemblyReferenceList[assemblyVariable].MainModule.Types.Where(t => t.Name.Equals(typeName)).Select(t => t).First();
            }
        }

        public void SetField(string fieldVariable, string typeName, string fieldName)
        {
            if (verbose) { Console.WriteLine("loading field " + fieldName + " from " + typeName); }
            fieldList[fieldVariable] = typeList[typeName].Fields.Where(t => t.Name.Equals(fieldName)).Select(t => t).First();
        }

        public void SetMethod(string methodVariable, string typeName, string methodName)
        {
            if (verbose) { Console.WriteLine("loading method " + methodName + " from " + typeName); }
            methodList[methodVariable] = typeList[typeName].Methods.Where(t => t.Name.Equals(methodName)).Select(t => t).First();
        }

        public void SetMethodReference(string methodReferenceVariable, string methodImport, string typeName, string methodName)
        {
            if (verbose) { Console.WriteLine("loading method reference " + methodReferenceVariable + " from " + typeName + " into " + methodImport); }
            methodReferenceList[methodReferenceVariable] = methodList[methodImport].Module.Import(typeList[typeName].Methods.Where(t => t.Name.Equals(methodName)).Select(t => t).First());
        }

        public void SetTypeProperty(string typeVariable, string typeAttribute, string typeValue)
        {
            List<string> typeAttributes = typeAttribute.Split(new[] { "|" }, StringSplitOptions.None).ToList();
            List<string> typeData = typeValue.Split(new[] { "|" }, StringSplitOptions.None).ToList();
            //shouldn't be too bad right ( ͡° ͜ʖ ͡°)
            for (int i = 0; i < typeAttributes.Count; i++)
            {
                typeList[typeVariable].GetType().GetProperty(typeAttributes[i]).SetValue(typeList[typeVariable], bool.Parse(typeData[i]), null);
            }
        }

        public void SetMethodProperty(string methodVariable, string methodAttribute, string methodValue)
        {
            List<string> methodAttributes = methodAttribute.Split(new[] { "|" }, StringSplitOptions.None).ToList();
            List<string> methodData = methodValue.Split(new[] { "|" }, StringSplitOptions.None).ToList();
            //shouldn't be too bad right ( ͡° ͜ʖ ͡°)
            for (int i = 0; i < methodAttributes.Count; i++)
            {
                methodList[methodVariable].GetType().GetProperty(methodAttributes[i]).SetValue(methodList[methodVariable], bool.Parse(methodData[i]), null);
            }
        }

        public void SetFieldProperty(string fieldVariable, string fieldAttribute, string fieldValue)
        {
            List<string> fieldAttributes = fieldAttribute.Split(new[] { "|" }, StringSplitOptions.None).ToList();
            List<string> fieldData = fieldValue.Split(new[] { "|" }, StringSplitOptions.None).ToList();
            //shouldn't be too bad right ( ͡° ͜ʖ ͡°)
            for (int i = 0; i < fieldAttributes.Count; i++)
            {
                fieldList[fieldVariable].GetType().GetProperty(fieldAttributes[i]).SetValue(fieldList[fieldVariable], bool.Parse(fieldData[i]), null);
            }
        }

        public void SetInstruction(string instructionVariable, string method, string location)
        {
            if (location == "!f")
            {
                instructionList[instructionVariable] = methodList[method].Body.Instructions.First();
            }
            else if (location == "!l")
            {
                instructionList[instructionVariable] = methodList[method].Body.Instructions.Last();
            }
            else
            {
                if (int.Parse(location) < 0)
                {
                    instructionList[instructionVariable] = methodList[method].Body.Instructions[methodList[method].Body.Instructions.Count + int.Parse(location)];
                } else
                {
                    instructionList[instructionVariable] = methodList[method].Body.Instructions[int.Parse(location)];
                }
            }
        }

        public void SetRunInstruction(string methodVariable, string insertType, string insertLocation, string ilCode, string argumentType, string argument)
        {
            ILProcessor processor_ciilr = methodList[methodVariable].Body.GetILProcessor();
            Instruction instruction_ciilr = null;
            switch (argumentType)
            {
                case "none":
                    instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(ilCode).GetValue(null));
                    break;
                case "byte":
                    instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(ilCode).GetValue(null), byte.Parse(argument));
                    break;
                case "double":
                    instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(ilCode).GetValue(null), double.Parse(argument));
                    break;
                case "float":
                    instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(ilCode).GetValue(null), float.Parse(argument));
                    break;
                case "instruction":
                    instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(ilCode).GetValue(null), instructionList[argument]);
                    break;
                case "int":
                    instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(ilCode).GetValue(null), int.Parse(argument));
                    break;
                case "long":
                    instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(ilCode).GetValue(null), long.Parse(argument));
                    break;
                case "method":
                    instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(ilCode).GetValue(null), methodReferenceList[argument]);
                    break;
                case "string":
                    instruction_ciilr = processor_ciilr.Create((OpCode)typeof(OpCodes).GetField(ilCode).GetValue(null), argument);
                    break;
            }
            if (verbose) { Console.WriteLine("creating instruction " + ilCode); }
            if (int.Parse(insertLocation) < 0) {
                insertLocation = (int.Parse(insertLocation) + processor_ciilr.Body.Instructions.Count).ToString();
            }
            if (insertType == "b")
            {
                processor_ciilr.InsertBefore(processor_ciilr.Body.Instructions[int.Parse(insertLocation)], instruction_ciilr);
            }
            else if (insertType == "a")
            {
                processor_ciilr.InsertAfter(processor_ciilr.Body.Instructions[int.Parse(insertLocation)], instruction_ciilr);
            }
            else if (insertType == "ap")
            {
                processor_ciilr.Append(instruction_ciilr);
            }
        }

        //Loops
        public void Loop_AddFieldWithVariable(string typeVariable, string fieldAttribute, string value, string name)
        {
            List<string> fieldAttributes = fieldAttribute.Split(new[] { "|" }, StringSplitOptions.None).ToList();
            List<string> variableData = name.Split(new[] { "|" }, StringSplitOptions.None).ToList();
            string[] variableDataArgs = variableData.Skip(1).ToArray();
            for (int j = 0; j < variableDataArgs.Length; j++)
            {
                if (variableDataArgs[j].StartsWith("$"))
                {
                    variableDataArgs[j] = variableList[value.Substring(1)].ToString();
                }
            }

            FieldAttributes fa = 0;
            foreach (string fieldAttrib in fieldAttributes)
            {
                fa |= (FieldAttributes)Enum.Parse(typeof(FieldAttributes), fieldAttrib);
            }

            FieldDefinition fd = new FieldDefinition(string.Format(variableData[0], variableDataArgs), fa, typeList[typeVariable]);
            int constant = 0;
            if (value.StartsWith("$"))
            {
                constant = variableList[value.Substring(1)];
            }
            else
            {
                constant = int.Parse(value);
            }

            if (!(typeList[typeVariable].Fields.Skip(1).Where(n => n.Constant.Equals(constant)).Any()))
            {
                if (verbose && superVerbose) { Console.WriteLine("adding " + string.Format(variableData[0], variableDataArgs) + " to " + typeVariable + " = " + constant); }
                typeList[typeVariable].Fields.Add(fd);
                fd.Constant = constant;
            }
        }

        //Variables
        public void SetVariable(string variableName, int variableValue)
        {
            variableList[variableName] = variableValue;
        }
    }
}
