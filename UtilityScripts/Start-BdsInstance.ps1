param (
    [Parameter(Mandatory=$true)]
    [string]$BdsDirectory,
    [Parameter(Mandatory=$false)]
    [int]$BackupIntervalMinutes=1440
)

function Invoke-ResilientProcess($cmd)
{
    $bdsProc = Start-Process -Wait -WorkingDirectory $BdsDirectory -FilePath $cmd;
    
    # Restart if killed
    Invoke-ResilientProcess;
}

function Backup-BdsWorld ()
{

}

# Check OS, as that determins how the process will be launched.
if ($IsLinux)
{
    $runBdsCommand = "LD_LIBRARY_PATH=. ./bedrock_server";
} 
elseif ($IsWindows)
{
    $runBdsCommand = "bedrock_server.exe";
}
else
{
    Write-Host "This script only supports Linux or Windows.";
    exit;
}

# Set a backup timer
$backupJob = Start-Job 
{
    Start-Sleep -Seconds $BackupIntervalMinutes * 60
    
    $bdsProc.StandardInput.WriteLine("save hold");
    Start-Sleep -Seconds 30
    $bdsProc.BeginOutputReadLine();

}

# Start the server
Invoke-ResilientProcess $runBdsCommand;
