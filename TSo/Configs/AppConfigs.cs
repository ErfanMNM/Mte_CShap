using System.Text;

namespace TSo.Configs;

public enum DataMode { Normal, Test, Hard }

public class AppConfigs
{
    private static string AppDataFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TSo");

    private static string ConfigFolder => Path.Combine(AppDataFolder, "Configs");
    private static string ConfigFile => Path.Combine(ConfigFolder, "App.ini");
    private static string UsersDbFolder => Path.Combine(AppDataFolder, "Users");
    private static string DatabaseFolder => Path.Combine(AppDataFolder, "Database");

    public static string UsersDbPath => Path.Combine(UsersDbFolder, "users.database");
    public static string QRDatadbPath => Path.Combine(DatabaseFolder, "QRDatabase.db");
    public static string ActiveUniqueDbPath => Path.Combine(DatabaseFolder, "ActiveUnique.db");
    public static string BatchHistoryDbPath => Path.Combine(DatabaseFolder, "Production", "batch_history.db");
    public static string LogPath => Path.Combine(AppDataFolder, "Logs", "ALL", "TSo.ptl");

    // === PLC ===
    public string? PLC_IP { get; set; }
    public int PLC_Port { get; set; } = 9600;
    public int PLC_Time_Refresh { get; set; } = 1000;
    public bool PLC_Test_Mode { get; set; } = true;
    public string? PLC_Ready_DM { get; set; } = "D16";
    public string? PLC_Deactive_DM { get; set; } = "D100";
    public string? PLC_Reject_DM { get; set; } = "D10";
    public string? PLC_Reset_Counter_DM { get; set; } = "D20";
    public string? PLC_Clear_DM { get; set; } = "D30";
    public string? PLC_Total_Count_DM { get; set; } = "D40";

    // === Camera ===
    public string? Camera_01_IP { get; set; } = "127.0.0.1";
    public int Camera_01_Port { get; set; } = 50001;

    // === Scanner ===
    public string? Handheld_COM_Port { get; set; } = "COM3";

    // === ERP ===
    public string? credentialERPPath { get; set; }
    public string? ERP_Sub_Inv { get; set; }
    public string? ERP_Org_Code { get; set; }
    public string? ERP_DatasetID { get; set; }
    public string? ERP_TableID { get; set; }
    public string? ERP_ProjectID { get; set; }

    // === Google Sheets ===
    public string? credentialPLCAddressPath { get; set; }
    public string? PLCAddressSheetId { get; set; }
    public string? PLCAddressSheetRange { get; set; }

    // === Production ===
    public string? Line_Name { get; set; }
    public string? production_list_path { get; set; }
    public string Data_Mode { get; set; } = "normal";

    // === Cloud & Backup ===
    public bool Cloud_Connection_Enabled { get; set; } = false;
    public int Cloud_Refresh_Interval_Minute { get; set; } = 60;
    public bool Cloud_Upload_Enabled { get; set; } = true;
    public bool Local_Backup_Enabled { get; set; } = true;
    public string? Backup_Folder_Path { get; set; }
    public string? AWS_Credential_Path { get; set; }

    // === App ===
    public bool AppHideEnable { get; set; } = false;
    public bool AppTwoFA_Enabled { get; set; } = false;
    public string Batch_Rule_Template { get; set; } = "{AN:6,15}-{N:6}-TOL{LINE}-{AN:1}";

    // === Production Speed ===
    public int Production_Speed_Mode { get; set; } = 0;
    public int Production_Speed_Sample_Count { get; set; } = 10;
    public int Production_Speed_Reset_Timeout { get; set; } = 30;

    // === Server ===
    public string Server_Host { get; set; } = "http://*:5000/";
    public string Server_Cors_Origins { get; set; } = "*";

    public void SetDefault()
    {
        PLC_IP = "127.0.0.1";
        PLC_Port = 9600;
        PLC_Time_Refresh = 1000;
        PLC_Test_Mode = false;
        Camera_01_IP = "127.0.0.1";
        Camera_01_Port = 49211;
        Handheld_COM_Port = "COM3";
        Line_Name = "Line 1";
        Data_Mode = "normal";
        Server_Host = "http://*:5000/";
    }

