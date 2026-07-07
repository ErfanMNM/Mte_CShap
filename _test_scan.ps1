# Full test: Login -> Select PO -> Wait Ready -> Start -> Scan Carton
$loginBody = @{
    username = "admin"
    password = "Admin@123"
}

Write-Host "=== 1. Login ==="
$login = Invoke-RestMethod -Uri "http://127.0.0.1:9999/api/auth/login" -Method Post -ContentType "application/json" -Body ($loginBody | ConvertTo-Json) -SessionVariable sv -TimeoutSec 10
Write-Host "Login OK: $($login.user.username)"

# Wait for state to become Ready (auto-transition after login)
Write-Host "`n=== 2. Waiting for state machine (NeedLogin -> Checking -> LoadPO -> Ready) ==="
$timeout = 15
$elapsed = 0
$state = ""
while ($elapsed -lt $timeout) {
    Start-Sleep -Milliseconds 200
    $elapsed += 0.2
    $status = Invoke-RestMethod -Uri "http://127.0.0.1:9999/api/production/status" -WebSession $sv -TimeoutSec 5
    $state = $status.state
    Write-Host "  [$elapsed s] State: $state"
    if ($state -eq "Ready") { break }
    if ($state -eq "Error") { Write-Host "ERROR! Check logs."; break }
}

if ($state -eq "Ready") {
    Write-Host "`n=== 3. Start Production ==="
    $start = Invoke-RestMethod -Uri "http://127.0.0.1:9999/api/production/start" -Method Post -ContentType "application/json" -WebSession $sv -TimeoutSec 10
    Write-Host "Start: $($start | ConvertTo-Json)"

    # Wait for Running or Paused
    Start-Sleep -Milliseconds 500
    $status2 = Invoke-RestMethod -Uri "http://127.0.0.1:9999/api/production/status" -WebSession $sv -TimeoutSec 5
    Write-Host "After Start - State: $($status2.state)"

    if ($status2.state -eq "Running" -or $status2.state -eq "Paused") {
        Write-Host "`n=== 4. Scan Carton ==="
        $scanBody = @{
            cartonCode = "CTN-TEST-004-001"
            machineName = "PDA-TEST"
            scannedAt = "2026-07-07 16:40:00"
            mode = "scan"
        }

        try {
            $scan = Invoke-RestMethod -Uri "http://127.0.0.1:9999/api/carton/scan" -Method Post -ContentType "application/json" -Body ($scanBody | ConvertTo-Json) -WebSession $sv -TimeoutSec 10
            Write-Host "Scan Response: $($scan | ConvertTo-Json)"
        } catch {
            # Fallback: use WebRequest to get raw response
            Write-Host "Invoke-RestMethod failed, trying Invoke-WebRequest..."
            $resp = Invoke-WebRequest -Uri "http://127.0.0.1:9999/api/carton/scan" -Method Post -ContentType "application/json" -Body ($scanBody | ConvertTo-Json) -WebSession $sv -TimeoutSec 10
            Write-Host "Raw Response: $($resp.Content)"
        }

        Write-Host "`n=== 5. Check Carton Info ==="
        $info = Invoke-RestMethod -Uri "http://127.0.0.1:9999/api/carton/CTN-TEST-004-001/info" -WebSession $sv -TimeoutSec 10
        Write-Host "Info: $($info | ConvertTo-Json)"
    }
}

Write-Host "`n=== Done ==="
