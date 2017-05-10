using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace UM4SN.Cecil
{
    public class MonoInjector
    {        
        //https://groups.google.com/forum/#!topic/mono-cecil/uoMLJEZrQ1Q
        public static MethodDefinition CopyMethod(TypeDefinition copyToTypedef, MethodDefinition sourceMethod)
        {

            ModuleDefinition targetModule = copyToTypedef.Module;

            // create a new MethodDefinition; all the content of sourceMethod will be copied to this new MethodDefinition

            MethodDefinition targetMethod = new MethodDefinition(sourceMethod.Name, sourceMethod.Attributes, targetModule.Import(sourceMethod.ReturnType));


            // Copy the parameters; 
            foreach (ParameterDefinition p in sourceMethod.Parameters)
            {
                ParameterDefinition nP = new ParameterDefinition(p.Name, p.Attributes, targetModule.Import(p.ParameterType));
                targetMethod.Parameters.Add(nP);
            }

            // copy the body
            MethodBody nBody = targetMethod.Body;
            MethodBody oldBody = sourceMethod.Body;

            nBody.InitLocals = oldBody.InitLocals;

            // copy the local variable definition
            foreach (VariableDefinition v in oldBody.Variables)
            {
                VariableDefinition nv = new VariableDefinition(v.Name, targetModule.Import(v.VariableType));
                nBody.Variables.Add(nv);
            }

            // copy the IL; we only need to take care of reference and method definitions
            Collection<Instruction> col = nBody.Instructions;
            foreach (Instruction i in oldBody.Instructions)
            {
                ILProcessor ilProcessor = nBody.GetILProcessor();
                object operand = i.Operand;
                if (operand == null)
                {
                    col.Add(ilProcessor.Create(i.OpCode));
                    continue;
                }

                // for any methodef that this method calls, we will copy it

                if (operand is MethodDefinition)
                {
                    MethodDefinition dmethod = operand as MethodDefinition;
                    MethodDefinition newMethod = CopyMethod(copyToTypedef, dmethod);

                    col.Add(ilProcessor.Create(i.OpCode, newMethod));
                    continue;
                }

                // for member reference, import it
                if (operand is FieldReference)
                {
                    FieldReference fref = operand as FieldReference;
                    FieldReference newf = targetModule.Import(fref);
                    col.Add(ilProcessor.Create(i.OpCode, newf));
                    continue;
                }
                if (operand is TypeReference)
                {
                    TypeReference tref = operand as TypeReference;
                    TypeReference newf = targetModule.Import(tref);
                    col.Add(ilProcessor.Create(i.OpCode, newf));
                    continue;
                }
                if (operand is TypeDefinition)
                {
                    TypeDefinition tdef = operand as TypeDefinition;
                    TypeReference newf = targetModule.Import(tdef);
                    col.Add(ilProcessor.Create(i.OpCode, newf));
                    continue;
                }
                if (operand is MethodReference)
                {
                    MethodReference mref = operand as MethodReference;
                    MethodReference newf = targetModule.Import(mref);
                    col.Add(ilProcessor.Create(i.OpCode, newf));
                    continue;
                }

                // we don't need to do any processing on the operand
                col.Add(i);
            }

            // copy the exception handler blocks

            foreach (ExceptionHandler eh in oldBody.ExceptionHandlers)
            {
                ExceptionHandler neh = new ExceptionHandler(eh.HandlerType);
                neh.CatchType = targetModule.Import(eh.CatchType);

                // we need to setup neh.Start and End; these are instructions; we need to locate it in the source by index
                if (eh.TryStart != null)
                {
                    int idx = oldBody.Instructions.IndexOf(eh.TryStart);
                    neh.TryStart = col[idx];
                }
                if (eh.TryEnd != null)
                {
                    int idx = oldBody.Instructions.IndexOf(eh.TryEnd);
                    neh.TryEnd = col[idx];
                }

                nBody.ExceptionHandlers.Add(neh);
            }

            // Add this method to the target typedef
            copyToTypedef.Methods.Add(targetMethod);
            targetMethod.DeclaringType = copyToTypedef;
            return targetMethod;
        }
    }
}
