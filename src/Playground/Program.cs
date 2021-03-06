﻿namespace ScTools.Playground
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using CodeWalker.GameFiles;

    using ScTools;
    using ScTools.GameFiles;
    using ScTools.ScriptLang;
    using ScTools.ScriptLang.Semantics;
    using ScTools.ScriptLang.Semantics.Symbols;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            LoadGTA5Keys();
            DoTest();
        }

        private static void LoadGTA5Keys()
        {
            string path = ".\\Keys";
            GTA5Keys.PC_AES_KEY = File.ReadAllBytes(path + "\\gtav_aes_key.dat");
            GTA5Keys.PC_NG_KEYS = CryptoIO.ReadNgKeys(path + "\\gtav_ng_key.dat");
            GTA5Keys.PC_NG_DECRYPT_TABLES = CryptoIO.ReadNgTables(path + "\\gtav_ng_decrypt_tables.dat");
            GTA5Keys.PC_NG_ENCRYPT_TABLES = CryptoIO.ReadNgTables(path + "\\gtav_ng_encrypt_tables.dat");
            GTA5Keys.PC_NG_ENCRYPT_LUTs = CryptoIO.ReadNgLuts(path + "\\gtav_ng_encrypt_luts.dat");
            GTA5Keys.PC_LUT = File.ReadAllBytes(path + "\\gtav_hash_lut.dat");
        }

        const string Code = @"
SCRIPT_NAME test_script

NATIVE PROC WAIT(INT ms)
NATIVE FUNC INT GET_GAME_TIMER()
NATIVE PROC BEGIN_TEXT_COMMAND_DISPLAY_TEXT(STRING text)
NATIVE PROC END_TEXT_COMMAND_DISPLAY_TEXT(FLOAT x, FLOAT y, INT p2)
NATIVE PROC ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME(STRING text)
NATIVE PROC ADD_TEXT_COMPONENT_INTEGER(INT value)
NATIVE PROC ADD_TEXT_COMPONENT_FLOAT(FLOAT value, INT decimalPlaces)
NATIVE FUNC FLOAT TIMESTEP()
NATIVE FUNC BOOL IS_CONTROL_PRESSED(INT padIndex, INT control)
NATIVE FUNC FLOAT VMAG(VEC3 v)
NATIVE FUNC FLOAT VMAG2(VEC3 v)
NATIVE FUNC FLOAT VDIST(VEC3 v1, VEC3 v2)
NATIVE FUNC FLOAT VDIST2(VEC3 v1, VEC3 v2)
NATIVE FUNC VEC3 GET_GAMEPLAY_CAM_COORD()
NATIVE PROC DELETE_PED(PED_INDEX& handle)
NATIVE FUNC PED_INDEX PLAYER_PED_ID()
NATIVE FUNC VEHICLE_INDEX GET_VEHICLE_PED_IS_IN(PED_INDEX ped, BOOL includeLastVehicle)
NATIVE FUNC BOOL DOES_ENTITY_EXIST(ENTITY_INDEX entity)
NATIVE FUNC INT GET_RANDOM_INT_IN_RANGE(INT startRange, INT endRange)
NATIVE PROC SET_VEHICLE_CUSTOM_PRIMARY_COLOUR(VEHICLE_INDEX vehicle, INT r, INT g, INT b)

PROC MAIN()
    WHILE TRUE
        WAIT(100)

        VEHICLE_INDEX veh = GET_VEHICLE_PED_IS_IN(PLAYER_PED_ID(), TRUE)

        IF DOES_ENTITY_EXIST(veh.base)
            SET_VEHICLE_CUSTOM_PRIMARY_COLOUR(veh, GET_RANDOM_INT_IN_RANGE(0, 255), GET_RANDOM_INT_IN_RANGE(0, 255), GET_RANDOM_INT_IN_RANGE(0, 255))
        ENDIF

    ENDWHILE
ENDPROC

PROC DRAW_FLOAT(FLOAT x, FLOAT y, FLOAT v)
    BEGIN_TEXT_COMMAND_DISPLAY_TEXT('NUMBER')
    ADD_TEXT_COMPONENT_FLOAT(v, 2)
    END_TEXT_COMMAND_DISPLAY_TEXT(x, y, 0)
ENDPROC
";

        public static void DoTest()
        {
            //NativeDB.Fetch(new Uri("https://raw.githubusercontent.com/alloc8or/gta5-nativedb-data/master/natives.json"), "ScriptHookV_1.0.2060.1.zip")
            //    .ContinueWith(t => File.WriteAllText("nativedb.json", t.Result.ToJson()))
            //    .Wait();

            var nativeDB = NativeDB.FromJson(File.ReadAllText("nativedb.json"));

            using var reader = new StringReader(Code);
            var comp = new Compilation { NativeDB = nativeDB };
            comp.SetMainModule(reader);
            comp.Compile();
            File.WriteAllText("test_script.ast.txt", comp.MainModule.GetAstDotGraph());

            var d = comp.GetAllDiagnostics();
            var symbols = comp.MainModule.SymbolTable;
            Console.WriteLine($"Errors:   {d.HasErrors} ({d.Errors.Count()})");
            Console.WriteLine($"Warnings: {d.HasWarnings} ({d.Warnings.Count()})");
            foreach (var diagnostic in d.AllDiagnostics)
            {
                diagnostic.Print(Console.Out);
            }

            foreach (var s in symbols.Symbols)
            {
                if (s is TypeSymbol t && t.Type is StructType struc)
                {
                    Console.WriteLine($"  > '{t.Name}' Size = {struc.SizeOf}");
                }
            }

            Console.WriteLine();
            new Dumper(comp.CompiledScript).Dump(Console.Out, true, true, true, true, true);

            YscFile ysc = new YscFile
            {
                Script = comp.CompiledScript
            };

            string outputPath = "test_script.ysc";
            byte[] data = ysc.Save(Path.GetFileName(outputPath));
            File.WriteAllBytes(outputPath, data);

            outputPath = Path.ChangeExtension(outputPath, "unencrypted.ysc");
            data = ysc.Save();
            File.WriteAllBytes(outputPath, data);
            ;
        }
    }
}