    public void Load()
    {
        SetDefault();

        if (!File.Exists(ConfigFile))
            return;

        try
        {
            var ini = ReadIniFile(ConfigFile);
            if (ini.TryGetValue("PLC", out var plc))
            {
                PLC_IP = plc.GetValueOrDefault("PLC_IP", PLC_IP);
                PLC_Port = int.TryParse(plc.GetValueOrDefault("PLC_Port", ""), out var p) ? p : PLC_Port;
                PLC_Time_Refresh = int.TryParse(plc.GetValueOrDefault("PLC_Time_Refresh", ""), out var pr) ? pr : PLC_Time_Refresh;
                PLC_Test_Mode = bool.TryParse(plc.GetValueOrDefault("PLC_Test_Mode", ""), out var pt) && pt;
                PLC_Ready_DM = plc.GetValueOrDefault("PLC_Ready_DM", PLC_Ready_DM);
                PLC_Deactive_DM = plc.GetValueOrDefault("PLC_Deactive_DM", PLC_Deactive_DM);
                PLC_Reject_DM = plc.GetValueOrDefault("PLC_Reject_DM", PLC_Reject_DM);
                PLC_Reset_Counter_DM = plc.GetValueOrDefault("PLC_Reset_Counter_DM", PLC_Reset_Counter_DM);
                PLC_Clear_DM = plc.GetValueOrDefault("PLC_Clear_DM", PLC_Clear_DM);
                PLC_Total_Count_DM = plc.GetValueOrDefault("PLC_Total_Count_DM", PLC_Total_Count_DM);
            }
            if (ini.TryGetValue("Camera", out var cam))
            {
                Camera_01_IP = cam.GetValueOrDefault("Camera_01_IP", Camera_01_IP);
                Camera_01_Port = int.TryParse(cam.GetValueOrDefault("Camera_01_Port", ""), out var cp) ? cp : Camera_01_Port;
            }
            if (ini.TryGetValue("Scanner", out var scan))
            {
                Handheld_COM_Port = scan.GetValueOrDefault("Handheld_COM_Port", Handheld_COM_Port);
            }
            if (ini.TryGetValue("ERP", out var erp))
            {
                credentialERPPath = erp.GetValueOrDefault("credentialERPPath", credentialERPPath);
                ERP_Sub_Inv = erp.GetValueOrDefault("ERP_Sub_Inv", ERP_Sub_Inv);
                ERP_Org_Code = erp.GetValueOrDefault("ERP_Org_Code", ERP_Org_Code);
                ERP_DatasetID = erp.GetValueOrDefault("ERP_DatasetID", ERP_DatasetID);
                ERP_TableID = erp.GetValueOrDefault("ERP_TableID", ERP_TableID);
                ERP_ProjectID = erp.GetValueOrDefault("ERP_ProjectID", ERP_ProjectID);
            }
            if (ini.TryGetValue("GoogleSheets", out var gs))
            {
                credentialPLCAddressPath = gs.GetValueOrDefault("credentialPLCAddressPath", credentialPLCAddressPath);
                PLCAddressSheetId = gs.GetValueOrDefault("PLCAddressSheetId", PLCAddressSheetId);
                PLCAddressSheetRange = gs.GetValueOrDefault("PLCAddressSheetRange", PLCAddressSheetRange);
            }
            if (ini.TryGetValue("Production", out var prod))
            {
                Line_Name = prod.GetValueOrDefault("Line_Name", Line_Name);
                production_list_path = prod.GetValueOrDefault("production_list_path", production_list_path);
                Data_Mode = prod.GetValueOrDefault("Data_Mode", Data_Mode);
            }
            if (ini.TryGetValue("Backup", out var bak))
            {
                Cloud_Connection_Enabled = bool.TryParse(bak.GetValueOrDefault("Cloud_Connection_Enabled", ""), out var cce) && cce;
                Cloud_Refresh_Interval_Minute = int.TryParse(bak.GetValueOrDefault("Cloud_Refresh_Interval_Minute", ""), out var crm) ? crm : Cloud_Refresh_Interval_Minute;
                Cloud_Upload_Enabled = bool.TryParse(bak.GetValueOrDefault("Cloud_Upload_Enabled", ""), out var cue) && cue;
                Local_Backup_Enabled = bool.TryParse(bak.GetValueOrDefault("Local_Backup_Enabled", ""), out var lbe) && lbe;
                Backup_Folder_Path = bak.GetValueOrDefault("Backup_Folder_Path", Backup_Folder_Path);
                AWS_Credential_Path = bak.GetValueOrDefault("AWS_Credential_Path", AWS_Credential_Path);
            }
            if (ini.TryGetValue("App", out var app))
            {
                AppHideEnable = bool.TryParse(app.GetValueOrDefault("AppHideEnable", ""), out var ahe) && ahe;
                AppTwoFA_Enabled = bool.TryParse(app.GetValueOrDefault("AppTwoFA_Enabled", ""), out var at2) && at2;
                Batch_Rule_Template = app.GetValueOrDefault("Batch_Rule_Template", Batch_Rule_Template);
            }
            if (ini.TryGetValue("Server", out var srv))
            {
                Server_Host = srv.GetValueOrDefault("Server_Host", Server_Host);
                Server_Cors_Origins = srv.GetValueOrDefault("Server_Cors_Origins", Server_Cors_Origins);
            }
        }
        catch { }
    }

