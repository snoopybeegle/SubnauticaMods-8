using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UM4SN;
using UnityEngine;
using System.IO;

namespace AutoPatcherGUI
{
    public partial class apgui : Form
    {
        public apgui()
        {
            InitializeComponent();
        }

        public BackgroundWorker patchWork;

        private void button1_Click(object sender, EventArgs e)
        {
            patchWork = new BackgroundWorker();
            patchWork.DoWork += new DoWorkEventHandler(patchWorkRun);
            patchWork.RunWorkerAsync();
        }

        public void patchWorkRun(object sender, DoWorkEventArgs e)
        {
            if (File.Exists(@".\apversion"))
            {
                string currentPlastic = File.ReadAllLines(@".\..\..\SNUnmanagedData\plastic_status.ignore")[0];
                string currentAp = File.ReadAllLines(@".\apversion")[0];
                if (currentAp == currentPlastic)
                {
                    MessageBox.Show("You already have version " + currentAp + " installed!");
                    label2.Text = "autopatcher - Already up to date.";
                    return;
                }
            }
            const string AssemblyPath = @".\Assembly-CSharp.dll";
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(AssemblyPath);
            const string AssemblyPathFirstpass = @".\Assembly-CSharp-firstpass.dll";
            AssemblyDefinition assemblyFirstpass = AssemblyDefinition.ReadAssembly(AssemblyPathFirstpass);

            try {
                foreach (TypeDefinition type in assembly.MainModule.Types)
                {
                    if (type.Name == "StartScreen")
                    {
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if (method.Name == "Start")
                            {
                                label2.Text = "autopatcher - StartScreen::Start()";
                                ILProcessor ilProcessor = method.Body.GetILProcessor();
                                Instruction instruction = ilProcessor.Body.Instructions.First();
                                MethodReference Start = method.Module.Import(typeof(PluginLoader).GetMethod("StartModLoader", new Type[] { }));
                                ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Call, Start));
                            }
                        }
                    }
                    else if (type.Name == "MainMenuMusic")
                    {
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if (method.Name == "Stop")
                            {
                                label2.Text = "autopatcher - MainMenuMusic::Stop()";
                                ILProcessor ilProcessor = method.Body.GetILProcessor();
                                Instruction instruction = ilProcessor.Body.Instructions.First();
                                MethodReference Stop = method.Module.Import(typeof(PluginLoader).GetMethod("ModLoaderFinishedLoading", new Type[] { }));
                                ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Call, Stop));
                            }
                        }
                    }
                    else if (type.Name == "MainMenuLoadButton")
                    {
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if (method.Name == "Load")
                            {
                                label2.Text = "autopatcher - MainMenuLoadButton::Load()";
                                ILProcessor ilProcessor = method.Body.GetILProcessor();
                                Instruction instruction = ilProcessor.Body.Instructions.First();
                                MethodReference Load = method.Module.Import(typeof(PluginLoader).GetMethod("StartModLoaderLoading", new Type[] { }));
                                ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Call, Load));
                            }
                        }
                    }
                    else if (type.Name == "CellManager")
                    {
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if (method.Name == "RegisterGlobalEntity")
                            {
                                label2.Text = "autopatcher - CellManager::RegisterGlobalEntity()";
                                ILProcessor ilProcessor = method.Body.GetILProcessor();
                                MethodReference get_transform = method.Module.Import(typeof(UnityEngine.Component).GetMethod("get_transform", new Type[] { }));
                                MethodReference get_gameObject = method.Module.Import(typeof(UnityEngine.Component).GetMethod("get_gameObject", new Type[] { }));
                                MethodReference debug_log = method.Module.Import(typeof(PluginLoader).GetMethod("LargeWorldEntityUMLoad", new Type[] { typeof(GameObject) }));
                                ilProcessor.InsertBefore(ilProcessor.Body.Instructions[0], ilProcessor.Create(OpCodes.Ldarg_1));
                                ilProcessor.InsertAfter(ilProcessor.Body.Instructions[0], ilProcessor.Create(OpCodes.Call, debug_log));
                            }
                        }
                    }
                    else if (type.Name == "TechType")
                    {
                        bool shouldContinue = true;
                        if (ModifierKeys == Keys.Shift)
                        {
                            shouldContinue = !(MessageBox.Show("You are disabling TechTypes! They will disable all items!", "", MessageBoxButtons.OKCancel) == DialogResult.OK);
                        }
                        if (shouldContinue)
                        {
                            List<int> intList = new List<int>();
                            foreach (FieldDefinition fd in type.Fields)
                            {
                                if (fd.Name == "value__") { continue; }
                                intList.Add((int)fd.Constant);
                            }
                            for (int i = 0; i < 10000; i++)
                            {
                                if (!intList.Contains(i))
                                {
                                    if (i / 100 % 1 == 0)
                                    {
                                        label2.Text = "autopatcher - TechType create id " + i;
                                    }
                                    TypeReference techTypeTypeReference = assembly.MainModule.Types.Where(t => t.Name.Equals("TechType")).Select(t => t).First();
                                    Type techTypeType = Type.GetType(techTypeTypeReference.FullName + ", " + techTypeTypeReference.Module.Assembly.FullName);
                                    type.Fields.Add(new FieldDefinition("id_" + i, FieldAttributes.Public | FieldAttributes.HasDefault | FieldAttributes.Static | FieldAttributes.Literal, type.Module.Import(techTypeType)));
                                    type.Fields.Last().Constant = i;
                                }
                            }
                        }
                    }
                    else if (type.Name == "Constructable")
                    {
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if (method.Name == "InitResourceMap")
                            {
                                label2.Text = "autopatcher - TechTree::InitResourceMap()";
                                method.IsPublic = true;
                            }
                        }
                    }
                    else if (type.Name == "TechTree")
                    {
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if (method.Name == "InstantiateFromPrefab")
                            {
                                label2.Text = "autopatcher - TechTree::InstantiateFromPrefab()";
                                ILProcessor ilProcessor = method.Body.GetILProcessor();
                                Instruction instruction = ilProcessor.Body.Instructions[3];
                                MethodReference InstantiateFromPrefab = method.Module.Import(typeof(PluginLoader).GetMethod("GameObjectSpawned", new Type[] { typeof(GameObject) }));
                                ilProcessor.InsertAfter(instruction, ilProcessor.Create(OpCodes.Ldloc_0));
                                ilProcessor.InsertAfter(ilProcessor.Body.Instructions[4], ilProcessor.Create(OpCodes.Call, InstantiateFromPrefab));
                            }
                            else if (method.Name == "InitializeTechTree")
                            {
                                label2.Text = "autopatcher - TechTree::InitializeTechTree()";
                                ILProcessor ilProcessor = method.Body.GetILProcessor();
                                Instruction instruction = ilProcessor.Body.Instructions.Last();
                                MethodReference InitializeTechTree = method.Module.Import(typeof(PluginLoader).GetMethod("TechTreeLoaded"));
                                ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Call, InitializeTechTree));
                            }
                            else if (method.Name == "GetInternal")
                            {
                                label2.Text = "autopatcher - TechTree::GetInternal()";
                                method.IsPrivate = false;
                                method.IsPublic = true;
                            }
                            else if (method.Name == "New")
                            {
                                label2.Text = "autopatcher - TechTree::New()";
                                method.IsPrivate = false;
                                method.IsPublic = true;
                            }
                        }
                        foreach (FieldDefinition field in type.Fields)
                        {
                            if (field.Name == "root")
                            {
                                label2.Text = "autopatcher - TechTree::root";
                                field.IsPrivate = false;
                                field.IsPublic = true;
                                MethodReference root = field.Module.Import(typeof(HideInInspector).GetConstructors()[0]);
                                field.CustomAttributes.Add(new CustomAttribute(root));
                            }
                        }
                        foreach (TypeDefinition type_ in type.NestedTypes)
                        {
                            if (type_.Name == "InternalAction" || type_.Name == "TechTree.InternalAction")
                            {
                                label2.Text = "autopatcher - TechTree.InternalAction";
                                type_.IsNestedPrivate = false;
                                type_.IsNestedPublic = true;
                            }
                        }
                    }
                    else if (type.Name == "CraftData")
                    {
                        foreach (FieldDefinition field in type.Fields)
                        {
                            if (field.Name == "entClassTechTable")
                            {
                                label2.Text = "autopatcher - CraftData::entClassTechTable";
                                field.IsPrivate = false;
                                field.IsPublic = true;
                            }
                            else if (field.Name == "equipmentTypes")
                            {
                                label2.Text = "autopatcher - CraftData::equipmentTypes";
                                field.IsPrivate = false;
                                field.IsPublic = true;
                            }
                            else if (field.Name == "techMapping")
                            {
                                label2.Text = "autopatcher - CraftData::techMapping";
                                field.IsPrivate = false;
                                field.IsPublic = true;
                                field.IsInitOnly = false;
                            }
                            else if (field.Name == "groups")
                            {
                                label2.Text = "autopatcher - CraftData::groups";
                                field.IsPrivate = false;
                                field.IsPublic = true;
                            }
                            else if (field.Name == "buildables")
                            {
                                label2.Text = "autopatcher - CraftData::buildables";
                                field.IsPrivate = false;
                                field.IsPublic = true;
                            }
                        }
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if (method.Name == "PreparePrefabIDCache")
                            {
                                label2.Text = "autopatcher - PreparePrefabIDCache";
                                ILProcessor ilProcessor = method.Body.GetILProcessor();
                                Instruction instruction = ilProcessor.Body.Instructions.Last();
                                MethodReference PreparePrefabIDCache = method.Module.Import(typeof(PluginLoader).GetMethod("AfterCraftCreated", new Type[] { }));
                                ilProcessor.InsertBefore(instruction, ilProcessor.Create(OpCodes.Call, PreparePrefabIDCache));
                            }
                        }
                    }
                    else if (type.Name == "Language")
                    {
                        foreach (FieldDefinition field in type.Fields)
                        {
                            if (field.Name == "strings")
                            {
                                label2.Text = "autopatcher - Language::strings";
                                field.IsPrivate = false;
                                field.IsPublic = true;
                            }
                        }
                    }
                }

                foreach (TypeDefinition type in assemblyFirstpass.MainModule.Types)
                {
                    if (type.Name == "PrefabDatabase")
                    {
                        foreach (MethodDefinition method in type.Methods)
                        {
                            if (method.Name == "GetPrefabForFilename")
                            {
                                label2.Text = "autopatcher - PrefabDatabase::GetPrefabForFilename()";
                                ILProcessor ilProcessor = method.Body.GetILProcessor();
                                ilProcessor.Body.Instructions[5].OpCode = OpCodes.Brtrue_S;
                                ilProcessor.InsertAfter(ilProcessor.Body.Instructions[5], ilProcessor.Create(OpCodes.Ldarg_0));
                                MethodReference GetPrefabForFilename = method.Module.Import(typeof(TechTypeItems).GetMethod("cTGO", new Type[] { typeof(string) }));
                                ilProcessor.InsertAfter(ilProcessor.Body.Instructions[6], ilProcessor.Create(OpCodes.Call, GetPrefabForFilename));
                                ilProcessor.InsertAfter(ilProcessor.Body.Instructions[7], ilProcessor.Create(OpCodes.Stloc_0));
                                ilProcessor.InsertAfter(ilProcessor.Body.Instructions[8], ilProcessor.Create(OpCodes.Ldloc_0));
                                MethodReference op_Implicit = method.Module.Import(typeof(UnityEngine.Object).GetMethod("op_Implicit", new Type[] { typeof(UnityEngine.Object) }));
                                ilProcessor.InsertAfter(ilProcessor.Body.Instructions[9], ilProcessor.Create(OpCodes.Call, op_Implicit));
                                ilProcessor.InsertAfter(ilProcessor.Body.Instructions[10], ilProcessor.Create(OpCodes.Brtrue_S, ilProcessor.Body.Instructions[ilProcessor.Body.Instructions.Count - 2]));
                            }
                            if (method.Name == "LoadPrefabDatabase")
                            {
                                label2.Text = "autopatcher - PrefabDatabase::LoadPrefabDatabase()";
                                ILProcessor ilProcessor = method.Body.GetILProcessor();
                                MethodReference LoadPrefabDatabase = method.Module.Import(typeof(PluginLoader).GetMethod("DatabaseLoaded", new Type[] { }));
                                ilProcessor.InsertAfter(ilProcessor.Body.Instructions[31], ilProcessor.Create(OpCodes.Call, LoadPrefabDatabase));
                            }
                        }
                    }
                }
                label2.Text = "autopatcher - WRITING";

                string plastic = File.ReadAllLines(@".\..\..\SNUnmanagedData\plastic_status.ignore")[0];
                File.WriteAllText(@".\apversion", plastic);
                assembly.Write(@".\Assembly-CSharp.s.dll");
                assemblyFirstpass.Write(@".\Assembly-CSharp-firstpass.s.dll");
                Directory.CreateDirectory(@".\..\..\SNUnityMod");
                string dllBackup = DateTime.Now.ToString("MM_dd_yyyy_HH_mm");
                Directory.CreateDirectory(@".\DllBackups\" + dllBackup);
                File.Move(@".\Assembly-CSharp.dll", @".\DllBackups\" + dllBackup + @".\Assembly-CSharp.dll");
                File.Move(@".\Assembly-CSharp-firstpass.dll", @".\DllBackups\" + dllBackup + @".\Assembly-CSharp-firstpass.dll");
                File.Move(@".\Assembly-CSharp.s.dll", @".\Assembly-CSharp.dll");
                File.Move(@".\Assembly-CSharp-firstpass.s.dll", @".\Assembly-CSharp-firstpass.dll");
                MessageBox.Show("Wrote with no errors!");
                label2.Text = "autopatcher - Up to date!";
            }
            catch (Exception ex)
            {
                label2.Text = label2.Text + "\nautopatcher - error " + ex.Message;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("steam://rungameid/264710");
        }
    }
}