    public void Save()
    {
        Directory.CreateDirectory(ConfigFolder);

        var sb = new StringBuilder();
        void Section(string name) => sb.AppendLine($"[{name}]");

        Section("PLC");
        sb.AppendLine($"PLC_IP={PLC_IP}");
        sb.AppendLine($"PLC_Port={PLC_Port}");
        sb.AppendLine($"PLC_Time_Refresh={PLC_Time_Refresh}");
        sb.AppendLine($"PLC_Test_Mode={PLC_Test_Mode}");
        sb.AppendLine($"PLC_Ready_DM={PLC_Ready_DM}");
        sb.AppendLine($"PLC_Deactive_DM={PLC_Deactive_DM}");
        sb.AppendLine($"PLC_Reject_DM={PLC_Reject_DM}");
        sb.AppendLine($"PLC_Reset_Counter_DM={PLC_Reset_Counter_DM}");
        sb.AppendLine($"PLC_Clear_DM={PLC_Clear_DM}");
        sb.AppendLine($"PLC_Total_Count_DM={PLC_Total_Count_DM}");

        Section("Camera");
        sb.AppendLine($"Camera_01_IP={Camera_01_IP}");
        sb.AppendLine($"Camera_01_Port={Camera_01_Port}");

        Section("Scanner");
        sb.AppendLine($"Handheld_COM_Port={Handheld_COM_Port}");

        Section("ERP");
        sb.AppendLine($"credentialERPPath={credentialERPPath}");
        sb.AppendLine($"ERP_Sub_Inv={ERP_Sub_Inv}");
        sb.AppendLine($"ERP_Org_Code={ERP_Org_Code}");
        sb.AppendLine($"ERP_DatasetID={ERP_DatasetID}");
        sb.AppendLine($"ERP_TableID={ERP_TableID}");
        sb.AppendLine($"ERP_ProjectID={ERP_ProjectID}");

        Section("GoogleSheets");
        sb.AppendLine($"credentialPLCAddressPath={credentialPLCAddressPath}");
        sb.AppendLine($"PLCAddressSheetId={PLCAddressSheetId}");
        sb.AppendLine($"PLCAddressSheetRange={PLCAddressSheetRange}");

        Section("Production");
        sb.AppendLine($"Line_Name={Line_Name}");
        sb.AppendLine($"production_list_path={production_list_path}");
        sb.AppendLine($"Data_Mode={Data_Mode}");

        Section("Backup");
        sb.AppendLine($"Cloud_Connection_Enabled={Cloud_Connection_Enabled}");
        sb.AppendLine($"Cloud_Refresh_Interval_Minute={Cloud_Refresh_Interval_Minute}");
        sb.AppendLine($"Cloud_Upload_Enabled={Cloud_Upload_Enabled}");
        sb.AppendLine($"Local_Backup_Enabled={Local_Backup_Enabled}");
        sb.AppendLine($"Backup_Folder_Path={Backup_Folder_Path}");
        sb.AppendLine($"AWS_Credential_Path={AWS_Credential_Path}");

        Section("App");
        sb.AppendLine($"AppHideEnable={AppHideEnable}");
        sb.AppendLine($"AppTwoFA_Enabled={AppTwoFA_Enabled}");
        sb.AppendLine($"Batch_Rule_Template={Batch_Rule_Template}");

        Section("Server");
        sb.AppendLine($"Server_Host={Server_Host}");
        sb.AppendLine($"Server_Cors_Origins={Server_Cors_Origins}");

        File.WriteAllText(ConfigFile, sb.ToString());
    }

    private static Dictionary<string, Dictionary<string, string>> ReadIniFile(string path)
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        string currentSection = "";
        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (line.StartsWith(';') || line.StartsWith('#')) continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line[1..^1];
                result[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            else if (currentSection.Length > 0)
            {
                var idx = line.IndexOf('=');
                if (idx > 0)
                {
                    var key = line[..idx].Trim();
                    var val = line[(idx + 1)..].Trim();
                    result[currentSection][key] = val;
                }
            }
        }
        return result;
    }
}
